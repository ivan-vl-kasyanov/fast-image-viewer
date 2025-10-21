// <copyright file="FusionCacheFactory.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Configuration;

using Microsoft.Extensions.Caching.Distributed;

using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace FastImageViewer.Caching;

internal static class FusionCacheFactory
{
    private const int CacheDurationMinutes = 15;
    private const int CacheJitterMinutes = 2;
    private const int FailSafeDurationHours = 1;
    private const int DistributedCacheDurationDays = 30;

    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(CacheDurationMinutes);
    private static readonly TimeSpan _cacheJitter = TimeSpan.FromMinutes(CacheJitterMinutes);
    private static readonly TimeSpan _failSafeDuration = TimeSpan.FromHours(FailSafeDurationHours);
    private static readonly TimeSpan _distributedCacheDuration = TimeSpan.FromDays(DistributedCacheDurationDays);

    internal static IFusionCache Create(IDistributedCache distributedCache)
    {
        var options = new FusionCacheOptions
        {
            CacheName = AppConstants.AppName,
            DefaultEntryOptions = new FusionCacheEntryOptions
            {
                Duration = _cacheDuration,
                JitterMaxDuration = _cacheJitter,
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = _failSafeDuration,
                DistributedCacheDuration = _distributedCacheDuration,
            },
        };

        var cache = new FusionCache(options);
        cache.SetupSerializer(new FusionCacheSystemTextJsonSerializer());
        cache.SetupDistributedCache(distributedCache);

        return cache;
    }
}