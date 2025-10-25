// <copyright file="ScreenMetricsProvider.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;

using FastImageViewer.Configuration;

namespace FastImageViewer.Imaging;

/// <summary>
/// Supplies screen metrics for rendering decisions.
/// </summary>
internal static class ScreenMetricsProvider
{
    /// <summary>
    /// Gets the screen metrics for the primary display.
    /// </summary>
    /// <param name="topLevel">The top-level window requesting the metrics.</param>
    /// <returns>The metrics describing the primary display.</returns>
    public static ScreenMetrics GetPrimaryMetrics(TopLevel topLevel)
    {
        var screen = topLevel.Screens?.Primary;
        if (screen is null)
        {
            return new ScreenMetrics(
                topLevel.ClientSize.Width.EnsureDimensionWithinInt32Range(nameof(topLevel.ClientSize.Width)),
                topLevel.ClientSize.Height.EnsureDimensionWithinInt32Range(nameof(topLevel.ClientSize.Height)),
                AppConstants.DefaultDpi);
        }

        var dpi = AppConstants.DefaultDpi * topLevel.RenderScaling;

        return new ScreenMetrics(
            screen.Bounds.Width,
            screen.Bounds.Height,
            dpi);
    }
}