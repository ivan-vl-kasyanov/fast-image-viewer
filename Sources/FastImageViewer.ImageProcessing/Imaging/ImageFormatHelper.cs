// <copyright file="ImageFormatHelper.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Provides helpers for reasoning about image file extensions.
/// </summary>
public sealed class ImageFormatHelper : IImageFormatHelper
{
    /// <inheritdoc/>
    public bool IsSupported(string extension)
    {
        return
            AppImageExtensionConstants.CommonImageExtensions.Contains(extension) ||
            AppImageExtensionConstants.ComplicatedImageExtensions.Contains(extension);
    }

    /// <inheritdoc/>
    public bool IsComplicated(string extension)
    {
        return AppImageExtensionConstants.ComplicatedImageExtensions.Contains(extension);
    }

    /// <inheritdoc/>
    public bool IsDiskCacheEligible(
        string extension,
        long length)
    {
        return
            (length > AppNumericConstants.DiskCacheEligibilityThresholdBytes) ||
            IsComplicated(extension);
    }
}