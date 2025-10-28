// <copyright file="ImageMetadataExtensions.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;

using ImageMagick;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Extracts image metadata from encoded image bytes.
/// </summary>
public static class ImageMetadataExtensions
{
    /// <summary>
    /// Reads <see cref="ImageMetadata"/> from encoded image bytes.
    /// </summary>
    /// <param name="bytes">The encoded image to inspect.</param>
    /// <returns>The metadata describing the image.</returns>
    public static ImageMetadata GetImageMetadata(this byte[] bytes)
    {
        using var stream = new MemoryStream(
            bytes,
            false);
        var info = new MagickImageInfo(stream);
        var density = info.Density;
        var dpi = density?.X > 0
            ? density!.X
            : AppNumericConstants.DefaultDpi;

        return new ImageMetadata(
            (int)info.Width,
            (int)info.Height,
            dpi);
    }
}