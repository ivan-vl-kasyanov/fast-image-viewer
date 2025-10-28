// <copyright file="IOriginalImageLoader.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.ImageProcessing.Models;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Defines helpers for loading original image data from disk.
/// </summary>
public interface IOriginalImageLoader
{
    /// <summary>
    /// Loads the original image bytes and metadata for the specified entry.
    /// </summary>
    /// <param name="entry">The image entry describing the file to open.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that provides the raw image data and metadata.</returns>
    Task<ImageData> LoadAsync(
        ImageEntry entry,
        CancellationToken cancellationToken);
}