// <copyright file="IImageFormatHelper.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.ImageProcessing.Imaging;

/// <summary>
/// Defines helpers for reasoning about image file extensions.
/// </summary>
public interface IImageFormatHelper
{
    /// <summary>
    /// Determines whether the specified extension is supported by the viewer.
    /// </summary>
    /// <param name="extension">The extension to evaluate.</param>
    /// <returns><c>true</c> if the extension is supported; otherwise, <c>false</c>.</returns>
    bool IsSupported(string extension);

    /// <summary>
    /// Determines whether the specified extension requires additional processing.
    /// </summary>
    /// <param name="extension">The extension to evaluate.</param>
    /// <returns><c>true</c> if the extension is considered complicated; otherwise, <c>false</c>.</returns>
    bool IsComplicated(string extension);

    /// <summary>
    /// Determines whether an image qualifies for disk caching based on extension and size.
    /// </summary>
    /// <param name="extension">The extension associated with the image.</param>
    /// <param name="length">The image size in bytes.</param>
    /// <returns><c>true</c> if the image should be cached on disk; otherwise, <c>false</c>.</returns>
    bool IsDiskCacheEligible(
        string extension,
        long length);
}