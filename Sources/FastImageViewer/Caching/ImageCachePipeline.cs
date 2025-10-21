// <copyright file="ImageCachePipeline.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Concurrent;
using System.Diagnostics;

using FastImageViewer.Configuration;
using FastImageViewer.Diagnostics;
using FastImageViewer.Imaging;
using FastImageViewer.Text;

using Microsoft.Extensions.Caching.Distributed;

using ZiggyCreatures.Caching.Fusion;

namespace FastImageViewer.Caching;

internal sealed class ImageCachePipeline(
    IFusionCache fusionCache,
    IDistributedCache distributedCache,
    PerformanceLogger logger,
    WarmthMode mode) : ICachePipeline
{
    private const int AbsoluteExpirationTimeDays = 30;
    private const int MemoryDurationMinutes = 20;
    private const int JitterMaxDurationMinutes = 2;
    private const int FailSafeMaxDurationHours = 1;
    private const int DistributedCacheDurationDays = 30;
    private const int MemoryOriginalDurationMinutes = 20;
    private const int JitterOriginalMaxDurationSeconds = 30;

    private static readonly TimeSpan AbsoluteExpirationTime = TimeSpan.FromDays(AbsoluteExpirationTimeDays);
    private static readonly TimeSpan MemoryDuration = TimeSpan.FromMinutes(MemoryDurationMinutes);
    private static readonly TimeSpan JitterMaxDuration = TimeSpan.FromMinutes(JitterMaxDurationMinutes);
    private static readonly TimeSpan FailSafeMaxDuration = TimeSpan.FromHours(FailSafeMaxDurationHours);
    private static readonly TimeSpan DistributedCacheDuration = TimeSpan.FromDays(DistributedCacheDurationDays);
    private static readonly TimeSpan MemoryOriginalDuration = TimeSpan.FromMinutes(MemoryOriginalDurationMinutes);
    private static readonly TimeSpan JitterOriginalMaxDuration = TimeSpan.FromSeconds(JitterOriginalMaxDurationSeconds);

    private readonly IFusionCache _fusionCache = fusionCache;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly PerformanceLogger _logger = logger;
    private readonly WarmthMode _mode = mode;
    private readonly ConcurrentDictionary<string, ImageMetadata> _metadataCache = new();

    private readonly DistributedCacheEntryOptions _distributedOptions = new()
    {
        AbsoluteExpirationRelativeToNow = AbsoluteExpirationTime,
    };

    private readonly FusionCacheEntryOptions _memoryOptions = new()
    {
        Duration = MemoryDuration,
        JitterMaxDuration = JitterMaxDuration,
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = FailSafeMaxDuration,
        DistributedCacheDuration = DistributedCacheDuration,
    };

    private readonly FusionCacheEntryOptions _originalOptions = new()
    {
        Duration = MemoryOriginalDuration,
        JitterMaxDuration = JitterOriginalMaxDuration,
        DistributedCacheDuration = TimeSpan.Zero,
    };

    public async Task<ImageDataResult?> GetReducedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        const string PerformanceOperationName = "Reduced";

        if (!entry.IsDiskCacheEligible)
        {
            return null;
        }

        var key = entry.CacheKey;
        var l1Watch = Stopwatch.StartNew();
        var maybe = await _fusionCache.TryGetAsync<byte[]>(
            key,
            _memoryOptions,
            cancellationToken);
        l1Watch.Stop();

        if (maybe.HasValue)
        {
            var metadata = EnsureMetadata(
                key,
                maybe.Value);
            _logger.LogDuration(
                new PerformanceMeasureInput(
                    PerformanceOperationName,
                    entry.FileName,
                    NonAllocationStrings.SourceL1,
                    _mode),
                metadata,
                l1Watch.Elapsed.TotalMilliseconds);

            return new ImageDataResult(
                maybe.Value,
                metadata,
                NonAllocationStrings.SourceL1,
                true);
        }

        var l2Watch = Stopwatch.StartNew();
        var fromDisk = await _distributedCache.GetAsync(
            key,
            cancellationToken);
        l2Watch.Stop();

        if (fromDisk is not null)
        {
            var metadata = EnsureMetadata(
                key,
                fromDisk);
            await _fusionCache.SetAsync(
                key,
                fromDisk,
                _memoryOptions,
                cancellationToken);
            _logger.LogDuration(
                new PerformanceMeasureInput(
                    PerformanceOperationName,
                    entry.FileName,
                    NonAllocationStrings.SourceL2,
                    _mode),
                metadata,
                l2Watch.Elapsed.TotalMilliseconds);

            return new ImageDataResult(
                fromDisk,
                metadata,
                NonAllocationStrings.SourceL2,
                true);
        }

        var created = await _logger.MeasureAsync(
            new PerformanceMeasureInput(
                PerformanceOperationName,
                entry.FileName,
                NonAllocationStrings.SourceCreated,
                _mode),
            async () =>
            {
                var data = await ImageReducer.CreateReducedAsync(
                    entry,
                    metrics,
                    cancellationToken);

                return new PerformanceMeasureResult<ImageData>(
                    data,
                    data.Metadata);
            });

        _metadataCache[key] = created.Metadata;
        await _distributedCache.SetAsync(
            key,
            created.Bytes,
            _distributedOptions,
            cancellationToken);
        await _fusionCache.SetAsync(
            key,
            created.Bytes,
            _memoryOptions,
            cancellationToken);

        return new ImageDataResult(
            created.Bytes,
            created.Metadata,
            NonAllocationStrings.SourceCreated,
            true);
    }

    public async Task<ImageDataResult> GetOriginalAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        const string PerformanceOperationName = "Original";

        var key = entry.CacheKey + AppConstants.OriginalCacheSuffix;
        var l1Watch = Stopwatch.StartNew();
        var maybe = await _fusionCache.TryGetAsync<byte[]>(
            key,
            _originalOptions,
            cancellationToken);
        l1Watch.Stop();

        if (maybe.HasValue)
        {
            var metadata = EnsureMetadata(
                key,
                maybe.Value);
            _logger.LogDuration(
                new PerformanceMeasureInput(
                    PerformanceOperationName,
                    entry.FileName,
                    NonAllocationStrings.SourceL1,
                    _mode),
                metadata,
                l1Watch.Elapsed.TotalMilliseconds);

            return new ImageDataResult(
                maybe.Value,
                metadata,
                NonAllocationStrings.SourceL1,
                false);
        }

        var data = await _logger.MeasureAsync(
            new PerformanceMeasureInput(
                PerformanceOperationName,
                entry.FileName,
                NonAllocationStrings.SourceOriginal,
                _mode),
            async () =>
            {
                var loaded = await OriginalImageLoader.LoadAsync(
                    entry,
                    cancellationToken);

                return new PerformanceMeasureResult<ImageData>(
                    loaded,
                    loaded.Metadata);
            });

        _metadataCache[key] = data.Metadata;
        await _fusionCache.SetAsync(
            key,
            data.Bytes,
            _originalOptions,
            cancellationToken);

        return new ImageDataResult(
            data.Bytes,
            data.Metadata,
            NonAllocationStrings.SourceOriginal,
            false);
    }

    public async Task WarmAllAsync(
        IReadOnlyList<ImageEntry> entries,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!entry.IsDiskCacheEligible)
            {
                continue;
            }

            await EnsureDistributedAsync(
                entry,
                metrics,
                cancellationToken);
        }

        long budget = 0;
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!entry.IsDiskCacheEligible)
            {
                continue;
            }

            if (budget >= AppConstants.PreloadRamBudgetBytes)
            {
                break;
            }

            var result = await GetReducedAsync(
                entry,
                metrics,
                cancellationToken);
            if (result is null)
            {
                continue;
            }

            budget += result.Bytes.Length;
        }
    }

    public async Task WarmNeighborsAsync(
        IReadOnlyList<ImageEntry> entries,
        int currentIndex,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        for (var offset = 1; offset <= AppConstants.NeighborWarmCount; offset++)
        {
            var previousIndex = currentIndex - offset;
            if ((previousIndex >= 0) &&
                (previousIndex < entries.Count))
            {
                await GetReducedAsync(
                    entries[previousIndex],
                    metrics,
                    cancellationToken);
            }

            var nextIndex = currentIndex + offset;
            if ((nextIndex >= 0) &&
                (nextIndex < entries.Count))
            {
                await GetReducedAsync(
                    entries[nextIndex],
                    metrics,
                    cancellationToken);
            }
        }
    }

    private async Task EnsureDistributedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        const string PerformanceOperationName = "Reduced";

        if (!entry.IsDiskCacheEligible)
        {
            return;
        }

        var existing = await _distributedCache.GetAsync(
            entry.CacheKey,
            cancellationToken);
        if (existing is not null)
        {
            _metadataCache.TryAdd(
                entry.CacheKey,
                EnsureMetadata(
                    entry.CacheKey,
                    existing));

            return;
        }

        var created = await _logger.MeasureAsync(
            new PerformanceMeasureInput(
                PerformanceOperationName,
                entry.FileName,
                NonAllocationStrings.SourceCreated,
                _mode),
            async () =>
            {
                var data = await ImageReducer.CreateReducedAsync(
                    entry,
                    metrics,
                    cancellationToken);

                return new PerformanceMeasureResult<ImageData>(
                    data,
                    data.Metadata);
            });

        _metadataCache[entry.CacheKey] = created.Metadata;
        await _distributedCache.SetAsync(
            entry.CacheKey,
            created.Bytes,
            _distributedOptions,
            cancellationToken);
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