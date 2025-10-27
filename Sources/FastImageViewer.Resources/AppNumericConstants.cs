// <copyright file="AppNumericConstants.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Resources;

/// <summary>
/// Provides numeric constant values used throughout the application.
/// </summary>
public static class AppNumericConstants
{
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
    /// The buffer size used when reading image files from disk.
    /// </summary>
    public const int ImageFileReadBufferSize = 16_384;

    /// <summary>
    /// The maximum size in bytes for a single rolling log file.
    /// </summary>
    public const long LogFileRollingSizeLimitBytes = 1_048_576;

    /// <summary>
    /// The number of retained historical log files.
    /// </summary>
    public const int LogFileRetainedCountLimit = 3;

    /// <summary>
    /// The cache duration used by fusion cache instances in minutes.
    /// </summary>
    public const int FusionCacheDurationMinutes = 15;

    /// <summary>
    /// The jitter duration used by fusion cache instances in minutes.
    /// </summary>
    public const int FusionCacheJitterMinutes = 2;

    /// <summary>
    /// The fail-safe duration used by fusion cache instances in hours.
    /// </summary>
    public const int FusionCacheFailSafeDurationHours = 1;

    /// <summary>
    /// The distributed cache duration used by fusion cache instances in days.
    /// </summary>
    public const int FusionCacheDistributedDurationDays = 30;

    /// <summary>
    /// The in-memory cache duration for image data in minutes.
    /// </summary>
    public const int MemoryCacheDurationMinutes = 20;

    /// <summary>
    /// The jitter duration for in-memory cache entries in minutes.
    /// </summary>
    public const int MemoryCacheJitterMinutes = 2;

    /// <summary>
    /// The fail-safe duration for in-memory cache entries in hours.
    /// </summary>
    public const int MemoryCacheFailSafeHours = 1;

    /// <summary>
    /// The distributed cache duration for in-memory cache entries in days.
    /// </summary>
    public const int MemoryCacheDistributedDays = 30;

    /// <summary>
    /// The default expiration timeout for distributed cache entries in days.
    /// </summary>
    public const int DefaultCacheExpirationDays = 14;

    /// <summary>
    /// The opacity representing a visible element.
    /// </summary>
    public const double OpacityVisible = 1d;

    /// <summary>
    /// The opacity representing a hidden element.
    /// </summary>
    public const double OpacityHidden = 0d;

    /// <summary>
    /// The minimum progress value used for UI clamps.
    /// </summary>
    public const double ProgressMinimum = 0d;

    /// <summary>
    /// The maximum progress value used for UI clamps.
    /// </summary>
    public const double ProgressMaximum = 1d;

    /// <summary>
    /// The minimum dimension allowed for reduced images.
    /// </summary>
    public const int MinimumImageDimension = 1;

    /// <summary>
    /// The identity scale factor used during image reduction.
    /// </summary>
    public const double IdentityScaleFactor = 1d;
}