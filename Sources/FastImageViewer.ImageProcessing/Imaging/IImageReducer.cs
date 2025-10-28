// <copyright file="IImageReducer.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.ImageProcessing.Models;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Defines helpers for generating reduced image representations.
/// </summary>
public interface IImageReducer
{
    /// <summary>
    /// Generates a reduced image for the specified entry and target metrics.
    /// </summary>
    /// <param name="entry">The image entry describing the source file.</param>
    /// <param name="metrics">The target screen metrics used to compute scaling.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that provides the reduced image data.</returns>
    Task<ImageData> CreateReducedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken);
}