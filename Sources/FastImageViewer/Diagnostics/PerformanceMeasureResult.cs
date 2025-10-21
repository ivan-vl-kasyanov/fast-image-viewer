// <copyright file="PerformanceMeasureResult.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Imaging;

namespace FastImageViewer.Diagnostics;

internal sealed record PerformanceMeasureResult<T>(
    T Value,
    ImageMetadata Metadata);