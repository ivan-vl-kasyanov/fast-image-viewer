// <copyright file="IApplicationPaths.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Shared.FastImageViewer.Configuration;

/// <summary>
/// Defines accessors for application-specific directories.
/// </summary>
public interface IApplicationPaths
{
    /// <summary>
    /// Gets the path to the gallery directory.
    /// </summary>
    string GalleryDirectory { get; }

    /// <summary>
    /// Gets the path to the cache directory.
    /// </summary>
    string CacheDirectory { get; }
}