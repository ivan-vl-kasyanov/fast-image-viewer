// <copyright file="ImageFormatHelper.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Immutable;

using FastImageViewer.Configuration;

namespace FastImageViewer.Imaging;

internal static class ImageFormatHelper
{
    private static readonly ImmutableHashSet<string> _complicatedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".heic",
        ".heif",
        ".tif",
        ".tiff",
        ".3fr",
        ".arw",
        ".cr2",
        ".cr3",
        ".crw",
        ".dcr",
        ".dng",
        ".erf",
        ".kdc",
        ".mef",
        ".mos",
        ".mrw",
        ".nef",
        ".nrw",
        ".orf",
        ".pef",
        ".raf",
        ".raw",
        ".rw2",
        ".sr2",
        ".srf",
        ".srw",
    }.ToImmutableHashSet();

    private static readonly ImmutableHashSet<string> _commonImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif",
        ".webp",
        ".avif",
    }.ToImmutableHashSet();

    internal static bool IsSupported(this string extension) // TODO: `this` added – use it.
    {
        return
            _commonImageExtensions.Contains(extension) ||
            _complicatedImageExtensions.Contains(extension);
    }

    internal static bool IsComplicated(this string extension) // TODO: `this` added – use it.
    {
        return _complicatedImageExtensions.Contains(extension);
    }

    internal static bool IsDiskCacheEligible(
        this string extension, // TODO: `this` added – use it.
        long length)
    {
        return
            (length > AppConstants.DiskCacheEligibilityThresholdBytes) ||
            IsComplicated(extension);
    }
}