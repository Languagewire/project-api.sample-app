/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using FluentAssertions;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Exceptions;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Requests;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;
using LanguageWire.SampleApp.Services;
using LanguageWire.SampleApp.UnitTests.Builders;
using LanguageWire.SampleApp.UnitTests.Verifiers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LanguageWire.SampleApp.UnitTests.Services;

public class TranslationsServiceTests
{
    private readonly Mock<ILogger<TranslationsService>> _loggerMock = new();
    private readonly Mock<IProjectApiClient> _projectApiClientMock = new();
    private readonly TranslationsService _sut;

    public TranslationsServiceTests()
        => _sut = new TranslationsService(_projectApiClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task WHEN_getting_basic_data_THEN_Services_and_WorkAreas_are_given()
    {
        SetupProjectApiClientGetBasicData();

        await _sut.GetBasicDataAsync(CancellationToken.None);

        _loggerMock.LoggerVerify(LogLevel.Information, " Services:", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "    1 - Service1", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, " Work Areas:", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "    1 - WorkArea1", times: Times.Once());
    }

    [Fact]
    public async Task
        WHEN_getting_account_data_THEN_TranslationMemories_InvoicingAccounts_Users_ProjectTemplates_are_given()
    {
        SetupProjectApiClientGetAccountData();

        await _sut.GetAccountDataAsync(CancellationToken.None);

        _loggerMock.LoggerVerify(LogLevel.Information, " Translation memories:", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "    1 - TranslationMemories1", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, " Invoicing accounts:", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "    1 - Invoicing account sample app", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, " Users:", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "    1 - Anna Patterson", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, " Project templates:", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "    1 - Sample app template", times: Times.Once());
    }

    [Fact]
    public async Task WHEN_creating_project_wrongly_THEN_errors_are_received_successfully()
    {
        var response = new List<ErrorResponse> { new("SOME_CODE", "Error message", "Some hint") };

        _projectApiClientMock
            .Setup(client => client.CreateProjectAsync(It.IsAny<ProjectCreationRequest>(), CancellationToken.None))
            .ThrowsAsync(new ProjectApiException(response));

        await _sut.CreateProjectWronglyAsync(CancellationToken.None);

        _loggerMock.LoggerVerify(
            LogLevel.Information,
            "    Try to create a project with uncompleted information",
            times: Times.Once());
        _loggerMock.LoggerVerify(
            LogLevel.Error,
            "          Received error SOME_CODE: Error message. Suggested hint: Some hint",
            times: Times.Once());
    }

    [Fact]
    public async Task WHEN_creating_project_with_file_without_error_THEN_project_is_started_successfully()
    {
        SetupProjectApiClientGetAccountData();

        var projectResponse = ProjectResponseBuilder.GetProjectResponse();
        _projectApiClientMock
            .Setup(client => client.CreateProjectAsync(It.IsAny<ProjectCreationRequest>(), CancellationToken.None))
            .ReturnsAsync(projectResponse);
        _projectApiClientMock.Setup(client => client.AddFileAsync(projectResponse.Id, CancellationToken.None));
        _projectApiClientMock.Setup(client => client.StartProjectAsync(projectResponse.Id, CancellationToken.None));

        await _sut.CreateProjectWithFileAsync(CancellationToken.None);

        _loggerMock.LoggerVerify(
            LogLevel.Information,
            "    Creating a project using a template with the minimal data",
            times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "     Adding a file to the project", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "     Starting the project", times: Times.Once());
    }

    [Fact]
    public async Task WHEN_creating_project_with_file_with_error_THEN_error_is_given()
    {
        SetupProjectApiClientGetAccountData();

        var projectResponse = It.IsAny<ProjectResponse>();
        _projectApiClientMock
            .Setup(client => client.CreateProjectAsync(It.IsAny<ProjectCreationRequest>(), CancellationToken.None))
            .ReturnsAsync(projectResponse);
        _projectApiClientMock.Setup(client => client.AddFileAsync(It.IsAny<int>(), CancellationToken.None))
                             .ThrowsAsync(new Exception());

        await FluentActions.Invoking(() => _sut.CreateProjectWithFileAsync(CancellationToken.None))
                           .Should()
                           .ThrowAsync<NullReferenceException>()
                           .Where(
                               exception => exception.Message.Equals(
                                   "Object reference not set to an instance of an object."));

        _loggerMock.LoggerVerify(
            LogLevel.Information,
            "    Creating a project using a template with the minimal data",
            times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "     Adding a file to the project", times: Times.Once());
    }

    [Fact]
    public async Task WHEN_polling_translations_without_projects_Finished_THEN_no_translated_files_are_given()
    {
        var waitingTime         = TimeSpan.FromMilliseconds(1);
        var projectListResponse = ProjectListResponseBuilder.GetEmptyProjectListResponse();
        _projectApiClientMock.Setup(client => client.GetProjectsByFiltersAsync("Finished", CancellationToken.None))
                             .ReturnsAsync(projectListResponse);

        await _sut.PollTranslationsAsync(waitingTime, CancellationToken.None);

        _loggerMock.LoggerVerify(LogLevel.Information, "     Waiting for next batch polling", times: Times.Exactly(5));
        _loggerMock.LoggerVerify(LogLevel.Information, "     Polling finished", times: Times.Once());
    }

    [Fact]
    public async Task WHEN_polling_translations_with_projects_Finished_THEN_translated_files_are_given()
    {
        const string status      = "Finished";
        const string fileContent = "tiki taka";

        var waitingTime         = TimeSpan.FromMilliseconds(1);
        var projectListResponse = ProjectListResponseBuilder.GetProjectListByStatusResponse(status);
        var projectResponse     = ProjectResponseBuilder.GetProjectWithFilesResponse(status);
        _projectApiClientMock.Setup(client => client.GetProjectsByFiltersAsync(status, CancellationToken.None))
                             .ReturnsAsync(projectListResponse);
        _projectApiClientMock.Setup(client => client.GetProjectAsync(It.IsAny<int>(), CancellationToken.None))
                             .ReturnsAsync(projectResponse);
        _projectApiClientMock
            .Setup(client => client.DownloadFilesAsync(It.IsAny<int>(), It.IsAny<Guid>(), CancellationToken.None))
            .ReturnsAsync(fileContent);

        await _sut.PollTranslationsAsync(waitingTime, CancellationToken.None);

        _loggerMock.LoggerVerify(LogLevel.Information, "     Polling for translation", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "     Downloading the file translated", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, loggerMessage: $"     {fileContent}", times: Times.Once());
    }

    [Fact]
    public async Task WHEN_remains_projects_in_status_Draft_THEN_cleanup_is_executed()
    {
        const string status = "Draft";

        var projectListResponse = ProjectListResponseBuilder.GetProjectListByStatusResponse(status);
        _projectApiClientMock.Setup(client => client.GetProjectsByFiltersAsync(status, CancellationToken.None))
                             .ReturnsAsync(projectListResponse);
        _projectApiClientMock.Setup(client => client.DeleteProjectAsync(It.IsAny<int>(), CancellationToken.None));

        await _sut.ProjectsCleanup(CancellationToken.None);

        _loggerMock.LoggerVerify(LogLevel.Information, "     Cleaning project...", times: Times.Once());
        _loggerMock.LoggerVerify(LogLevel.Information, "     Project cleaned", times: Times.Once());
    }

    [Fact]
    public async Task WHEN_no_remains_projects_in_status_Draft_THEN_cleanup_is_skipped()
    {
        const string status = "Draft";

        var projectListResponse = ProjectListResponseBuilder.GetEmptyProjectListResponse();
        _projectApiClientMock.Setup(client => client.GetProjectsByFiltersAsync(status, CancellationToken.None))
                             .ReturnsAsync(projectListResponse);

        await _sut.ProjectsCleanup(CancellationToken.None);

        _loggerMock.LoggerVerify(LogLevel.Information, "     Cleaning project...", times: Times.Never());
        _loggerMock.LoggerVerify(LogLevel.Information, "     Project cleaned", times: Times.Never());
    }

    private void SetupProjectApiClientGetBasicData()
    {
        _projectApiClientMock.Setup(client => client.GetServicesAsync(CancellationToken.None))
                             .ReturnsAsync(
                                 new List<IdAndNameResponse>
                                 {
                                     new(Id: 1, "Service1"),
                                     new(Id: 2, "Service2")
                                 });

        _projectApiClientMock.Setup(client => client.GetWorkAreasAsync(CancellationToken.None))
                             .ReturnsAsync(
                                 new List<IdAndNameResponse>
                                 {
                                     new(Id: 1, "WorkArea1"),
                                     new(Id: 2, "WorkArea2")
                                 });
    }

    private void SetupProjectApiClientGetAccountData()
    {
        _projectApiClientMock.Setup(client => client.GetTranslationMemoriesAsync(CancellationToken.None))
                             .ReturnsAsync(new List<IdAndNameResponse> { new(Id: 1, "TranslationMemories1") });

        _projectApiClientMock.Setup(client => client.GetInvoicingAccountsAsync(CancellationToken.None))
                             .ReturnsAsync(new List<IdAndNameResponse> { new(Id: 1, "Invoicing account sample app") });

        _projectApiClientMock.Setup(client => client.GetUsersAsync(CancellationToken.None))
                             .ReturnsAsync(
                                 new List<UserResponse>
                                 {
                                     new(Id: 1, "Anna", "Patterson"),
                                     new(Id: 2, "James", "Whistler")
                                 });

        _projectApiClientMock.Setup(client => client.GetProjectTemplatesAsync(CancellationToken.None))
                             .ReturnsAsync(new List<IdAndNameResponse> { new(Id: 1, "Sample app template") });
    }
}