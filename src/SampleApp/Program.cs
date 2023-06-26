/*
 * This file is part of the LanguageWire Project API Sample APP.
 *
 * (c) LanguageWire <contact@languagewire.com>
 *
 * For the full copyright and license information, please view the LICENSE file that was distributed with this source
 * code.
 */
using LanguageWire.SampleApp;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("/================================/");
Console.WriteLine("/    Project API Sample APP      /");
Console.WriteLine("/================================/");

var services = DependencyInjection.Initialize();

await using var serviceProvider = services.BuildServiceProvider();

var sampleApp = serviceProvider.GetRequiredService<SampleAppRunner>();
await sampleApp.RunAsync();

Console.WriteLine("Closing the application...");