/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.IntegrationTests.Clients;

internal interface ITestApiClient
{
    Task<HttpResponseMessage> GetSomeDataAsync();
}