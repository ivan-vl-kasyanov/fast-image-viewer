// <copyright file="ImageData.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Imaging;

internal sealed record ImageData(
    byte[] Bytes,
    ImageMetadata Metadata);