/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using Microsoft.Extensions.Logging;
using Moq;

namespace LanguageWire.SampleApp.UnitTests.Verifiers;

internal static class LoggerVerifier
{
    public static void LoggerVerify<T>(
        this Mock<ILogger<T>> loggerMock,
        LogLevel level,
        string? loggerMessage,
        Times times)
        => loggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) => loggerMessage != null && value.ToString()!.Contains(loggerMessage)),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!),
            times);
}