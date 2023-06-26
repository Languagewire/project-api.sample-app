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

internal static class ProjectResponseBuilder
{
    public static ProjectResponse GetProjectResponse()
        => new(
            Id: new int(),
            Title: default!,
            Briefing: default,
            BriefingForExperts: default,
            PurchaseOrderNumber: default,
            Deadline: default,
            CreationDate: default,
            FinishedAt: default,
            TemplateId: default,
            ServiceId: default,
            TranslationMemoryId: default,
            InvoicingAccountId: default,
            UserId: default,
            WorkAreaId: default,
            Status: default,
            PlatformLink: default,
            ReferenceFiles: default,
            Files: default,
            ExternalId: default);

    public static ProjectResponse GetProjectWithFilesResponse(string translationStatus)
        => new(
            Id: new int(),
            Title: default!,
            Briefing: default,
            BriefingForExperts: default,
            PurchaseOrderNumber: default,
            Deadline: default,
            CreationDate: default,
            FinishedAt: default,
            TemplateId: default,
            ServiceId: default,
            TranslationMemoryId: default,
            InvoicingAccountId: default,
            UserId: default,
            WorkAreaId: default,
            Status: default,
            PlatformLink: default,
            ReferenceFiles: default,
            Files: new List<FileTranslationsResponse>
            {
                new(
                    SourceFileId: default,
                    SourceFileName: default,
                    SourceLanguage: default,
                    Translations: new List<TranslationResponse>
                    {
                        new(
                            Id: default,
                            FileId: Guid.NewGuid(),
                            TargetLanguage: default,
                            translationStatus,
                            TranslationFileName: default,
                            TranslationFileLink: default)
                    })
            },
            ExternalId: default);
}