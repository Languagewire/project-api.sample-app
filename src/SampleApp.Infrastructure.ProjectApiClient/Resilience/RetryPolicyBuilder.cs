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

internal static class RetryPolicyBuilder
{
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, string retries)
    {
        builder.AddTransientHttpErrorPolicy(
            policyBuilder => policyBuilder
                             .OrResult(
                                 message => !message.IsSuccessStatusCode
                                            && message.StatusCode != HttpStatusCode.TooManyRequests)
                             .WaitAndRetryAsync(
                                 retries.Split(',').Select(retryTime => TimeSpan.FromSeconds(float.Parse(retryTime)))));
        return builder;
    }
}