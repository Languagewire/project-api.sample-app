/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Configuration;

public record ProjectApiConfiguration
{
    public string ApiUrl { get; init; } = default!;
    public string AuthorityUrl { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
}