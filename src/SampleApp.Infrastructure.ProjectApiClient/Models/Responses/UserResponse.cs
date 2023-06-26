/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
// ReSharper disable ClassNeverInstantiated.Global

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;

public record UserResponse(int Id, string FirstName, string LastName);