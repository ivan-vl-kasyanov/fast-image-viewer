// <copyright file="CacheCleaner.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;

using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;

using FastImageViewer.Resources;

using Serilog;

namespace FastImageViewer;

/// <summary>
/// Provides cache maintenance helpers for clean start scenarios.
/// </summary>
internal static class CacheCleaner
{
    /// <summary>
    /// Clears cached data and removes persisted cache files when possible.
    /// </summary>
    /// <param name="cacheDirectory">The cache directory to clean.</param>
    public static void Run(string cacheDirectory)
    {
        InvalidateEntries(cacheDirectory);
        DeleteCacheDirectory(cacheDirectory);
    }

    private static void InvalidateEntries(string cacheDirectory)
    {
        try
        {
            var scheduler = CacheDatabase.TaskpoolScheduler ?? TaskPoolScheduler.Default;
            var localMachinePath = Path.Combine(
                cacheDirectory,
                AppInvariantStringConstants.LocalMachineCacheFileName);

            using var cache = new SqliteBlobCache(
                localMachinePath,
                new SystemJsonSerializer(),
                scheduler,
                false);

            cache
                .InvalidateAll()
                .ToTask()
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Failed to invalidate cache entries.");
        }
    }

    private static void DeleteCacheDirectory(string cacheDirectory)
    {
        try
        {
            if (Directory.Exists(cacheDirectory))
            {
                Directory.Delete(
                    cacheDirectory,
                    true);
            }
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Failed to delete cache directory \"{CacheDirectory}\".",
                cacheDirectory);
        }
    }
}