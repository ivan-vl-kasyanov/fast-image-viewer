// <copyright file="ICachePipeline.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Imaging;

namespace FastImageViewer.Caching;

internal interface ICachePipeline
{
    Task<ImageDataResult?> GetReducedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken);

    Task<ImageDataResult> GetOriginalAsync(
        ImageEntry entry,
        CancellationToken cancellationToken);

    Task WarmAllAsync(
        IReadOnlyList<ImageEntry> entries,
        ScreenMetrics metrics,
        CancellationToken cancellationToken);

    Task WarmNeighborsAsync(
        IReadOnlyList<ImageEntry> entries,
        int currentIndex,
        ScreenMetrics metrics,
        CancellationToken cancellationToken);
}