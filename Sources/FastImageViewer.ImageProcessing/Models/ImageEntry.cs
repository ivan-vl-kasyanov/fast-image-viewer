// <copyright file="ImageEntry.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.ImageProcessing.Models;

/// <summary>
/// Represents a gallery image entry and cached characteristics.
/// </summary>
/// <param name="FullPath">The full path to the file on disk.</param>
/// <param name="FileName">The file name displayed to the user.</param>
/// <param name="LastWriteUtcTicks">The timestamp used to invalidate caches.</param>
/// <param name="LengthBytes">The file size in bytes.</param>
/// <param name="Extension">The normalized file extension.</param>
/// <param name="CacheKey">The cache key derived from the file properties.</param>
/// <param name="IsComplicated">Indicates whether the image needs special processing.</param>
/// <param name="IsDiskCacheEligible">Indicates whether the image qualifies for disk caching.</param>
public sealed record ImageEntry(
    string FullPath,
    string FileName,
    long LastWriteUtcTicks,
    long LengthBytes,
    string Extension,
    string CacheKey,
    bool IsComplicated,
    bool IsDiskCacheEligible);