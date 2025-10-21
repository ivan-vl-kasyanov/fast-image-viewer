// <copyright file="FusionCacheFactory.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Configuration;

using Microsoft.Extensions.Caching.Distributed;

using ZiggyCreatures.Caching.Fusion;

namespace FastImageViewer.Caching;

internal static class FusionCacheFactory
{
    internal static IFusionCache Create(IDistributedCache distributedCache)
    {
        var options = new FusionCacheOptions
        {
            CacheName = AppConstants.AppName,
            DistributedCache = distributedCache,
            DefaultEntryOptions = new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(15),
                JitterMaxDuration = TimeSpan.FromMinutes(2),
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromHours(1),
                DistributedCacheDuration = TimeSpan.FromDays(30),
            },
        };

        return new FusionCache(options);
    }
}