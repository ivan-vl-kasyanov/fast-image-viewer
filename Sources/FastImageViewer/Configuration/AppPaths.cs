// <copyright file="AppPaths.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Configuration;

internal static class AppPaths
{
    private static readonly Lock _syncRoot = new();
    private static string? _galleryDirectory;
    private static string? _cacheDirectory;

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

    public static string CacheDirectory
    {
        get
        {
            if ((_cacheDirectory is not null) &&
                Directory.Exists(_cacheDirectory))
            {
                return _cacheDirectory;
            }

            lock (_syncRoot)
            {
                if ((_cacheDirectory is not null) &&
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