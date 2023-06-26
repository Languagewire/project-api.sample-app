/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using IdentityModel.Client;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Configuration;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        const string projectApiTokenHandlerName = "ProjectApiTokenHandler";

        services.Configure<ProjectApiConfiguration>(configuration.GetSection("LanguageWireProjectApi"))
                .ConfigureOptions<ProjectApiConfigurationMapper>()
                .AddAccessTokenManagement(
                    options =>
                    {
                        options.Client.Clients.Add(
                            projectApiTokenHandlerName,
                            value: new ClientCredentialsTokenRequest
                            {
                                Address =
                                    $"{configuration["LanguageWireProjectApi:AuthorityUrl"]}protocol/openid-connect/token",
                                ClientId     = configuration["LanguageWireProjectApi:ClientId"],
                                ClientSecret = configuration["LanguageWireProjectApi:ClientSecret"]
                            });
                    })
                .ConfigureBackchannelHttpClient()
                .AddRetryPolicy(configuration["HttpClientsResiliency:RetriesTimes"]!);

        services.AddHttpClient<IProjectApiClient, ProjectApiClient>()
                .AddClientAccessTokenHandler(projectApiTokenHandlerName)
                .AddRetryPolicy(configuration["HttpClientsResiliency:RetriesTimes"]!)
                .AddRetryAfterPolicy();

        return services;
    }
}