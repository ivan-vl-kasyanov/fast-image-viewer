// <copyright file="IScreenMetricsProvider.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;

using FastImageViewer.ImageProcessing.Models;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Defines helpers for obtaining screen metrics.
/// </summary>
public interface IScreenMetricsProvider
{
    /// <summary>
    /// Gets the screen metrics for the primary display.
    /// </summary>
    /// <param name="topLevel">The top-level window requesting the metrics.</param>
    /// <returns>The metrics describing the primary display.</returns>
    ScreenMetrics GetPrimaryMetrics(TopLevel topLevel);
}