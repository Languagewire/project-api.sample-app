/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
namespace LanguageWire.SampleApp.Configuration;

internal record AuthenticationConfiguration
{
    public Uri AuthorityUrl { get; set; } = new(string.Empty, UriKind.Relative);
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
}