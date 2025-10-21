// <copyright file="ImageMetadata.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Imaging;

internal readonly record struct ImageMetadata(
    int Width,
    int Height,
    double Dpi)
{
    public static ImageMetadata Unknown { get; } = new(0, 0, 0d);
}