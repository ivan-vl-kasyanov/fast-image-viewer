// <copyright file="ImageCachePipeline.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Concurrent;

using FastImageViewer.ImageProcessing.Imaging;
using FastImageViewer.Resources;
using FastImageViewer.Shared.FastImageViewer.Configuration;

using Serilog;

using ZiggyCreatures.Caching.Fusion;

namespace FastImageViewer.Cache;

/// <summary>
/// Implements image caching logic across memory and distributed caches.
/// </summary>
/// <param name="fusionCache">The fusion cache used to store image data.</param>
/// <param name="mode">The current warm-up mode.</param>
public sealed class ImageCachePipeline(
    IFusionCache fusionCache,
    WarmthMode mode) : ICachePipeline
{
    private static readonly TimeSpan MemoryDuration = TimeSpan.FromMinutes(AppNumericConstants.MemoryCacheDurationMinutes);
    private static readonly TimeSpan JitterMaxDuration = TimeSpan.FromMinutes(AppNumericConstants.MemoryCacheJitterMinutes);
    private static readonly TimeSpan FailSafeMaxDuration = TimeSpan.FromHours(AppNumericConstants.MemoryCacheFailSafeHours);
    private static readonly TimeSpan DistributedCacheDuration = TimeSpan.FromDays(AppNumericConstants.MemoryCacheDistributedDays);

    private readonly IFusionCache _fusionCache = fusionCache;
    private readonly WarmthMode _mode = mode;
    private readonly ConcurrentDictionary<string, ImageMetadata> _metadataCache = new();

    private readonly FusionCacheEntryOptions _cacheOptions = new()
    {
        Duration = MemoryDuration,
        JitterMaxDuration = JitterMaxDuration,
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = FailSafeMaxDuration,
        DistributedCacheDuration = DistributedCacheDuration,
    };

    /// <summary>
    /// Retrieves a reduced image from cache or generates it when necessary.
    /// </summary>
    /// <param name="entry">The image entry identifying the source image.</param>
    /// <param name="metrics">The screen metrics used to size the reduced image.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>
    /// The cached or newly generated reduced image, or <c>null</c> when not applicable.
    /// </returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task<ImageDataResult?> GetReducedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        if (!entry.IsDiskCacheEligible ||
            (_mode == WarmthMode.Cold))
        {
            return null;
        }

        var key = entry.CacheKey;

        try
        {
            var bytes = await _fusionCache.GetOrSetAsync<byte[]>(
                key,
                async (_, token) =>
                {
                    var data = await ImageReducer.CreateReducedAsync(
                        entry,
                        metrics,
                        token);
                    _metadataCache[key] = data.Metadata;

                    return data.Bytes;
                },
                _cacheOptions,
                cancellationToken);
            var metadata = EnsureMetadata(
                key,
                bytes);

            return new ImageDataResult(
                bytes,
                metadata,
                AppInvariantStringConstants.SourceCache,
                true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogBackgroundError(
                ex,
                key);

            return null;
        }
    }

    /// <summary>
    /// Retrieves the original image from cache or loads it from disk.
    /// </summary>
    /// <param name="entry">The image entry identifying the source image.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The cached or newly loaded original image data.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the original image cannot be loaded.</exception>
    public async Task<ImageDataResult> GetOriginalAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await OriginalImageLoader.LoadAsync(
                entry,
                cancellationToken);

            return new ImageDataResult(
                data.Bytes,
                data.Metadata,
                AppInvariantStringConstants.SourceOriginal,
                false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load original image \"{entry.FileName}\".",
                ex);
        }
    }

    /// <summary>
    /// Warms the cache by pre-loading eligible reduced images.
    /// </summary>
    /// <param name="entries">The entries to process.</param>
    /// <param name="metrics">The screen metrics used to size reduced images.</param>
    /// <param name="progress">
    /// The progress reporter for cache warming completion.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when warming finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task WarmAllAsync(
        IReadOnlyList<ImageEntry> entries,
        ScreenMetrics metrics,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        if (_mode == WarmthMode.Cold)
        {
            progress?.Report(AppNumericConstants.ProgressMaximum);

            return;
        }

        var eligible = new List<ImageEntry>(entries.Count);
        foreach (var entry in entries)
        {
            if (entry.IsDiskCacheEligible)
            {
                eligible.Add(entry);
            }
        }

        if (eligible.Count == 0)
        {
            progress?.Report(AppNumericConstants.ProgressMaximum);

            return;
        }

        var processed = 0;
        long budget = 0;
        var pending = new List<ImageEntry>(eligible.Count);
        foreach (var entry in eligible)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var key = entry.CacheKey;
                var existing = await _fusionCache.TryGetAsync<byte[]>(
                    key,
                    _cacheOptions,
                    cancellationToken);

                if (existing.HasValue)
                {
                    EnsureMetadata(
                        key,
                        existing.Value);
                    processed++;
                    progress?.Report((double)processed / eligible.Count);

                    budget += existing.Value.Length;
                    if (budget >= AppNumericConstants.PreloadRamBudgetBytes)
                    {
                        progress?.Report(AppNumericConstants.ProgressMaximum);

                        return;
                    }

                    continue;
                }

                pending.Add(entry);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogBackgroundError(
                    ex,
                    entry.CacheKey);

                pending.Add(entry);
            }
        }

        foreach (var entry in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (budget >= AppNumericConstants.PreloadRamBudgetBytes)
            {
                break;
            }

            try
            {
                var key = entry.CacheKey;
                var bytes = await _fusionCache.GetOrSetAsync<byte[]>(
                    key,
                    async (_, token) =>
                    {
                        var data = await ImageReducer.CreateReducedAsync(
                            entry,
                            metrics,
                            token);
                        _metadataCache[key] = data.Metadata;

                        return data.Bytes;
                    },
                    _cacheOptions,
                    cancellationToken);
                EnsureMetadata(
                    key,
                    bytes);
                processed++;
                progress?.Report((double)processed / eligible.Count);

                budget += bytes.Length;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogBackgroundError(
                    ex,
                    entry.CacheKey);
            }
        }

        progress?.Report(AppNumericConstants.ProgressMaximum);
    }

    private static void LogBackgroundError(
        Exception exception,
        string cacheKey)
    {
        Log.Error(
            exception,
            "Cache pipeline error for key \"{CacheKey}\".",
            cacheKey);
    }

    private ImageMetadata EnsureMetadata(
        string key,
        byte[] bytes)
    {
        if (_metadataCache.TryGetValue(key, out var metadata))
        {
            return metadata;
        }

        var computed = ImageMetadataReader.FromBytes(bytes);
        _metadataCache[key] = computed;

        return computed;
    }
}