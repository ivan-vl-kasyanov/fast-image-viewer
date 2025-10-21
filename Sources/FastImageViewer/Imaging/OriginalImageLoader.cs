// <copyright file="OriginalImageLoader.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Configuration;

using ImageMagick;

namespace FastImageViewer.Imaging;

internal sealed class OriginalImageLoader
{
    public Task<ImageData> LoadAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => Load(
                entry,
                cancellationToken),
            cancellationToken);
    }

    private static ImageData Load(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var image = new MagickImage(entry.FullPath);
        cancellationToken.ThrowIfCancellationRequested();

        var density = image.Density ?? new Density(AppConstants.DefaultDpi);
        var dpi = density.X > 0
            ? density.X
            : AppConstants.DefaultDpi;
        image.Format = MagickFormat.Png;

        using var stream = new MemoryStream();
        image.Write(stream);
        var bytes = stream.ToArray();
        var metadata = new ImageMetadata(
            Convert.ToInt32(image.Width), // TODO: Add checks for overflow.
            Convert.ToInt32(image.Height), // TODO: Add checks for overflow.
            dpi);

        return new ImageData(bytes, metadata);
    }
}