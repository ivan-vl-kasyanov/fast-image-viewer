// <copyright file="ViewerState.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Presentation;

internal sealed record ViewerState(
    bool CanMoveBackward,
    bool CanMoveForward,
    bool CanToggleOriginal,
    string ToggleButtonContent,
    string WindowTitle,
    bool IsLoading,
    string? ErrorMessage);