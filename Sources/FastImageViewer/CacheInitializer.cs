// <copyright file="CacheInitializer.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Reactive.Concurrency;

using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;

using FastImageViewer.Resources;

namespace FastImageViewer;

/// <summary>
/// Provides helpers for configuring the Akavache cache providers.
/// </summary>
internal static class CacheInitializer
{
    /// <summary>
    /// Configures the cache providers for the current process.
    /// </summary>
    /// <param name="cacheDirectory">The directory that stores cache state.</param>
    public static void Configure(string cacheDirectory)
    {
        var previewImageCachePath = Path.Combine(
            cacheDirectory,
            AppInvariantStringConstants.PreviewImageCacheName);
        var scheduler = CacheDatabase.TaskpoolScheduler ?? TaskPoolScheduler.Default;
        var serializer = new SystemJsonSerializer();
        var customCache = new SqliteBlobCache(
            previewImageCachePath,
            serializer,
            scheduler,
            false);

        Splat
            .Builder
            .AppBuilder
            .CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(builder =>
            {
                builder
                    .WithApplicationName(AppInvariantStringConstants.AppName)
                    .WithSqliteProvider()
                    .WithSqliteDefaults()
                    .WithLocalMachine(customCache);
            });
    }
}