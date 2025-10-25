// <copyright file="AppConstants.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Configuration;

/// <summary>
/// Provides constant values used throughout the application.
/// </summary>
internal static class AppConstants
{
    /// <summary>
    /// The application name shared across caches and logs.
    /// </summary>
    public const string AppName = "FastImageViewer";

    /// <summary>
    /// The folder name that stores gallery images.
    /// </summary>
    public const string GalleryFolderName = "Gallery";

    /// <summary>
    /// The folder name that stores cache content.
    /// </summary>
    public const string CacheFolderName = "Cache";

    /// <summary>
    /// The file name used for the local machine cache database.
    /// </summary>
    public const string LocalMachineCacheFileName = "LocalMachine.db";

    /// <summary>
    /// The file name for the auto-generated gallery readme file.
    /// </summary>
    public const string GalleryReadmeFileName = "README.gallery.txt";

    /// <summary>
    /// The contents of the auto-generated gallery readme file.
    /// </summary>
    public const string GalleryReadmeContent = "Copy images into this folder to browse them with FastImageViewer.";

    /// <summary>
    /// The separator character used when building cache keys.
    /// </summary>
    public const char CacheKeySeparator = '|';

    /// <summary>
    /// The suffix appended to cache keys that store original images.
    /// </summary>
    public const string OriginalCacheSuffix = ":original";

    /// <summary>
    /// The target quality percentage for reduced images.
    /// </summary>
    public const int ReducedQuality = 75;

    /// <summary>
    /// The size threshold in bytes that makes images eligible for disk caching.
    /// </summary>
    public const long DiskCacheEligibilityThresholdBytes = 1_048_576;

    /// <summary>
    /// The default DPI used when metadata is unavailable.
    /// </summary>
    public const double DefaultDpi = 96d;

    /// <summary>
    /// The RAM budget in bytes used when pre-loading images.
    /// </summary>
    public const long PreloadRamBudgetBytes = 160L * 1024L * 1024L;

    /// <summary>
    /// The file name for the rolling log file.
    /// </summary>
    public const string LogFileName = "FastImageViewer.log";

    /// <summary>
    /// The label indicating data came from the original image.
    /// </summary>
    public const string SourceOriginal = "Original";

    /// <summary>
    /// The label indicating data came from cache.
    /// </summary>
    public const string SourceCache = "Cache";

    /// <summary>
    /// The fallback window title when no file is selected.
    /// </summary>
    public const string WindowTitleFallback = "Fast Image Viewer";

    /// <summary>
    /// The toggle button text for showing the original image.
    /// </summary>
    public const string ToggleShowOriginal = "Show Original";

    /// <summary>
    /// The toggle button text for showing the reduced image.
    /// </summary>
    public const string ToggleShowReduced = "Show Reduced";
}