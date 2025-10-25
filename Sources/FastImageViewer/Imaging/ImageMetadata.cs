// <copyright file="ImageMetadata.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Imaging;

/// <summary>
/// Describes the dimensions and DPI of an image.
/// </summary>
/// <param name="Width">The pixel width of the image.</param>
/// <param name="Height">The pixel height of the image.</param>
/// <param name="Dpi">The DPI reported by the source.</param>
internal readonly record struct ImageMetadata(
    int Width,
    int Height,
    double Dpi)
{
    /// <summary>
    /// Gets a placeholder metadata instance that indicates unknown dimensions.
    /// </summary>
    public static ImageMetadata Unknown { get; } = new(0, 0, 0d);
}