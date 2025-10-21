// <copyright file="ScreenMetricsProvider.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;

using FastImageViewer.Configuration;

namespace FastImageViewer.Imaging;

internal static class ScreenMetricsProvider
{
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