/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Configuration;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Exceptions;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Requests;
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;
using Microsoft.Extensions.Options;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient;

internal class ProjectApiClient : IProjectApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ProjectApiClient(IOptions<ProjectApiConfiguration> projectApiConfigurationOptions, HttpClient httpClient)
    {
        _httpClient            = httpClient;
        httpClient.BaseAddress = new Uri(projectApiConfigurationOptions.Value.ApiUrl);
    }

    public async Task<ProjectResponse?> CreateProjectAsync(
        ProjectCreationRequest projectCreationRequest,
        CancellationToken cancellationToken)
    {
        var response = await PostAsync(
            "projects",
            payload: JsonSerializer.Serialize(projectCreationRequest),
            cancellationToken);

        return await GetObjectFromResponse<ProjectResponse>(response, cancellationToken);
    }

    public async Task AddFileAsync(int projectId, CancellationToken cancellationToken)
    {
        const string sourceLanguage = "en-GB";
        const string fileName       = "ProjectApiSampleApp.txt";
        const string fileContent    = "Open once a time...";

        var targetLanguages = string.Join(
            separator: ',',
            values: new List<string>
            {
                "de-DE",
                "es-ES"
            });

        await PostWithMediaTypeAsync(
            endpoint:
            $"projects/{projectId}/files?fileName={fileName}&sourceLanguage={sourceLanguage}&targetLanguages={targetLanguages}",
            fileContent,
            cancellationToken);
    }

    public async Task StartProjectAsync(int projectId, CancellationToken cancellationToken)
        => await PostAsync(endpoint: $"projects/{projectId}/do-start", string.Empty, cancellationToken);

    public async Task DeleteProjectAsync(int projectId, CancellationToken cancellationToken)
        => await DeleteAsync(endpoint: $"projects/{projectId}", cancellationToken);

    public async Task<ProjectResponse> GetProjectAsync(int projectId, CancellationToken cancellationToken)
        => await GetAsync<ProjectResponse>(endpointName: $"projects/{projectId}", cancellationToken);

    public async Task<ProjectListResponse> GetProjectsByFiltersAsync(string status, CancellationToken cancellationToken)
        => await GetAsync<ProjectListResponse>(
            endpointName: "projects?Limit=500"
                          + "&Title=Growing corn"
                          + $"&Status={status}"
                          + $"&CreatedAfter={DateTime.UtcNow.AddDays(-1):O}",
            cancellationToken);

    public async Task<List<IdAndNameResponse>> GetServicesAsync(CancellationToken cancellationToken)
        => await GetAsync<List<IdAndNameResponse>>("services", cancellationToken);

    public async Task<List<IdAndNameResponse>> GetWorkAreasAsync(CancellationToken cancellationToken)
        => await GetAsync<List<IdAndNameResponse>>("workareas", cancellationToken);

    public async Task<List<IdAndNameResponse>> GetTranslationMemoriesAsync(CancellationToken cancellationToken)
        => await GetAsync<List<IdAndNameResponse>>("translation-memories", cancellationToken);

    public async Task<List<IdAndNameResponse>> GetInvoicingAccountsAsync(CancellationToken cancellationToken)
        => await GetAsync<List<IdAndNameResponse>>("invoicing-accounts", cancellationToken);

    public async Task<List<UserResponse>> GetUsersAsync(CancellationToken cancellationToken)
        => await GetAsync<List<UserResponse>>("users", cancellationToken);

    public async Task<List<IdAndNameResponse>> GetProjectTemplatesAsync(CancellationToken cancellationToken)
        => await GetAsync<List<IdAndNameResponse>>("project-templates", cancellationToken);

    public async Task<string> DownloadFilesAsync(int projectId, Guid fileId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            requestUri: $"projects/{projectId}/files/{fileId}",
            cancellationToken);
        await CheckInvalidResponse(response, cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<T> GetAsync<T>(string endpointName, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(endpointName, cancellationToken);
        return await GetObjectFromResponse<T>(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> PostAsync(
        string endpoint,
        string payload,
        CancellationToken cancellationToken)
    {
        var content  = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

        await CheckInvalidResponse(response, cancellationToken);

        return response;
    }

    private async Task PostWithMediaTypeAsync(string endpoint, string payload, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Text.Plain));

        request.Content = new StringContent(payload);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        await CheckInvalidResponse(response, cancellationToken);
    }

    private async Task DeleteAsync(string endpoint, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);

        await CheckInvalidResponse(response, cancellationToken);
    }

    private async Task<T> GetObjectFromResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await CheckInvalidResponse(response, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions)
               ?? throw new ArgumentNullException(response.RequestMessage?.RequestUri?.AbsoluteUri);
    }

    private async Task CheckInvalidResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(responseContent, _jsonSerializerOptions)
                         ?? throw new ArgumentNullException(response.RequestMessage?.RequestUri?.AbsoluteUri);

            throw new ProjectApiException(errors);
        }
    }
}