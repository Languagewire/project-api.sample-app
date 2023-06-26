/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Mappers;

public static class UserMapper
{
    public static List<IdAndNameResponse> MapToIdAndNameResponseList(this IEnumerable<UserResponse> userResponses)
        => userResponses.Select(userResponse => userResponse.MapToIdAndNameResponse()).ToList();

    private static IdAndNameResponse MapToIdAndNameResponse(this UserResponse userResponse)
        => new(userResponse.Id, Name: $"{userResponse.FirstName} {userResponse.LastName}");
}