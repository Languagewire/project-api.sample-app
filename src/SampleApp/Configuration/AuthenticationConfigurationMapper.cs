/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageWire.SampleApp.Configuration;

internal class AuthenticationConfigurationMapper : IConfigureOptions<AuthenticationConfiguration>
{
    private readonly ProjectApiConfiguration _projectApiConfiguration;

    public AuthenticationConfigurationMapper(IOptions<ProjectApiConfiguration> appSettingsOptions)
        => _projectApiConfiguration = appSettingsOptions.Value;

    public void Configure(AuthenticationConfiguration options)
    {
        ArgumentNullException.ThrowIfNull(_projectApiConfiguration.AuthorityUrl);
        ArgumentNullException.ThrowIfNull(_projectApiConfiguration.ClientId);
        ArgumentNullException.ThrowIfNull(_projectApiConfiguration.ClientSecret);

        options.AuthorityUrl = new Uri(_projectApiConfiguration.AuthorityUrl);
        options.ClientId     = _projectApiConfiguration.ClientId;
        options.ClientSecret = _projectApiConfiguration.ClientSecret;
    }
}