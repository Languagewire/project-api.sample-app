/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using System.Net;
using FluentAssertions;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.IntegrationTests.Clients;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.IntegrationTests.Handlers;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.IntegrationTests;

/// <summary>
///     This tests are to make sure the mixing of the different Polly policies for retrials are behaving as we expect.
/// </summary>
public class RetryPoliciesTests
{
    private const string RetriesSeconds = "1,2,4";
    private HttpMessageHandlerWithRetries _messageHandler = null!;

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task WHEN_success_response_is_received_THEN_no_retrials_are_made(HttpStatusCode statusCode)
    {
        _messageHandler = new HttpMessageHandlerWithRetries(statusCode);
        var testApiClient = SetupClientAsInDependencyInjection(_messageHandler);

        var response = await testApiClient.GetSomeDataAsync();

        response.StatusCode.Should().Be(statusCode);
        _messageHandler.NumberOfRequests.Should().Be(1);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task WHEN_transient_errors_THEN_regular_retry_policy_applies(HttpStatusCode statusCode)
    {
        _messageHandler = new HttpMessageHandlerWithRetries(statusCode, statusCode, statusCode, statusCode);
        var testApiClient = SetupClientAsInDependencyInjection(_messageHandler);

        var response = await testApiClient.GetSomeDataAsync();

        response.StatusCode.Should().Be(statusCode);
        _messageHandler.NumberOfRequests.Should().Be(4);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 2, lowerLimit: 800, upperLimit: 1200);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 3, lowerLimit: 1800, upperLimit: 2200);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 4, lowerLimit: 3800, upperLimit: 4200);
    }

    [Fact]
    public async Task WHEN_transient_errors_and_then_ok_THEN_regular_policy_applies_until_ok()
    {
        _messageHandler = new HttpMessageHandlerWithRetries(
            HttpStatusCode.BadRequest,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK);
        var testApiClient = SetupClientAsInDependencyInjection(_messageHandler);

        var response = await testApiClient.GetSomeDataAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _messageHandler.NumberOfRequests.Should().Be(3);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 2, lowerLimit: 800, upperLimit: 1200);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 3, lowerLimit: 1800, upperLimit: 2200);
    }

    [Fact]
    public async Task WHEN_exceptions_received_and_then_ok_THEN_regular_policy_applies_until_ok()
    {
        _messageHandler = new HttpMessageHandlerWithRetries(null, null, HttpStatusCode.OK);
        var testApiClient = SetupClientAsInDependencyInjection(_messageHandler);

        var response = await testApiClient.GetSomeDataAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _messageHandler.NumberOfRequests.Should().Be(3);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 2, lowerLimit: 800, upperLimit: 1200);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 3, lowerLimit: 1800, upperLimit: 2200);
    }

    [Fact]
    public async Task WHEN_TooManyRequests_response_is_received_twice_THEN_the_RetryAfter_policy_applies_only_once()
    {
        _messageHandler = new HttpMessageHandlerWithRetries(
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.TooManyRequests);
        var testApiClient = SetupClientAsInDependencyInjection(_messageHandler);

        var response = await testApiClient.GetSomeDataAsync();

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        _messageHandler.NumberOfRequests.Should().Be(2);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 2, lowerLimit: 4800, upperLimit: 5200);
    }

    [Fact]
    public async Task
        WHEN_TooManyRequests_and_then_transient_errors_and_then_ok_THEN_RetryAfter_policy_applies_and_then_the_regular_policy_applies_until_ok()
    {
        _messageHandler = new HttpMessageHandlerWithRetries(
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.BadRequest,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK);
        var testApiClient = SetupClientAsInDependencyInjection(_messageHandler);

        var response = await testApiClient.GetSomeDataAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _messageHandler.NumberOfRequests.Should().Be(4);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 2, lowerLimit: 4800, upperLimit: 5200);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 3, lowerLimit: 800, upperLimit: 1200);
        AssertRetryTimeBetween(_messageHandler.RequestTimes, requestIndex: 4, lowerLimit: 1800, upperLimit: 2200);
    }

    [Fact]
    public async Task
        WHEN_transient_error_then_TooManyRequests_and_then_transient_errors_and_then_ok_THEN_RetryAfter_do_not_affect_regular_policy()
    {
        _messageHandler = new HttpMessageHandlerWithRetries(
            HttpStatusCode.BadRequest,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK);
        var testApiClient = SetupClientAsInDependencyInjection(_messageHandler);

        var response = await testApiClient.GetSomeDataAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _messageHandler.NumberOfRequests.Should().Be(4);
        var firstRetryTime = _messageHandler.RequestTimes[2] - _messageHandler.RequestTimes[1];
        firstRetryTime.TotalMilliseconds.Should().BeGreaterThan(800).And.BeLessThan(1200);
        var secondRetryTime = _messageHandler.RequestTimes[3] - _messageHandler.RequestTimes[2];
        secondRetryTime.TotalMilliseconds.Should().BeGreaterThan(4800).And.BeLessThan(5200);
        var thirdRetryTime = _messageHandler.RequestTimes[4] - _messageHandler.RequestTimes[3];
        thirdRetryTime.TotalMilliseconds.Should().BeGreaterThan(1800).And.BeLessThan(2200);
    }

    private ITestApiClient SetupClientAsInDependencyInjection(HttpMessageHandler messageHandler)
    {
        var servicesCollection = new ServiceCollection();

        servicesCollection.AddHttpClient<ITestApiClient, TestApiClient>()
                          .ConfigurePrimaryHttpMessageHandler(() => messageHandler)
                          .AddRetryPolicy(RetriesSeconds)
                          .AddRetryAfterPolicy();

        using var serviceProvider = servicesCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITestApiClient>();
    }

    private static void AssertRetryTimeBetween(
        Dictionary<int, TimeSpan> messageHandlerRequestTimes,
        int requestIndex,
        int lowerLimit,
        int upperLimit)
    {
        var retryTime = messageHandlerRequestTimes[requestIndex] - messageHandlerRequestTimes[requestIndex - 1];
        retryTime.TotalMilliseconds.Should().BeGreaterThan(lowerLimit).And.BeLessThan(upperLimit);
    }
}