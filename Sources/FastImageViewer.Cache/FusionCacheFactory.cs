// <copyright file="FusionCacheFactory.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;

using Microsoft.Extensions.Caching.Distributed;

using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace FastImageViewer.Cache;

/// <summary>
/// Creates configured instances of <see cref="IFusionCache"/>.
/// </summary>
public static class FusionCacheFactory
{
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(AppNumericConstants.FusionCacheDurationMinutes);
    private static readonly TimeSpan _cacheJitter = TimeSpan.FromMinutes(AppNumericConstants.FusionCacheJitterMinutes);
    private static readonly TimeSpan _failSafeDuration = TimeSpan.FromHours(AppNumericConstants.FusionCacheFailSafeDurationHours);
    private static readonly TimeSpan _distributedCacheDuration = TimeSpan.FromDays(AppNumericConstants.FusionCacheDistributedDurationDays);

    /// <summary>
    /// Creates a configured <see cref="IFusionCache"/> backed by the provided distributed cache.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used for persistence.</param>
    /// <returns>The configured fusion cache instance.</returns>
    public static IFusionCache Create(IDistributedCache distributedCache)
    {
        var options = new FusionCacheOptions
        {
            CacheName = AppInvariantStringConstants.AppName,
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