// <copyright file="AkavacheDistributedCacheAdapter.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Reactive.Threading.Tasks;

using Akavache;

using Microsoft.Extensions.Caching.Distributed;

namespace FastImageViewer.Caching;

internal sealed class AkavacheDistributedCacheAdapter(IBlobCache blobCache) : IDistributedCache
{
    private const int DefaultCacheExpirationTimeout = 14;

    private readonly IBlobCache _blobCache = blobCache;

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
            return null;
        }
    }

    /// <inheritdoc/>
    public void Refresh(string key)
    {
        // TODO: Implement if possible KISS or remove this comment.
    }

    /// <inheritdoc/>
    public Task RefreshAsync(
        string key,
        CancellationToken token = default)
    {
        // TODO: Implement if possible KISS or remove this comment.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Remove(string key)
    {
        _blobCache.Invalidate(key);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(
        string key,
        CancellationToken token = default)
    {
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
        var expiration = ResolveExpiration(options);

        _blobCache.InsertObject(
            key,
            value,
            expiration);
    }

    /// <inheritdoc/>
    public Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        var expiration = ResolveExpiration(options);

        return _blobCache
            .InsertObject(
                key,
                value,
                expiration)
            .ToTask(token);
    }

    private static DateTimeOffset? ResolveExpiration(DistributedCacheEntryOptions options)
    {
        return
            options
                .AbsoluteExpirationRelativeToNow
                .HasValue
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
}