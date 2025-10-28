// <copyright file="ScreenMetricsProvider.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;

using FastImageViewer.ImageProcessing.Models;
using FastImageViewer.Resources;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Supplies screen metrics for rendering decisions.
/// </summary>
public sealed class ScreenMetricsProvider : IScreenMetricsProvider
{
    /// <inheritdoc/>
    public ScreenMetrics GetPrimaryMetrics(TopLevel topLevel)
    {
        var screen = topLevel.Screens?.Primary;
        if (screen is null)
        {
            return new ScreenMetrics(
                topLevel.ClientSize.Width.EnsureDimensionWithinInt32Range(
                    nameof(topLevel.ClientSize.Width)),
                topLevel.ClientSize.Height.EnsureDimensionWithinInt32Range(
                    nameof(topLevel.ClientSize.Height)),
                AppNumericConstants.DefaultDpi);
        }

        var dpi = AppNumericConstants.DefaultDpi * topLevel.RenderScaling;

        return new ScreenMetrics(
            screen.Bounds.Width,
            screen.Bounds.Height,
            dpi);
    }
}