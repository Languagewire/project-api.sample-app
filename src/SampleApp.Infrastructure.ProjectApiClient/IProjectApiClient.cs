/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Requests;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient;

public interface IProjectApiClient
{
    Task<ProjectResponse?> CreateProjectAsync(
        ProjectCreationRequest projectCreationRequest,
        CancellationToken cancellationToken);

    Task AddFileAsync(int projectId, CancellationToken cancellationToken);
    Task StartProjectAsync(int projectId, CancellationToken cancellationToken);
    Task DeleteProjectAsync(int projectId, CancellationToken cancellationToken);
    Task<ProjectResponse> GetProjectAsync(int projectId, CancellationToken cancellationToken);
    Task<ProjectListResponse> GetProjectsByFiltersAsync(string status, CancellationToken cancellationToken);
    Task<List<IdAndNameResponse>> GetServicesAsync(CancellationToken cancellationToken);
    Task<List<IdAndNameResponse>> GetWorkAreasAsync(CancellationToken cancellationToken);
    Task<List<IdAndNameResponse>> GetTranslationMemoriesAsync(CancellationToken cancellationToken);
    Task<List<IdAndNameResponse>> GetInvoicingAccountsAsync(CancellationToken cancellationToken);
    Task<List<UserResponse>> GetUsersAsync(CancellationToken cancellationToken);
    Task<List<IdAndNameResponse>> GetProjectTemplatesAsync(CancellationToken cancellationToken);
    Task<string> DownloadFilesAsync(int projectId, Guid fileId, CancellationToken cancellationToken);
}