// <copyright file="PresentationKind.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Viewer.Ui.Presentation;

/// <summary>
/// Identifies which version of the image is currently shown.
/// </summary>
internal enum PresentationKind
{
    /// <summary>
    /// Indicates that nothing is currently presented.
    /// </summary>
    None,

    /// <summary>
    /// Indicates that the original image is shown.
    /// </summary>
    Original,

    /// <summary>
    /// Indicates that the reduced image is shown.
    /// </summary>
    Reduced,
}