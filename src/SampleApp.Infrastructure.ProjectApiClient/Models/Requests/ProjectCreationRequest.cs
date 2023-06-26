/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Requests;

public record ProjectCreationRequest(
    string Title,
    string? Briefing,
    string? BriefingForExperts,
    string? PurchaseOrderNumber,
    DateTime? Deadline,
    int? TemplateId,
    int? ServiceId,
    int? TranslationMemoryId,
    int? InvoicingAccountId,
    int? UserId,
    int? WorkAreaId,
    string? ExternalId);