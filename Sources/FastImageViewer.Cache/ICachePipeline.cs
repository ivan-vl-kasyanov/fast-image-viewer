// <copyright file="ICachePipeline.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Cache.Models;
using FastImageViewer.ImageProcessing.Models;

namespace FastImageViewer.Cache;

/// <summary>
/// Defines methods for retrieving and pre-warming cached image data.
/// </summary>
public interface ICachePipeline : IDisposable
{
    /// <summary>
    /// Retrieves a reduced image using the supplied screen metrics.
    /// </summary>
    /// <param name="entry">The image entry to retrieve.</param>
    /// <param name="metrics">The target screen metrics.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>
    /// The cached or computed reduced image, or <c>null</c> when unavailable.
    /// </returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    Task<ImageDataResult?> GetReducedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the original image for the specified entry.
    /// </summary>
    /// <param name="entry">The image entry to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The cached or loaded original image data.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    Task<ImageDataResult> GetOriginalAsync(
        ImageEntry entry,
        CancellationToken cancellationToken);

    /// <summary>
    /// Warms the cache for eligible entries using the supplied screen metrics.
    /// </summary>
    /// <param name="entries">The entries to warm.</param>
    /// <param name="metrics">The target screen metrics.</param>
    /// <param name="progress">The progress reporter for warming completion.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when warming finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    Task WarmAllAsync(
        IReadOnlyList<ImageEntry> entries,
        ScreenMetrics metrics,
        IProgress<double>? progress,
        CancellationToken cancellationToken);
}