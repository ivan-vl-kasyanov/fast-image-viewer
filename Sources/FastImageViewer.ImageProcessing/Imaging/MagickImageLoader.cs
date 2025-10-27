// <copyright file="MagickImageLoader.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.ImageProcessing.Models;
using FastImageViewer.Resources;

using ImageMagick;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Provides helpers for reading image files into <see cref="MagickImage"/> instances.
/// </summary>
internal static class MagickImageLoader
{
    /// <summary>
    /// Opens the image described by the supplied entry and invokes a processor on the loaded data.
    /// </summary>
    /// <typeparam name="TResult">The type returned by the processor.</typeparam>
    /// <param name="entry">The image entry describing the file to open.</param>
    /// <param name="processor">The processor executed against the loaded image.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result produced by <paramref name="processor"/>.</returns>
    public static async Task<TResult> WithImageAsync<TResult>(
        ImageEntry entry,
        Func<MagickImage, CancellationToken, TResult> processor,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(processor);

        cancellationToken.ThrowIfCancellationRequested();
        await using var fileStream = new FileStream(
            entry.FullPath,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Share = FileShare.Read,
                BufferSize = AppNumericConstants.ImageFileReadBufferSize,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });
        cancellationToken.ThrowIfCancellationRequested();

        using var image = new MagickImage();
        await image
            .ReadAsync(
                fileStream,
                cancellationToken)
            .ConfigureAwait(false);

        return processor(
            image,
            cancellationToken);
    }
}