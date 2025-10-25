// <copyright file="ViewerState.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Presentation;

/// <summary>
/// Represents the UI state of the viewer window.
/// </summary>
/// <param name="CanMoveBackward">Indicates whether backward navigation is allowed.</param>
/// <param name="CanMoveForward">Indicates whether forward navigation is allowed.</param>
/// <param name="CanToggleOriginal">Indicates whether toggling to the original image is allowed.</param>
/// <param name="ToggleButtonContent">The content displayed on the toggle button.</param>
/// <param name="WindowTitle">The window title to display.</param>
/// <param name="IsLoading">Indicates whether the viewer is loading content.</param>
/// <param name="IsCaching">Indicates whether cache warming is in progress.</param>
/// <param name="CachingProgress">The cache warming progress fraction.</param>
/// <param name="ErrorMessage">The current error message or <c>null</c>.</param>
internal sealed record ViewerState(
    bool CanMoveBackward,
    bool CanMoveForward,
    bool CanToggleOriginal,
    string ToggleButtonContent,
    string WindowTitle,
    bool IsLoading,
    bool IsCaching,
    double CachingProgress,
    string? ErrorMessage);