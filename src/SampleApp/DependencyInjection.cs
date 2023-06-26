/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient;
using LanguageWire.SampleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace LanguageWire.SampleApp;

internal static class DependencyInjection
{
    public static ServiceCollection Initialize()
    {
        var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                                                      .AddJsonFile("appsettings.json", optional: false)
                                                      .AddJsonFile("appsettings.Development.json", optional: true)
                                                      .Build();

        return ConfigureServices(configuration);
    }

    private static ServiceCollection ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddLogging(ConfigureLogging)
                .AddInfrastructure(configuration)
                .AddTransient<ITranslationsService, TranslationsService>()
                .AddTransient<SampleAppRunner>();

        return services;
    }

    private static void ConfigureLogging(this ILoggingBuilder loggingBuilder)
    {
        var logger = new LoggerConfiguration().MinimumLevel.Information()
                                              .MinimumLevel
                                              .Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                                              .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}")
                                              .CreateLogger();

        loggingBuilder.ClearProviders().AddSerilog(logger, dispose: true);
    }
}