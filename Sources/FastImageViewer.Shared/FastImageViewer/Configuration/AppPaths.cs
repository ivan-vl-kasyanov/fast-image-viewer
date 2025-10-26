// <copyright file="AppPaths.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;

namespace FastImageViewer.Shared.FastImageViewer.Configuration;

/// <summary>
/// Provides lazily initialized application paths for gallery and cache content.
/// </summary>
public static class AppPaths
{
    private static readonly Lock _syncRoot = new();
    private static string? _galleryDirectory;
    private static string? _cacheDirectory;

    /// <summary>
    /// Gets the path to the gallery directory, creating it and its README file if needed.
    /// </summary>
    /// <returns>The absolute path to the gallery directory.</returns>
    public static string GalleryDirectory
    {
        get
        {
            if (_galleryDirectory is not null)
            {
                return _galleryDirectory;
            }

            lock (_syncRoot)
            {
                if (_galleryDirectory is not null)
                {
                    return _galleryDirectory!;
                }

                var root = Path.Combine(
                    AppContext.BaseDirectory,
                    AppConstants.GalleryFolderName);
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                var readmePath = Path.Combine(
                    root,
                    AppConstants.GalleryReadmeFileName);
                if (!File.Exists(readmePath))
                {
                    File.WriteAllText(
                        readmePath,
                        AppConstants.GalleryReadmeContent);
                }

                _galleryDirectory = root;

                return _galleryDirectory;
            }
        }
    }

    /// <summary>
    /// Gets the path to the cache directory, creating it if necessary.
    /// </summary>
    /// <returns>The absolute path to the cache directory.</returns>
    public static string CacheDirectory
    {
        get
        {
            if (_cacheDirectory is not null &&
                Directory.Exists(_cacheDirectory))
            {
                return _cacheDirectory;
            }

            lock (_syncRoot)
            {
                if (_cacheDirectory is not null &&
                    Directory.Exists(_cacheDirectory))
                {
                    return _cacheDirectory!;
                }

                var root = Path.Combine(
                    AppContext.BaseDirectory,
                    AppConstants.CacheFolderName);
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                _cacheDirectory = root;

                return _cacheDirectory;
            }
        }
    }
}