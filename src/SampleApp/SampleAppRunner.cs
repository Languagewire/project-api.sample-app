/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Services;
using Microsoft.Extensions.Logging;

namespace LanguageWire.SampleApp;

internal class SampleAppRunner
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger _logger;
    private readonly ITranslationsService _translationsService;

    public SampleAppRunner(ITranslationsService translationsService, ILogger<SampleAppRunner> logger)
    {
        _translationsService = translationsService;
        _logger              = logger;
    }

    public async Task RunAsync()
    {
        await DisplayOptions();
        await ProcessOptionSelectedAsync(Console.ReadKey());
    }

    private static async Task DisplayOptions()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));

        Console.WriteLine("Choose your option:");
        Console.WriteLine(" 1. Get basic data");
        Console.WriteLine(" 2. Get account data");
        Console.WriteLine(" 3. Create project wrongly and handle errors.");
        Console.WriteLine(" 4. Create project with template");
        Console.WriteLine(" 5. Poll project for translations");
        Console.WriteLine(" Esc OR Enter. Close application");
    }

    private async Task ProcessOptionSelectedAsync(ConsoleKeyInfo readKey)
    {
        while (readKey.Key is not ConsoleKey.Enter and not ConsoleKey.Escape)
        {
            switch (readKey.KeyChar)
            {
                case '1':
                    Console.WriteLine("\n Getting basic data...");

                    await GetBasicDataAsync(_cancellationTokenSource.Token);

                    await DisplayOptions();
                    readKey = Console.ReadKey();
                    continue;

                case '2':
                    Console.WriteLine("\n Getting basic data...");

                    await GetAccountDataAsync(_cancellationTokenSource.Token);

                    await DisplayOptions();
                    readKey = Console.ReadKey();
                    continue;

                case '3':
                    Console.WriteLine("\n Starting a wrong project creation...");
                    await StartWrongProjectCreation(_cancellationTokenSource.Token);

                    await DisplayOptions();
                    readKey = Console.ReadKey();
                    continue;

                case '4':
                    Console.WriteLine("\n Starting project creation...");
                    await StartProjectCreation(_cancellationTokenSource.Token);

                    await DisplayOptions();
                    readKey = Console.ReadKey();
                    continue;

                case '5':
                    Console.WriteLine("\n Polling project for translations...");
                    await PollTranslations(_cancellationTokenSource.Token);

                    await DisplayOptions();
                    readKey = Console.ReadKey();
                    continue;
                default:
                    readKey = Console.ReadKey();
                    continue;
            }
        }

        _cancellationTokenSource.Cancel();
    }

    private async Task StartWrongProjectCreation(CancellationToken cancellationToken)
    {
        try
        {
            await _translationsService.CreateProjectWronglyAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}", e.Message);
            await _translationsService.ProjectsCleanup(cancellationToken);
        }
    }

    private async Task StartProjectCreation(CancellationToken cancellationToken)
    {
        try
        {
            await _translationsService.CreateProjectWithFileAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}", e.Message);
            await _translationsService.ProjectsCleanup(cancellationToken);
        }
    }

    private async Task PollTranslations(CancellationToken cancellationToken)
    {
        try
        {
            await _translationsService.PollTranslationsAsync(waitingTime: GetWaitingTime(), cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}", e.Message);
        }
    }

    private async Task GetBasicDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _translationsService.GetBasicDataAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}", e.Message);
        }
    }

    private async Task GetAccountDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _translationsService.GetAccountDataAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}", e.Message);
        }
    }

    /// <summary>
    ///     For Raw MT projects we recommend polling every minute, for real projects from 1 to 6 hours
    /// </summary>
    /// <returns>TimeSpan to wait</returns>
    private static TimeSpan GetWaitingTime() => TimeSpan.FromMinutes(1);
}