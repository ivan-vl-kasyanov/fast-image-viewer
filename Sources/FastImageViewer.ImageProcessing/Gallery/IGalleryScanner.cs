// <copyright file="IGalleryScanner.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.ImageProcessing.Models;

namespace FastImageViewer.ImageProcessing.Gallery;

/// <summary>
/// Defines helpers for scanning the gallery directory.
/// </summary>
public interface IGalleryScanner
{
    /// <summary>
    /// Scans the gallery directory and builds image entries.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that provides the ordered gallery entries.</returns>
    Task<IReadOnlyList<ImageEntry>> ScanAsync(CancellationToken cancellationToken);
}