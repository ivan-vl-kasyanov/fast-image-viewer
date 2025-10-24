// <copyright file="PerformanceLogger.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Diagnostics;

using FastImageViewer.Configuration;
using FastImageViewer.Imaging;

using Serilog;
using Serilog.Core;

namespace FastImageViewer.Diagnostics;

internal sealed class PerformanceLogger : IDisposable
{
    private readonly Logger _logger;
    private readonly WarmthMode _mode;

    private PerformanceLogger(
        Logger logger,
        WarmthMode mode,
        string logPath)
    {
        _logger = logger;
        _mode = mode;
        LogFilePath = logPath;
    }

    public string LogFilePath { get; }

    public static PerformanceLogger Create(WarmthMode mode)
    {
        var logFile = Path.Combine(
            AppContext.BaseDirectory,
            $"{AppConstants.LogFilePrefix}{DateTime.UtcNow:yyyyMMdd-HHmmss}{AppConstants.LogFileExtension}");
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFile)
            .CreateLogger();

        return new PerformanceLogger(
            logger,
            mode,
            logFile);
    }

    public async Task<T> MeasureAsync<T>(
        PerformanceMeasureInput input,
        Func<Task<PerformanceMeasureResult<T>>> action)
    {
        var stopwatch = Stopwatch.StartNew();
#pragma warning disable S2139 // Exceptions should be either logged or rethrown but not both | Temporary
        try
        {
            var result = await action();
            stopwatch.Stop();
            LogDuration(
                input,
                result.Metadata,
                stopwatch.Elapsed.TotalMilliseconds);

            return result.Value;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(
                ex,
                "{Operation}|{Mode}|{File}|{Source}|{Elapsed}ms",
                input.Operation,
                _mode,
                input.FileName,
                input.Source,
                stopwatch.Elapsed.TotalMilliseconds);

            throw;
        }
#pragma warning restore S2139 // Exceptions should be either logged or rethrown but not both | Temporary
    }

    public void LogDuration(
        PerformanceMeasureInput input,
        ImageMetadata metadata,
        double elapsedMilliseconds)
    {
        _logger.Information(
            "{Operation}|{Mode}|{File}|{Source}|{Width}x{Height}@{Dpi:F2}|{Elapsed}ms",
            input.Operation,
            _mode,
            input.FileName,
            input.Source,
            metadata.Width,
            metadata.Height,
            metadata.Dpi,
            elapsedMilliseconds);
    }

    public void LogBackgroundError(Exception exception)
    {
        _logger.Error(
            exception,
            "Background operation failed");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _logger.Dispose();
    }
}