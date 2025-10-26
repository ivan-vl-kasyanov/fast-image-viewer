// <copyright file="AkavacheDistributedCacheAdapter.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Concurrent;
using System.Reactive.Threading.Tasks;

using Akavache;

using Microsoft.Extensions.Caching.Distributed;

namespace FastImageViewer.Cache;

/// <summary>
/// Adapts an <see cref="IBlobCache"/> instance to the <see cref="IDistributedCache"/> contract.
/// </summary>
/// <param name="blobCache">The underlying Akavache blob cache.</param>
public sealed class AkavacheDistributedCacheAdapter(IBlobCache blobCache) : IDistributedCache
{
    private const int DefaultCacheExpirationTimeout = 14;

    private readonly IBlobCache _blobCache = blobCache;
    private readonly ConcurrentDictionary<string, CacheEntryOptionsSnapshot> _options = new();

    /// <inheritdoc/>
    public byte[]? Get(string key)
    {
        return GetAsync(key)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc/>
    public async Task<byte[]?> GetAsync(
        string key,
        CancellationToken token = default)
    {
        try
        {
            return await _blobCache
                .GetObject<byte[]>(key)
                .ToTask(token);
        }
        catch (KeyNotFoundException)
        {
            _options.TryRemove(
                key,
                out _);

            return null;
        }
    }

    /// <inheritdoc/>
    public void Refresh(string key)
    {
        var value = Get(key);
        if (value is null)
        {
            return;
        }

        var expiration = ResolveExpiration(key);

        WaitForCompletion(_blobCache
            .InsertObject(
                key,
                value,
                expiration)
            .ToTask());
    }

    /// <inheritdoc/>
    public Task RefreshAsync(
        string key,
        CancellationToken token = default)
    {
        return RefreshInternalAsync(
            key,
            token);
    }

    /// <inheritdoc/>
    public void Remove(string key)
    {
        _options.TryRemove(
            key,
            out _);

        WaitForCompletion(_blobCache
            .Invalidate(key)
            .ToTask());
    }

    /// <inheritdoc/>
    public Task RemoveAsync(
        string key,
        CancellationToken token = default)
    {
        _options.TryRemove(
            key,
            out _);

        return _blobCache
            .Invalidate(key)
            .ToTask(token);
    }

    /// <inheritdoc/>
    public void Set(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options)
    {
        var snapshot = CreateSnapshot(options);
        var expiration = ResolveExpiration(snapshot);

        _options[key] = snapshot;

        WaitForCompletion(_blobCache
            .InsertObject(
                key,
                value,
                expiration)
            .ToTask());
    }

    /// <inheritdoc/>
    public Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        var snapshot = CreateSnapshot(options);
        var expiration = ResolveExpiration(snapshot);

        _options[key] = snapshot;

        return _blobCache
            .InsertObject(
                key,
                value,
                expiration)
            .ToTask(token);
    }

    private static CacheEntryOptionsSnapshot CreateSnapshot(DistributedCacheEntryOptions options)
    {
        return new CacheEntryOptionsSnapshot(
            options.AbsoluteExpiration,
            options.AbsoluteExpirationRelativeToNow,
            options.SlidingExpiration);
    }

    private static DateTimeOffset? ResolveExpiration(CacheEntryOptionsSnapshot options)
    {
        return
            options.AbsoluteExpirationRelativeToNow.HasValue
                ? DateTimeOffset
                    .UtcNow
                    .Add(options.AbsoluteExpirationRelativeToNow.Value)
                : options.AbsoluteExpiration ?? (options.SlidingExpiration.HasValue
                    ? DateTimeOffset
                        .UtcNow
                        .Add(options.SlidingExpiration.Value)
                    : DateTimeOffset
                        .UtcNow
                        .AddDays(DefaultCacheExpirationTimeout));
    }

    private static void WaitForCompletion(Task task)
    {
        task
            .GetAwaiter()
            .GetResult();
    }

    private DateTimeOffset? ResolveExpiration(string key)
    {
        var snapshot = _options.TryGetValue(key, out var cached)
            ? cached
            : CacheEntryOptionsSnapshot.Default;

        return ResolveExpiration(snapshot);
    }

    private async Task RefreshInternalAsync(
        string key,
        CancellationToken token)
    {
        var value = await GetAsync(
            key,
            token);
        if (value is null)
        {
            return;
        }

        var expiration = ResolveExpiration(key);
        await _blobCache
            .InsertObject(
                key,
                value,
                expiration)
            .ToTask(token);
    }

    private readonly struct CacheEntryOptionsSnapshot(
        DateTimeOffset? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        TimeSpan? slidingExpiration)
    {
        public static CacheEntryOptionsSnapshot Default { get; } = new(null, null, null);

        public DateTimeOffset? AbsoluteExpiration { get; } = absoluteExpiration;

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; } = absoluteExpirationRelativeToNow;

        public TimeSpan? SlidingExpiration { get; } = slidingExpiration;
    }
}