// <copyright file="ImageDataResult.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Imaging;

namespace FastImageViewer.Caching;

/// <summary>
/// Represents cached image data together with its origin details.
/// </summary>
/// <param name="Bytes">The image bytes retrieved from cache or disk.</param>
/// <param name="Metadata">The metadata describing the image.</param>
/// <param name="Source">The description of where the data originated.</param>
/// <param name="IsReduced">Indicates whether the data represents a reduced image.</param>
internal sealed record ImageDataResult(
    byte[] Bytes,
    ImageMetadata Metadata,
    string Source,
    bool IsReduced);