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
/// <param name="applicationPaths">The application paths used to locate the gallery.</param>
/// <param name="imageFormatHelper">The helper that evaluates image formats.</param>
public sealed class GalleryScanner(
    IApplicationPaths applicationPaths,
    IImageFormatHelper imageFormatHelper) : IGalleryScanner
{
    private readonly IApplicationPaths _applicationPaths = applicationPaths;
    private readonly IImageFormatHelper _imageFormatHelper = imageFormatHelper;

    /// <inheritdoc/>
    public Task<IReadOnlyList<ImageEntry>> ScanAsync(CancellationToken cancellationToken)
    {
        return Task.Run(
            BuildEntries,
            cancellationToken);
    }

    private IReadOnlyList<ImageEntry> BuildEntries()
    {
        var root = _applicationPaths.GalleryDirectory;
        var directory = new DirectoryInfo(root);

        return directory.Exists
            ? directory
                .EnumerateFiles()
                .Where(file => _imageFormatHelper.IsSupported(file.Extension))
                .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
                .Select(CreateEntry)
                .ToImmutableArray()
            : [];
    }

    private ImageEntry CreateEntry(FileInfo file)
    {
        var cacheKey = string.Concat(
            file.FullName,
            AppInvariantStringConstants.CacheKeySeparator,
            file.LastWriteTimeUtc.Ticks,
            AppInvariantStringConstants.CacheKeySeparator,
            file.Length);
        var extension = file.Extension.ToLowerInvariant();
        var isComplicated = _imageFormatHelper.IsComplicated(extension);
        var isEligible = _imageFormatHelper.IsDiskCacheEligible(
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