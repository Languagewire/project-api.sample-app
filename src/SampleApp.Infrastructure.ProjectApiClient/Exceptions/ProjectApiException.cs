/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Models.Responses;

namespace LanguageWire.SampleApp.Infrastructure.ProjectApiClient.Exceptions;

public class ProjectApiException : Exception
{
    public ProjectApiException(List<ErrorResponse> errors)
    {
        Errors  = errors;
        Message = BuildMessageFromErrorResponse();
    }

    public ProjectApiException(string message, List<ErrorResponse> errors)
        : base(message)
    {
        Errors  = errors;
        Message = BuildMessageFromErrorResponse();
    }

    public ProjectApiException(string message, Exception inner, List<ErrorResponse> errors)
        : base(message, inner)
    {
        Errors  = errors;
        Message = BuildMessageFromErrorResponse();
    }

    public override string Message { get; }
    public List<ErrorResponse> Errors { get; }

    private string BuildMessageFromErrorResponse()
        => string.Join(
            ",",
            values: Errors.Select(e => $"Received error {e.Code}: {e.Message}. Suggested hint: {e.Hint}").ToList());
}