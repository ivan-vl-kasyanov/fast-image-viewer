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
        return await MagickImageLoader
            .WithImageAsync(
                entry,
                CreateReducedProcessor(metrics),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static Func<MagickImage, CancellationToken, ImageData> CreateReducedProcessor(
        ScreenMetrics metrics)
    {
        return (image, cancellationToken) => CreateReducedImage(
            image,
            metrics,
            cancellationToken);
    }

    private static ImageData CreateReducedImage(
        MagickImage image,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (targetWidth, targetHeight, shouldResize) = DetermineTargetDimensions(
            image,
            metrics);
        var targetDpi = DetermineTargetDpi(
            image,
            metrics);

        ApplyTargetDensity(
            image,
            targetDpi);
        ResizeIfRequired(
            image,
            targetWidth,
            targetHeight,
            shouldResize);
        ConfigureReducedEncoding(image);

        var bytes = EncodeReducedImage(image);
        var metadata = CreateMetadata(
            targetWidth,
            targetHeight,
            targetDpi);

        return new ImageData(
            bytes,
            metadata);
    }

    private static (int TargetWidth, int TargetHeight, bool ShouldResize) DetermineTargetDimensions(
        MagickImage image,
        ScreenMetrics metrics)
    {
        var originalWidth = image.Width;
        var originalHeight = image.Height;
        var shouldResize =
            (originalWidth > metrics.Width) ||
            (originalHeight > metrics.Height);
        var scaleWidth = metrics.Width > 0
            ? (double)metrics.Width / originalWidth
            : AppNumericConstants.IdentityScaleFactor;
        var scaleHeight = metrics.Height > 0
            ? (double)metrics.Height / originalHeight
            : AppNumericConstants.IdentityScaleFactor;
        var scale = shouldResize
            ? Math.Min(
                Math.Min(
                    scaleWidth,
                    scaleHeight),
                AppNumericConstants.IdentityScaleFactor)
            : AppNumericConstants.IdentityScaleFactor;
        var targetWidth = Math.Max(
            AppNumericConstants.MinimumImageDimension,
            (int)Math.Round(originalWidth * scale));
        var targetHeight = Math.Max(
            AppNumericConstants.MinimumImageDimension,
            (int)Math.Round(originalHeight * scale));

        return (targetWidth, targetHeight, shouldResize);
    }

    private static double DetermineTargetDpi(
        MagickImage image,
        ScreenMetrics metrics)
    {
        var density = image.Density ?? new Density(AppNumericConstants.DefaultDpi);
        var originalDpi = density.X > 0
            ? density.X
            : AppNumericConstants.DefaultDpi;
        var requestedDpi = metrics.Dpi > 0
            ? metrics.Dpi
            : AppNumericConstants.DefaultDpi;

        return Math.Min(originalDpi, requestedDpi);
    }

    private static void ApplyTargetDensity(
        MagickImage image,
        double targetDpi)
    {
        image.Density = new Density(
            targetDpi,
            targetDpi,
            DensityUnit.PixelsPerInch);
    }

    private static void ResizeIfRequired(
        MagickImage image,
        int targetWidth,
        int targetHeight,
        bool shouldResize)
    {
        if (!shouldResize)
        {
            return;
        }

        var originalWidth = image.Width;
        var originalHeight = image.Height;
        if ((targetWidth == originalWidth) && (targetHeight == originalHeight))
        {
            return;
        }

        image.Resize(
            Convert.ToUInt32(targetWidth),
            Convert.ToUInt32(targetHeight));
    }

    private static void ConfigureReducedEncoding(MagickImage image)
    {
        image.Format = MagickFormat.WebP;
        image.Quality = AppNumericConstants.ReducedQuality;
    }

    private static byte[] EncodeReducedImage(MagickImage image)
    {
        return image.ToByteArray(MagickFormat.WebP);
    }

    private static ImageMetadata CreateMetadata(
        int width,
        int height,
        double dpi)
    {
        return new ImageMetadata(
            width,
            height,
            dpi);
    }
}