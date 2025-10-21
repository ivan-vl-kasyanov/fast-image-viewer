// <copyright file="AppStartupContext.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Diagnostics;

namespace FastImageViewer.Configuration;

internal static class AppStartupContext
{
    private static readonly Lock _syncRoot = new();
    private static WarmthMode? _mode;
    private static PerformanceLogger? _logger;

    public static void SetMode(WarmthMode value)
    {
        lock (_syncRoot)
        {
            _mode = value;
        }
    }

    public static WarmthMode GetMode()
    {
        lock (_syncRoot)
        {
            return _mode ?? WarmthMode.Cold;
        }
    }

    public static void SetLogger(PerformanceLogger value)
    {
        lock (_syncRoot)
        {
            _logger = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    public static PerformanceLogger GetLogger()
    {
        lock (_syncRoot)
        {
            return _logger ?? throw new InvalidOperationException("Logger not configured.");
        }
    }
}