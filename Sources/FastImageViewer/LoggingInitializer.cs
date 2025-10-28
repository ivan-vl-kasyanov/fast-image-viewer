// <copyright file="LoggingInitializer.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;
using FastImageViewer.Shared.FastImageViewer.Configuration;

using Serilog;
using Serilog.Events;

namespace FastImageViewer;

/// <summary>
/// Provides helpers for configuring structured logging.
/// </summary>
internal static class LoggingInitializer
{
    /// <summary>
    /// Attempts to configure the application logger.
    /// </summary>
    /// <param name="applicationPaths">The application paths used to determine log storage.</param>
    /// <returns><c>true</c> when logging is ready; otherwise, <c>false</c>.</returns>
    public static bool TryConfigure(IApplicationPaths applicationPaths)
    {
        ArgumentNullException.ThrowIfNull(applicationPaths);

        try
        {
            var logFilePath = Path.Combine(
                applicationPaths.CacheDirectory,
                AppInvariantStringConstants.LogFileName);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.File(
                    logFilePath,
                    fileSizeLimitBytes: AppNumericConstants.LogFileRollingSizeLimitBytes,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Month,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: AppNumericConstants.LogFileRetainedCountLimit,
                    shared: true)
                .CreateLogger();

            Log.Information("Program starting...");

            return true;
        }
        catch (Exception ex)
        {
            Console
                .Error
                .Write(ex);
            Console.ReadKey();

            return false;
        }
    }
}