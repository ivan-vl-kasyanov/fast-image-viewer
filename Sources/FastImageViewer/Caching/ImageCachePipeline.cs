// <copyright file="ImageCachePipeline.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Concurrent;

using FastImageViewer.Configuration;
using FastImageViewer.Imaging;
using FastImageViewer.Text;

using Serilog;

using ZiggyCreatures.Caching.Fusion;

namespace FastImageViewer.Caching;

internal sealed class ImageCachePipeline(
    IFusionCache fusionCache,
    WarmthMode mode) : ICachePipeline
{
    private const int MemoryDurationMinutes = 20;
    private const int JitterMaxDurationMinutes = 2;
    private const int FailSafeMaxDurationHours = 1;
    private const int DistributedCacheDurationDays = 30;
    private const int MemoryOriginalDurationMinutes = 20;
    private const int JitterOriginalMaxDurationSeconds = 30;

    private static readonly TimeSpan MemoryDuration = TimeSpan.FromMinutes(MemoryDurationMinutes);
    private static readonly TimeSpan JitterMaxDuration = TimeSpan.FromMinutes(JitterMaxDurationMinutes);
    private static readonly TimeSpan FailSafeMaxDuration = TimeSpan.FromHours(FailSafeMaxDurationHours);
    private static readonly TimeSpan DistributedCacheDuration = TimeSpan.FromDays(DistributedCacheDurationDays);
    private static readonly TimeSpan MemoryOriginalDuration = TimeSpan.FromMinutes(MemoryOriginalDurationMinutes);
    private static readonly TimeSpan JitterOriginalMaxDuration = TimeSpan.FromSeconds(JitterOriginalMaxDurationSeconds);

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
                NonAllocationStrings.SourceCache,
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

    public async Task<ImageDataResult> GetOriginalAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        var key = entry.CacheKey + AppConstants.OriginalCacheSuffix;

        try
        {
            if (_mode == WarmthMode.Cold)
            {
                var data = await OriginalImageLoader.LoadAsync(
                    entry,
                    cancellationToken);

                return new ImageDataResult(
                    data.Bytes,
                    data.Metadata,
                    NonAllocationStrings.SourceOriginal,
                    false);
            }

            var bytes = await _fusionCache.GetOrSetAsync<byte[]>(
                key,
                async (_, token) =>
                {
                    var data = await OriginalImageLoader.LoadAsync(
                        entry,
                        token);
                    _metadataCache[key] = data.Metadata;

                    return data.Bytes;
                },
                _originalOptions,
                cancellationToken);
            var metadata = EnsureMetadata(
                key,
                bytes);

            return new ImageDataResult(
                bytes,
                metadata,
                NonAllocationStrings.SourceCache,
                false);
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

            throw new InvalidOperationException(
                $"Failed to load original image \"{entry.FileName}\".",
                ex);
        }
    }

    public async Task WarmAllAsync(
        IReadOnlyList<ImageEntry> entries,
        ScreenMetrics metrics,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        if (_mode == WarmthMode.Cold)
        {
            progress?.Report(1);

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
            progress?.Report(1);

            return;
        }

        var processed = 0;
        foreach (var entry in eligible)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

        long budget = 0;
        var cacheKeys = eligible.Select(entry => entry.CacheKey);
        foreach (var key in cacheKeys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var existing = await _fusionCache.TryGetAsync<byte[]>(
                    key,
                    _cacheOptions,
                    cancellationToken);
                if (!existing.HasValue)
                {
                    continue;
                }

                budget += existing.Value.Length;
                if (budget >= AppConstants.PreloadRamBudgetBytes)
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
                    key);
            }
        }

        progress?.Report(1);
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