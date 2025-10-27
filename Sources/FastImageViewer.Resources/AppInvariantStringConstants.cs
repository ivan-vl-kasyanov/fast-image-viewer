// <copyright file="AppInvariantStringConstants.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Resources;

/// <summary>
/// Provides invariant string values used throughout the application.
/// </summary>
public static class AppInvariantStringConstants
{
    /// <summary>
    /// The identifier applied to the image control in XAML.
    /// </summary>
    public const string ControlDisplayImageName = "DisplayImage";

    /// <summary>
    /// The identifier applied to the close button in XAML.
    /// </summary>
    public const string ControlCloseButtonName = "CloseButton";

    /// <summary>
    /// The identifier applied to the backward button in XAML.
    /// </summary>
    public const string ControlBackwardButtonName = "BackwardButton";

    /// <summary>
    /// The identifier applied to the forward button in XAML.
    /// </summary>
    public const string ControlForwardButtonName = "ForwardButton";

    /// <summary>
    /// The identifier applied to the toggle original button in XAML.
    /// </summary>
    public const string ControlToggleOriginalButtonName = "ToggleOriginalButton";

    /// <summary>
    /// The identifier applied to the loading indicator container in XAML.
    /// </summary>
    public const string ControlLoadingContainerName = "LoadingContainer";

    /// <summary>
    /// The identifier applied to the caching indicator container in XAML.
    /// </summary>
    public const string ControlCachingContainerName = "CachingContainer";

    /// <summary>
    /// The identifier applied to the caching progress bar in XAML.
    /// </summary>
    public const string ControlCachingProgressBarName = "CachingProgressBar";

    /// <summary>
    /// The identifier applied to the error container in XAML.
    /// </summary>
    public const string ControlErrorContainerName = "ErrorContainer";

    /// <summary>
    /// The identifier applied to the error text block in XAML.
    /// </summary>
    public const string ControlErrorTextBlockName = "ErrorTextBlock";

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
    public const string LocalMachineCacheFileName = "LocalMachine.sqlite3";

    /// <summary>
    /// The separator used when building cache keys.
    /// </summary>
    public const char CacheKeySeparator = '|';

    /// <summary>
    /// The file name for the rolling log file.
    /// </summary>
    public const string LogFileName = "FastImageViewer.log";

    /// <summary>
    /// The WebP define that enables lossless encoding.
    /// </summary>
    public const string WebpLosslessDefineName = "lossless";

    /// <summary>
    /// The value indicating that WebP lossless encoding is enabled.
    /// </summary>
    public const string WebpLosslessDefineValue = "true";

    /// <summary>
    /// The WebP define that selects the encoding method.
    /// </summary>
    public const string WebpMethodDefineName = "method";

    /// <summary>
    /// The WebP encoding method optimized for lossless compression.
    /// </summary>
    public const string WebpMethodDefineValue = "6";

    /// <summary>
    /// The WebP define that configures alpha channel quality.
    /// </summary>
    public const string WebpAlphaQualityDefineName = "alpha-quality";

    /// <summary>
    /// The alpha channel quality value that preserves full fidelity.
    /// </summary>
    public const string WebpAlphaQualityDefineValue = "100";

    /// <summary>
    /// The label indicating data came from the original image.
    /// </summary>
    public const string SourceOriginal = "Original";

    /// <summary>
    /// The label indicating data came from cache.
    /// </summary>
    public const string SourceCache = "Cache";
}