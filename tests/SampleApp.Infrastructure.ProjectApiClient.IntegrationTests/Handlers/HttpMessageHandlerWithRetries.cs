/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using System.Net;
using System.Net.Http.Headers;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.IntegrationTests.Handlers;

public class HttpMessageHandlerWithRetries : HttpMessageHandler
{
    private const int RetryAfterResponseHeaderSeconds = 5;
    private readonly List<HttpResponseMessage?> _responseMessages;

    public HttpMessageHandlerWithRetries(params HttpStatusCode?[] statusCodes)
    {
        var responseMessages = statusCodes.Select(
                                              statusCode => statusCode.HasValue
                                                  ? BuildHttpResponseMessage(statusCode.Value)
                                                  : null)
                                          .ToList();
        _responseMessages = responseMessages;
    }

    public int NumberOfRequests { get; private set; }
    public Dictionary<int, TimeSpan> RequestTimes { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        NumberOfRequests++;
        RequestTimes[NumberOfRequests] = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);

        if (_responseMessages[NumberOfRequests - 1] is null)
            throw new HttpRequestException("Test exception");

        var response = _responseMessages[NumberOfRequests - 1]!;
        response.RequestMessage = request;
        return await Task.FromResult(response);
    }

    private static HttpResponseMessage BuildHttpResponseMessage(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode);

        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            response.Headers.RetryAfter =
                new RetryConditionHeaderValue(TimeSpan.FromSeconds(RetryAfterResponseHeaderSeconds));
        }

        return response;
    }
}