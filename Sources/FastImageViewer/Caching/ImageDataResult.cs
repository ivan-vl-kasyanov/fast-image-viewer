// <copyright file="ImageDataResult.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Imaging;

namespace FastImageViewer.Caching;

internal sealed record ImageDataResult(
    byte[] Bytes,
    ImageMetadata Metadata,
    string Source,
    bool IsReduced);