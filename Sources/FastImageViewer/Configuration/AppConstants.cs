// <copyright file="AppConstants.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Configuration;

internal static class AppConstants
{
    public const string AppName = "FastImageViewer";

    public const string GalleryFolderName = "Gallery";

    public const string CacheFolderName = "Cache";

    public const string LocalMachineCacheFileName = "LocalMachine.db";

    public const string GalleryReadmeFileName = "README.gallery.txt";

    public const string GalleryReadmeContent = "Copy images into this folder to browse them with FastImageViewer.";

    public const char CacheKeySeparator = '|';

    public const string OriginalCacheSuffix = ":original";

    public const int ReducedQuality = 75;

    public const long DiskCacheEligibilityThresholdBytes = 1_048_576;

    public const double DefaultDpi = 96d;

    public const long PreloadRamBudgetBytes = 160L * 1024L * 1024L;

    public const string LogFileName = "FastImageViewer.log";
}