/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Exceptions;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Mappers;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Requests;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;
using Microsoft.Extensions.Logging;

namespace LanguageWire.SampleApp.Services;

internal class TranslationsService : ITranslationsService
{
    private const string FileTranslatedStatusName = "Finished";
    private readonly ILogger _logger;
    private readonly IProjectApiClient _projectApiClient;

    public TranslationsService(IProjectApiClient projectApiClient, ILogger<TranslationsService> logger)
    {
        _projectApiClient = projectApiClient;
        _logger           = logger;
    }

    public async Task GetBasicDataAsync(CancellationToken cancellationToken)
    {
        var services = await _projectApiClient.GetServicesAsync(cancellationToken);
        _logger.LogInformation(" Services:");
        PrintResultList(services);

        var workAreas = await _projectApiClient.GetWorkAreasAsync(cancellationToken);
        _logger.LogInformation(" Work Areas:");
        PrintResultList(workAreas);
    }

    public async Task GetAccountDataAsync(CancellationToken cancellationToken)
    {
        var translationMemories = await _projectApiClient.GetTranslationMemoriesAsync(cancellationToken);
        _logger.LogInformation(" Translation memories:");
        PrintResultList(translationMemories);

        var invoicingAccounts = await _projectApiClient.GetInvoicingAccountsAsync(cancellationToken);
        _logger.LogInformation(" Invoicing accounts:");
        PrintResultList(invoicingAccounts);

        var users = await _projectApiClient.GetUsersAsync(cancellationToken);
        _logger.LogInformation(" Users:");
        PrintResultList(users.MapToIdAndNameResponseList());

        var projectTemplates = await _projectApiClient.GetProjectTemplatesAsync(cancellationToken);
        _logger.LogInformation(" Project templates:");
        PrintResultList(projectTemplates);
    }

    public async Task CreateProjectWronglyAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("    Try to create a project with uncompleted information");
        try
        {
            var wrongProject = BuildWrongProjectCreationRequest();
            await _projectApiClient.CreateProjectAsync(wrongProject, cancellationToken);
        }
        catch (ProjectApiException ex)
        {
            ex.Errors.ForEach(
                e => _logger.LogError(
                    "          Received error {Code}: {Message}. Suggested hint: {Hint}",
                    e.Code,
                    e.Message,
                    e.Hint));
        }
    }

    public async Task CreateProjectWithFileAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("    Fetching information necessary for the project creation");
        // In some cases you might just fetch once and then use the same values over and over from configuration or even hardcoded.
        var users            = await _projectApiClient.GetUsersAsync(cancellationToken);
        var projectTemplates = await _projectApiClient.GetProjectTemplatesAsync(cancellationToken);

        try
        {
            _logger.LogInformation("    Creating a project using a template with the minimal data");
            var projectToBeCreated = BuildGoodProjectCreationRequest(projectTemplates, users);
            var project            = await _projectApiClient.CreateProjectAsync(projectToBeCreated, cancellationToken);

            _logger.LogInformation("     Adding a file to the project");
            await _projectApiClient.AddFileAsync(project!.Id, cancellationToken);

            _logger.LogInformation("     Starting the project");
            await _projectApiClient.StartProjectAsync(project.Id, cancellationToken);
        }
        catch (ProjectApiException ex)
        {
            _logger.LogError(ex.InnerException, "    {ExceptionMessage}", ex.Message);
        }
    }

    public async Task PollTranslationsAsync(TimeSpan waitingTime, CancellationToken cancellationToken)
    {
        const int maxIterations       = 5;
        var       retries             = 0;
        var       processedProjectIds = GetAlreadyProcessedProjects();

        do
        {
            var finishedProjects = await _projectApiClient.GetProjectsByFiltersAsync("Finished", cancellationToken);

            foreach (var project in finishedProjects.Projects.Where(
                         project => !processedProjectIds.Contains(project.Id)))
            {
                var translatedFiles = await PollTranslationsUntilFinished(project.Id, cancellationToken);

                if (translatedFiles is null)
                    continue;

                await ProcessTranslatedFiles(project, translatedFiles, cancellationToken);
                processedProjectIds.Add(project.Id);
            }

            retries++;
            _logger.LogInformation("     Waiting for next batch polling");
            await Task.Delay(waitingTime, cancellationToken);
        } while (retries < maxIterations);

        _logger.LogInformation("     Polling finished");
    }

    public async Task ProjectsCleanup(CancellationToken cancellationToken)
    {
        var projects = await _projectApiClient.GetProjectsByFiltersAsync("Draft", cancellationToken);

        foreach (var project in projects.Projects)
        {
            _logger.LogInformation("     Cleaning project...");
            await _projectApiClient.DeleteProjectAsync(project.Id, cancellationToken);
            _logger.LogInformation("     Project cleaned");
        }
    }

    private static ProjectCreationRequest BuildWrongProjectCreationRequest()
        => new(
            "Wrong translation project",
            Briefing: "This is just to showcase the error handling. This project request is missing the user id "
                      + "which is required and a template id which should be specified unless we then specify "
                      + "a service id, translation memory id, invoicing account id, and work area id.",
            BriefingForExperts: default,
            PurchaseOrderNumber: default,
            Deadline: DateTime.Today.AddDays(2),
            TemplateId: default,
            ServiceId: default,
            TranslationMemoryId: default,
            InvoicingAccountId: default,
            UserId: default,
            WorkAreaId: default,
            ExternalId: Guid.NewGuid().ToString());

    private static ProjectCreationRequest BuildGoodProjectCreationRequest(
        IEnumerable<IdAndNameResponse> projectTemplates,
        IEnumerable<UserResponse> users)
        => new(
            "Example project from sample app",
            Briefing: default,
            BriefingForExperts: default,
            PurchaseOrderNumber: default,
            Deadline: DateTime.Today.AddDays(2),
            projectTemplates.First().Id,
            ServiceId: default,
            TranslationMemoryId: default,
            InvoicingAccountId: default,
            users.First().Id,
            WorkAreaId: default,
            ExternalId: Guid.NewGuid().ToString());

    /// <summary>
    ///     A placeholder to query your already processed projects on your system
    /// </summary>
    /// <returns>Empty list of ids as an example</returns>
    private static List<int> GetAlreadyProcessedProjects() => new();

    private async Task ProcessTranslatedFiles(
        ProjectItem project,
        IEnumerable<Guid> filesToDownload,
        CancellationToken cancellationToken)
    {
        foreach (var fileToDownload in filesToDownload)
        {
            _logger.LogInformation("     Downloading the file translated");
            var fileContentDownloaded = await _projectApiClient.DownloadFilesAsync(
                project.Id,
                fileToDownload,
                cancellationToken);

            _logger.LogInformation("     File downloaded");
            _logger.LogInformation("     File content:");
            _logger.LogInformation("     {FileContentDownloaded}", fileContentDownloaded);
        }
    }

    private async Task<IEnumerable<Guid>?> PollTranslationsUntilFinished(
        int projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("     Polling for translation");
        var project = await _projectApiClient.GetProjectAsync(projectId, cancellationToken);

        return AreAllFilesTranslated(project)
            ? GetTranslationFileIds(project)
            : null;
    }

    private static IEnumerable<Guid> GetTranslationFileIds(ProjectResponse project)
        => project.Files!
                  .SelectMany(
                      file => file.Translations.Where(translation => translation.Status == FileTranslatedStatusName))
                  .Select(translation => translation.FileId!.Value);

    private static bool AreAllFilesTranslated(ProjectResponse project)
        => project.Files!.All(
            file => file.Translations.All(translation => translation.Status == FileTranslatedStatusName));

    private void PrintResultList(List<IdAndNameResponse> idAndNameResponses)
    {
        foreach (var response in idAndNameResponses)
            _logger.LogInformation("    {Id} - {Name}", response.Id, response.Name);
    }
}