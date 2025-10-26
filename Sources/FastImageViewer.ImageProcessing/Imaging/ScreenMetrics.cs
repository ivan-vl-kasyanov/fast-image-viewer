// <copyright file="ScreenMetrics.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Describes screen characteristics used when rendering images.
/// </summary>
/// <param name="Width">The width of the usable screen area in pixels.</param>
/// <param name="Height">The height of the usable screen area in pixels.</param>
/// <param name="Dpi">The effective DPI of the display.</param>
public readonly record struct ScreenMetrics(
    int Width,
    int Height,
    double Dpi);