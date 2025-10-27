// <copyright file="ImageCachePipeline.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Concurrent;

using FastImageViewer.ImageProcessing.Imaging;
using FastImageViewer.Resources;

using Serilog;

using ZiggyCreatures.Caching.Fusion;

namespace FastImageViewer.Cache;

/// <summary>
/// Implements image caching logic across memory and distributed caches.
/// </summary>
/// <param name="fusionCache">The fusion cache used to store image data.</param>
public sealed class ImageCachePipeline(
    IFusionCache fusionCache) : ICachePipeline
{
    private static readonly TimeSpan MemoryDuration = TimeSpan.FromMinutes(AppNumericConstants.MemoryCacheDurationMinutes);
    private static readonly TimeSpan JitterMaxDuration = TimeSpan.FromMinutes(AppNumericConstants.MemoryCacheJitterMinutes);
    private static readonly TimeSpan FailSafeMaxDuration = TimeSpan.FromHours(AppNumericConstants.MemoryCacheFailSafeHours);
    private static readonly TimeSpan DistributedCacheDuration = TimeSpan.FromDays(AppNumericConstants.MemoryCacheDistributedDays);

    private readonly IFusionCache _fusionCache = fusionCache;
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
        if (!entry.IsDiskCacheEligible)
        {
            return null;
        }

        var key = entry.CacheKey;
        try
        {
            var bytes = await GetCachedReducedBytesAsync(
                    entry,
                    metrics,
                    cancellationToken)
                .ConfigureAwait(false);
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
        var eligible = CollectEligibleEntries(
            entries,
            cancellationToken);
        if (eligible.Count == 0)
        {
            ReportWarmupCompletion(progress);

            return;
        }

        var state = CreateWarmupState(
            eligible.Count,
            progress);

        await WarmEligibleEntriesAsync(
                eligible,
                metrics,
                state,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static List<ImageEntry> CollectEligibleEntries(
        IReadOnlyList<ImageEntry> entries,
        CancellationToken cancellationToken)
    {
        var eligible = new List<ImageEntry>(entries.Count);
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.IsDiskCacheEligible)
            {
                eligible.Add(entry);
            }
        }

        return eligible;
    }

    private static WarmupState CreateWarmupState(
        int total,
        IProgress<double>? progress)
    {
        return new WarmupState(
            total,
            progress);
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

    private static void ReportWarmupCompletion(IProgress<double>? progress)
    {
        progress?.Report(AppNumericConstants.ProgressMaximum);
    }

    private async Task WarmEligibleEntriesAsync(
        IReadOnlyList<ImageEntry> eligible,
        ScreenMetrics metrics,
        WarmupState state,
        CancellationToken cancellationToken)
    {
        var pending = await ProcessCachedEntriesAsync(
            eligible,
            state,
            cancellationToken);

        if (state.IsBudgetExceeded)
        {
            state.ReportCompletion();

            return;
        }

        if (pending.Count > 0)
        {
            await WarmPendingEntriesAsync(
                pending,
                metrics,
                state,
                cancellationToken);
        }

        state.ReportCompletion();
    }

    private async Task<List<ImageEntry>> ProcessCachedEntriesAsync(
        IReadOnlyList<ImageEntry> eligible,
        WarmupState state,
        CancellationToken cancellationToken)
    {
        var pending = new List<ImageEntry>(eligible.Count);
        foreach (var entry in eligible)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var existing = await _fusionCache.TryGetAsync<byte[]>(
                    entry.CacheKey,
                    _cacheOptions,
                    cancellationToken);

                if (!existing.HasValue)
                {
                    pending.Add(entry);

                    continue;
                }

                EnsureMetadata(
                    entry.CacheKey,
                    existing.Value);

                if (state.RegisterProcessed(existing.Value))
                {
                    break;
                }
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

        return pending;
    }

    private async Task WarmPendingEntriesAsync(
        IReadOnlyList<ImageEntry> pending,
        ScreenMetrics metrics,
        WarmupState state,
        CancellationToken cancellationToken)
    {
        foreach (var entry in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (state.IsBudgetExceeded)
            {
                break;
            }

            try
            {
                var bytes = await GetCachedReducedBytesAsync(
                    entry,
                    metrics,
                    cancellationToken);

                EnsureMetadata(
                    entry.CacheKey,
                    bytes);

                if (state.RegisterProcessed(bytes))
                {
                    break;
                }
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
    }

    private ValueTask<byte[]> GetCachedReducedBytesAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        return _fusionCache.GetOrSetAsync<byte[]>(
            entry.CacheKey,
            async (_, token) => await CreateReducedBytesAsync(
                entry,
                metrics,
                token),
            _cacheOptions,
            cancellationToken);
    }

    private async Task<byte[]> CreateReducedBytesAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        var data = await ImageReducer.CreateReducedAsync(
            entry,
            metrics,
            cancellationToken);
        _metadataCache[entry.CacheKey] = data.Metadata;

        return data.Bytes;
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

    private sealed class WarmupState(
        int total,
        IProgress<double>? progress)
    {
        private readonly int _total = total;
        private readonly IProgress<double>? _progress = progress;

        public long Budget { get; private set; }

        public bool IsBudgetExceeded => Budget >= AppNumericConstants.PreloadRamBudgetBytes;

        private int Processed { get; set; }

        public bool RegisterProcessed(byte[] bytes)
        {
            Processed++;
            if (_total > 0)
            {
                _progress?.Report((double)Processed / _total);
            }

            Budget += bytes.LongLength;

            return IsBudgetExceeded;
        }

        public void ReportCompletion()
        {
            _progress?.Report(AppNumericConstants.ProgressMaximum);
        }
    }
}