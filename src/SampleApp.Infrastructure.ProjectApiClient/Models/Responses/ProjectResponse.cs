/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;

public record ProjectResponse(
    int Id,
    string Title,
    string? Briefing,
    string? BriefingForExperts,
    string? PurchaseOrderNumber,
    DateTime? Deadline,
    DateTime CreationDate,
    DateTime? FinishedAt,
    int? TemplateId,
    int? ServiceId,
    int? TranslationMemoryId,
    int? InvoicingAccountId,
    int? UserId,
    int? WorkAreaId,
    string? Status,
    string? PlatformLink,
    List<string>? ReferenceFiles,
    List<FileTranslationsResponse>? Files,
    string? ExternalId);