// <copyright file="OriginalImageLoader.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;

using ImageMagick;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Provides helpers for loading original image data from disk.
/// </summary>
public static class OriginalImageLoader
{
    /// <summary>
    /// Loads the original image bytes and metadata for the specified entry.
    /// </summary>
    /// <param name="entry">The image entry describing the file to open.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that provides the raw image data and metadata.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public static async Task<ImageData> LoadAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var fileStream = new FileStream(
            entry.FullPath,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Share = FileShare.Read,
                BufferSize = 4096,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });
        cancellationToken.ThrowIfCancellationRequested();

        using var image = new MagickImage();
        await image
            .ReadAsync(
                fileStream,
                cancellationToken)
            .ConfigureAwait(false);

        var density = image.Density ?? new Density(AppConstants.DefaultDpi);
        var dpi = density.X > 0
            ? density.X
            : AppConstants.DefaultDpi;
        var bytes = image.ToByteArray();
        var metadata = new ImageMetadata(
            image.Width.EnsureDimensionWithinInt32Range(nameof(image.Width)),
            image.Height.EnsureDimensionWithinInt32Range(nameof(image.Height)),
            dpi);

        return new ImageData(
            bytes,
            metadata);
    }
}