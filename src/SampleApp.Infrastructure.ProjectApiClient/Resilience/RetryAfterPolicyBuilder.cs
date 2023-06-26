/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Resilience;

internal static class RetryAfterPolicyBuilder
{
    public static IHttpClientBuilder AddRetryAfterPolicy(this IHttpClientBuilder builder)
    {
        builder.AddPolicyHandler(
            Policy.HandleResult<HttpResponseMessage>(
                      responseMessage => responseMessage.StatusCode == HttpStatusCode.TooManyRequests)
                  .WaitAndRetryAsync(
                      retryCount: 1,
                      (_, response, _) => GetRetryAfterResponseHeader(response),
                      (
                          _,
                          _,
                          _,
                          _) => Task.CompletedTask));
        return builder;
    }

    private static TimeSpan GetRetryAfterResponseHeader(DelegateResult<HttpResponseMessage> response)
    {
        const int defaultDelaySeconds = 1;

        var retryAfterHeaderValue = response.Result.Headers.RetryAfter;

        return retryAfterHeaderValue?.Delta ?? TimeSpan.FromSeconds(defaultDelaySeconds);
    }
}