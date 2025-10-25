// <copyright file="ImageData.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Imaging;

/// <summary>
/// Holds image bytes paired with the associated metadata.
/// </summary>
/// <param name="Bytes">The encoded image data.</param>
/// <param name="Metadata">The metadata describing the image dimensions.</param>
internal sealed record ImageData(
    byte[] Bytes,
    ImageMetadata Metadata);