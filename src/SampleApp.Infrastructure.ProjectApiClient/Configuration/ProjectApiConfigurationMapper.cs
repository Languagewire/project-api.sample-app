/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using Microsoft.Extensions.Options;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Configuration;

public class ProjectApiConfigurationMapper : IConfigureOptions<ProjectApiConfiguration>
{
    public void Configure(ProjectApiConfiguration options)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.ApiUrl);
        ArgumentException.ThrowIfNullOrEmpty(options.AuthorityUrl);
        ArgumentException.ThrowIfNullOrEmpty(options.ClientId);
        ArgumentException.ThrowIfNullOrEmpty(options.ClientSecret);
    }
}