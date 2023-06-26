/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
namespace LanguageWire.SampleApp.Services;

public interface ITranslationsService
{
    Task GetBasicDataAsync(CancellationToken cancellationToken);
    Task GetAccountDataAsync(CancellationToken cancellationToken);
    Task CreateProjectWronglyAsync(CancellationToken cancellationToken);
    Task CreateProjectWithFileAsync(CancellationToken cancellationToken);
    Task PollTranslationsAsync(TimeSpan waitingTime, CancellationToken cancellationToken);
    Task ProjectsCleanup(CancellationToken cancellationToken);
}