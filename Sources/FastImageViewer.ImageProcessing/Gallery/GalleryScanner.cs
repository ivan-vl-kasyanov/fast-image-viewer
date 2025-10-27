// <copyright file="GalleryScanner.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Immutable;

using FastImageViewer.ImageProcessing.Imaging;
using FastImageViewer.ImageProcessing.Models;
using FastImageViewer.Resources;
using FastImageViewer.Shared.FastImageViewer.Configuration;

namespace FastImageViewer.ImageProcessing.Gallery;

/// <summary>
/// Scans the gallery directory for available images.
/// </summary>
public static class GalleryScanner
{
    /// <summary>
    /// Scans the gallery directory and builds image entries.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that provides the ordered gallery entries.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public static Task<IReadOnlyList<ImageEntry>> ScanAsync(CancellationToken cancellationToken)
    {
        return Task.Run(
            BuildEntries,
            cancellationToken);
    }

    private static IReadOnlyList<ImageEntry> BuildEntries()
    {
        var root = AppPaths.GalleryDirectory;
        var directory = new DirectoryInfo(root);

        return directory.Exists
            ? directory
                .EnumerateFiles()
                .Where(file => ImageFormatHelper.IsSupported(file.Extension))
                .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
                .Select(CreateEntry)
                .ToImmutableArray()
            : [];
    }

    private static ImageEntry CreateEntry(FileInfo file)
    {
        var cacheKey = string.Concat(
            file.FullName,
            AppInvariantStringConstants.CacheKeySeparator,
            file.LastWriteTimeUtc.Ticks,
            AppInvariantStringConstants.CacheKeySeparator,
            file.Length);
        var extension = file.Extension.ToLowerInvariant();
        var isComplicated = ImageFormatHelper.IsComplicated(extension);
        var isEligible = ImageFormatHelper.IsDiskCacheEligible(
            extension,
            file.Length);

        return new ImageEntry(
            file.FullName,
            file.Name,
            file.LastWriteTimeUtc.Ticks,
            file.Length,
            extension,
            cacheKey,
            isComplicated,
            isEligible);
    }
}