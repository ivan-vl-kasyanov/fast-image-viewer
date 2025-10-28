// <copyright file="ImageCachePipeline.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Concurrent;

using FastImageViewer.Cache.Models;
using FastImageViewer.ImageProcessing.Imaging;
using FastImageViewer.ImageProcessing.Models;
using FastImageViewer.Resources;

using Serilog;

using ZiggyCreatures.Caching.Fusion;

namespace FastImageViewer.Cache;

/// <summary>
/// Implements image caching logic across memory and distributed caches.
/// </summary>
/// <param name="fusionCache">The fusion cache used to store image data.</param>
/// <param name="imageReducer">The reducer used to generate reduced images.</param>
/// <param name="originalImageLoader">The loader used for original images.</param>
public sealed class ImageCachePipeline(
    IFusionCache fusionCache,
    IImageReducer imageReducer,
    IOriginalImageLoader originalImageLoader) : ICachePipeline
{
    private static readonly TimeSpan MemoryDuration = TimeSpan.FromMinutes(AppNumericConstants.MemoryCacheDurationMinutes);
    private static readonly TimeSpan JitterMaxDuration = TimeSpan.FromMinutes(AppNumericConstants.MemoryCacheJitterMinutes);
    private static readonly TimeSpan FailSafeMaxDuration = TimeSpan.FromHours(AppNumericConstants.MemoryCacheFailSafeHours);
    private static readonly TimeSpan DistributedCacheDuration = TimeSpan.FromDays(AppNumericConstants.MemoryCacheDistributedDays);

    private readonly IFusionCache _fusionCache = fusionCache;
    private readonly IImageReducer _imageReducer = imageReducer;
    private readonly IOriginalImageLoader _originalImageLoader = originalImageLoader;

    private readonly Lock _disposeLock = new();
    private readonly ConcurrentDictionary<string, ImageMetadata> _metadataCache = new();

    private readonly FusionCacheEntryOptions _cacheOptions = new()
    {
        Duration = MemoryDuration,
        JitterMaxDuration = JitterMaxDuration,
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = FailSafeMaxDuration,
        DistributedCacheDuration = DistributedCacheDuration,
    };

    private bool _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_disposeLock)
        {
            if (_disposed)
            {
                return;
            }

            _fusionCache.Dispose();

            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<ImageDataResult> GetOriginalAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _originalImageLoader.LoadAsync(
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

    /// <inheritdoc/>
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
            eligible,
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
        IReadOnlyList<ImageEntry> entries,
        IProgress<double>? progress)
    {
        return new WarmupState(
            entries,
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

                if (state.RegisterProcessed(
                        entry,
                        existing.Value))
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

                if (state.RegisterProcessed(
                        entry,
                        bytes))
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
        var data = await _imageReducer.CreateReducedAsync(
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

        var computed = bytes.GetImageMetadata();
        _metadataCache[key] = computed;

        return computed;
    }

    private sealed class WarmupState(
        IReadOnlyList<ImageEntry> entries,
        IProgress<double>? progress)
    {
        private readonly long _totalLength = CalculateTotalLength(entries);
        private readonly int _totalCount = entries.Count;
        private readonly IProgress<double>? _progress = progress;

        private long _budget;

        private long _processedLength;

        private int _processedCount;

        public bool IsBudgetExceeded => _budget >= AppNumericConstants.PreloadRamBudgetBytes;

        public bool RegisterProcessed(
            ImageEntry entry,
            byte[] bytes)
        {
            _processedCount++;

            _budget += bytes.LongLength;

            if (_totalLength > 0)
            {
                var length = entry.LengthBytes;
                if (length > 0)
                {
                    if (long.MaxValue - _processedLength < length)
                    {
                        _processedLength = long.MaxValue;
                    }
                    else
                    {
                        _processedLength += length;
                    }
                }

                var progression = Math.Min(
                    (double)_processedLength / _totalLength,
                    AppNumericConstants.ProgressMaximum);
                _progress?.Report(progression);
            }
            else if (_totalCount > 0)
            {
                _progress?.Report((double)_processedCount / _totalCount);
            }

            return IsBudgetExceeded;
        }

        public void ReportCompletion()
        {
            _progress?.Report(AppNumericConstants.ProgressMaximum);
        }

        private static long CalculateTotalLength(IReadOnlyList<ImageEntry> entries)
        {
            var total = 0L;
            foreach (var length in entries.Select(static entry => entry.LengthBytes))
            {
                if (length <= 0)
                {
                    continue;
                }

                if (long.MaxValue - total < length)
                {
                    return long.MaxValue;
                }

                total += length;
            }

            return total;
        }
    }
}