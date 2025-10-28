// <copyright file="ApplicationPaths.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;

namespace FastImageViewer.Shared.FastImageViewer.Configuration;

/// <summary>
/// Provides lazily initialized application paths for gallery and cache content.
/// </summary>
public sealed class ApplicationPaths : IApplicationPaths
{
    private readonly SemaphoreSlim _syncRoot = new(1, 1);

    private string? _galleryDirectory;

    private string? _cacheDirectory;

    /// <inheritdoc/>
    public string GalleryDirectory
    {
        get
        {
            if (_galleryDirectory is not null)
            {
                return _galleryDirectory;
            }

            _syncRoot.Wait();
            try
            {
                if (_galleryDirectory is not null)
                {
                    return _galleryDirectory!;
                }

                var root = Path.Combine(
                    AppContext.BaseDirectory,
                    AppInvariantStringConstants.GalleryFolderName);
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                _galleryDirectory = root;

                return _galleryDirectory;
            }
            finally
            {
                _syncRoot.Release();
            }
        }
    }

    /// <inheritdoc/>
    public string CacheDirectory
    {
        get
        {
            if ((_cacheDirectory is not null) &&
                Directory.Exists(_cacheDirectory))
            {
                return _cacheDirectory;
            }

            _syncRoot.Wait();
            try
            {
                if ((_cacheDirectory is not null) &&
                    Directory.Exists(_cacheDirectory))
                {
                    return _cacheDirectory!;
                }

                var root = Path.Combine(
                    AppContext.BaseDirectory,
                    AppInvariantStringConstants.CacheFolderName);
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                _cacheDirectory = root;

                return _cacheDirectory;
            }
            finally
            {
                _syncRoot.Release();
            }
        }
    }
}