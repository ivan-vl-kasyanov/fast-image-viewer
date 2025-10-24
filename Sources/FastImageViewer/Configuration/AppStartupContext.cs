// <copyright file="AppStartupContext.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Configuration;

internal static class AppStartupContext
{
    private static readonly Lock _syncRoot = new();
    private static WarmthMode? _mode;

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
}