// <copyright file="AppStartupContext.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Configuration;

/// <summary>
/// Stores startup configuration derived from command-line arguments.
/// </summary>
internal static class AppStartupContext
{
    private static readonly Lock _syncRoot = new();
    private static WarmthMode? _mode;

    /// <summary>
    /// Sets the requested <see cref="WarmthMode"/> for the current run.
    /// </summary>
    /// <param name="value">The warm-up mode to store.</param>
    public static void SetMode(WarmthMode value)
    {
        lock (_syncRoot)
        {
            _mode = value;
        }
    }

    /// <summary>
    /// Gets the stored <see cref="WarmthMode"/> value, defaulting to <see cref="WarmthMode.Cold"/>.
    /// </summary>
    /// <returns>
    /// The previously stored warm-up mode or <see cref="WarmthMode.Cold"/>.
    /// </returns>
    public static WarmthMode GetMode()
    {
        lock (_syncRoot)
        {
            return _mode ?? WarmthMode.Cold;
        }
    }
}