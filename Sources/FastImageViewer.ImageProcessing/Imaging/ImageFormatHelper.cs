// <copyright file="ImageFormatHelper.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Immutable;

using FastImageViewer.Resources;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Provides helpers for reasoning about image file extensions.
/// </summary>
public static class ImageFormatHelper
{
    private static readonly ImmutableHashSet<string> _complicatedImageExtensions = ImmutableHashSet.CreateRange(
        StringComparer.OrdinalIgnoreCase,
        AppImageExtensionConstants.ComplicatedImageExtensions);

    private static readonly ImmutableHashSet<string> _commonImageExtensions = ImmutableHashSet.CreateRange(
        StringComparer.OrdinalIgnoreCase,
        AppImageExtensionConstants.CommonImageExtensions);

    /// <summary>
    /// Determines whether the specified extension is supported by the viewer.
    /// </summary>
    /// <param name="extension">The extension to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the extension is supported; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSupported(this string extension)
    {
        return
            _commonImageExtensions.Contains(extension) ||
            _complicatedImageExtensions.Contains(extension);
    }

    /// <summary>
    /// Determines whether the specified extension requires additional processing.
    /// </summary>
    /// <param name="extension">The extension to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the extension is considered complicated; otherwise,
    /// <c>false</c>.
    /// </returns>
    public static bool IsComplicated(this string extension)
    {
        return _complicatedImageExtensions.Contains(extension);
    }

    /// <summary>
    /// Determines whether an image qualifies for disk caching based on extension and size.
    /// </summary>
    /// <param name="extension">The extension associated with the image.</param>
    /// <param name="length">The image size in bytes.</param>
    /// <returns>
    /// <c>true</c> if the image should be cached on disk; otherwise,
    /// <c>false</c>.
    /// </returns>
    public static bool IsDiskCacheEligible(
        this string extension,
        long length)
    {
        return
            length > AppNumericConstants.DiskCacheEligibilityThresholdBytes ||
            extension.IsComplicated();
    }
}