// <copyright file="ImageReducer.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;

using ImageMagick;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Creates reduced-size variants of images suitable for quick display.
/// </summary>
public static class ImageReducer
{
    /// <summary>
    /// Generates a reduced image for the specified entry and target metrics.
    /// </summary>
    /// <param name="entry">The image entry describing the source file.</param>
    /// <param name="metrics">The target screen metrics used to compute scaling.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that provides the reduced image data.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public static async Task<ImageData> CreateReducedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
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
                BufferSize = AppConstants.ImageFileReadBufferSize,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });
        cancellationToken.ThrowIfCancellationRequested();

        using var image = new MagickImage();
        await image
            .ReadAsync(
                fileStream,
                cancellationToken)
            .ConfigureAwait(false);

        var originalWidth = image.Width;
        var originalHeight = image.Height;
        var scaleWidth = metrics.Width > 0
            ? (double)metrics.Width / originalWidth
            : 1d;
        var scaleHeight = metrics.Height > 0
            ? (double)metrics.Height / originalHeight
            : 1d;
        var shouldResize =
            (originalWidth > metrics.Width) ||
            (originalHeight > metrics.Height);
        var scale = shouldResize
            ? Math.Min(
                Math.Min(
                    scaleWidth,
                    scaleHeight),
                1d)
            : 1d;
        var targetWidth = Math.Max(
            1,
            (int)Math.Round(originalWidth * scale));
        var targetHeight = Math.Max(
            1,
            (int)Math.Round(originalHeight * scale));
        var originalDensity = image.Density ?? new Density(AppConstants.DefaultDpi);
        var originalDpi = originalDensity.X > 0
            ? originalDensity.X
            : AppConstants.DefaultDpi;
        var targetDpi = Math.Min(
            originalDpi,
            metrics.Dpi > 0 ? metrics.Dpi : AppConstants.DefaultDpi);
        image.Density = new Density(
            targetDpi,
            targetDpi,
            DensityUnit.PixelsPerInch);
        if (shouldResize &&
            ((targetWidth != originalWidth) || (targetHeight != originalHeight)))
        {
            image.Resize(
                Convert.ToUInt32(targetWidth),
                Convert.ToUInt32(targetHeight));
        }

        image.Format = MagickFormat.WebP;
        image.Quality = AppConstants.ReducedQuality;

        var bytes = image.ToByteArray(MagickFormat.WebP);
        var metadata = new ImageMetadata(
            targetWidth,
            targetHeight,
            targetDpi);

        return new ImageData(
            bytes,
            metadata);
    }
}