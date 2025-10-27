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
        return await MagickImageLoader
            .WithImageAsync(
                entry,
                ProcessOriginalImage,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static ImageData ProcessOriginalImage(
        MagickImage image,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var dpi = DetermineOriginalDpi(image);
        var bytes = EncodeLosslessWebp(image);
        var metadata = CreateOriginalMetadata(
            image,
            dpi);

        return new ImageData(
            bytes,
            metadata);
    }

    private static double DetermineOriginalDpi(MagickImage image)
    {
        var density = image.Density ?? new Density(AppNumericConstants.DefaultDpi);

        return density.X > 0
            ? density.X
            : AppNumericConstants.DefaultDpi;
    }

    private static byte[] EncodeLosslessWebp(MagickImage image)
    {
        ConfigureLosslessWebpSettings(image);

        try
        {
            return image.ToByteArray(MagickFormat.WebP);
        }
        finally
        {
            ResetWebpSettings(image);
        }
    }

    private static void ConfigureLosslessWebpSettings(MagickImage image)
    {
        image.Settings.SetDefine(
            MagickFormat.WebP,
            AppInvariantStringConstants.WebpLosslessDefineName,
            AppInvariantStringConstants.WebpLosslessDefineValue);
        image.Settings.SetDefine(
            MagickFormat.WebP,
            AppInvariantStringConstants.WebpMethodDefineName,
            AppInvariantStringConstants.WebpMethodDefineValue);
        image.Settings.SetDefine(
            MagickFormat.WebP,
            AppInvariantStringConstants.WebpAlphaQualityDefineName,
            AppInvariantStringConstants.WebpAlphaQualityDefineValue);
    }

    private static void ResetWebpSettings(MagickImage image)
    {
        image.Settings.RemoveDefine(
            MagickFormat.WebP,
            AppInvariantStringConstants.WebpLosslessDefineName);
        image.Settings.RemoveDefine(
            MagickFormat.WebP,
            AppInvariantStringConstants.WebpMethodDefineName);
        image.Settings.RemoveDefine(
            MagickFormat.WebP,
            AppInvariantStringConstants.WebpAlphaQualityDefineName);
    }

    private static ImageMetadata CreateOriginalMetadata(
        MagickImage image,
        double dpi)
    {
        return new ImageMetadata(
            image.Width.EnsureDimensionWithinInt32Range(nameof(image.Width)),
            image.Height.EnsureDimensionWithinInt32Range(nameof(image.Height)),
            dpi);
    }
}