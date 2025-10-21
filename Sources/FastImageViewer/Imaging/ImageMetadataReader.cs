// <copyright file="ImageMetadataReader.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Configuration;

using ImageMagick;

namespace FastImageViewer.Imaging;

internal static class ImageMetadataReader
{
    internal static ImageMetadata FromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(
            bytes,
            false);
        var info = new MagickImageInfo(stream);
        var density = info.Density;
        var dpi = density?.X > 0
            ? density!.X
            : AppConstants.DefaultDpi;

        return new ImageMetadata(
            (int)info.Width,
            (int)info.Height,
            dpi);
    }
}