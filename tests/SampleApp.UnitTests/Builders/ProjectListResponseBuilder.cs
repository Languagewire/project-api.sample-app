/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;

namespace LanguageWire.SampleApp.UnitTests.Builders;

internal static class ProjectListResponseBuilder
{
    public static ProjectListResponse GetEmptyProjectListResponse()
        => new(Projects: new List<ProjectItem>(), Metadata: default!);

    public static ProjectListResponse GetProjectListByStatusResponse(string status)
        => new(
            Projects: new List<ProjectItem>
            {
                new(
                    Id: default,
                    ExternalId: default,
                    Title: default!,
                    UserId: default,
                    CompanyId: default,
                    status,
                    CreationDate: default,
                    Deadline: default,
                    FinishedAt: default)
            },
            Metadata: default!);
}