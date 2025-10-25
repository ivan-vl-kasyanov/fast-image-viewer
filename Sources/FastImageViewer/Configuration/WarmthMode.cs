// <copyright file="WarmthMode.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Configuration;

/// <summary>
/// Describes how aggressively the application should prepare cached data.
/// </summary>
internal enum WarmthMode
{
    /// <summary>
    /// Loads images on demand without pre-warming caches.
    /// </summary>
    Cold,

    /// <summary>
    /// Pre-warms caches to improve startup responsiveness.
    /// </summary>
    Hot,

    /// <summary>
    /// Removes existing cache data before exiting.
    /// </summary>
    Clean,
}