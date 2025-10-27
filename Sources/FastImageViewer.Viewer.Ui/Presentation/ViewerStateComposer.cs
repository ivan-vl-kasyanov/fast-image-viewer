// <copyright file="ViewerStateComposer.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.ImageProcessing.Gallery;
using FastImageViewer.Resources;
using FastImageViewer.Viewer.Ui.Models;

namespace FastImageViewer.Viewer.Ui.Presentation;

/// <summary>
/// Builds <see cref="ViewerState"/> instances from controller data.
/// </summary>
internal static class ViewerStateComposer
{
    /// <summary>
    /// Composes a <see cref="ViewerState"/> snapshot for the supplied controller data.
    /// </summary>
    /// <param name="navigator">The gallery navigator that provides navigation status.</param>
    /// <param name="display">The display coordinator exposing presentation state.</param>
    /// <param name="isCaching">Indicates whether cache warming is currently in progress.</param>
    /// <param name="cachingProgress">The cache warming progress value.</param>
    /// <returns>The composed viewer state.</returns>
    public static ViewerState Compose(
        GalleryNavigator? navigator,
        ImageDisplayCoordinator display,
        bool isCaching,
        double cachingProgress)
    {
        if (navigator is null)
        {
            return CreateEmptyState(
                display,
                isCaching,
                cachingProgress);
        }

        var (canMoveBackward, canMoveForward) = isCaching
            ? (false, false)
            : (navigator.CanMovePrevious, navigator.CanMoveNext);
        var toggle = DetermineToggle(
            display,
            isCaching);
        var title = display.CurrentEntry?.FileName ?? AppLocalizedStrings.WindowTitleFallback;

        return new ViewerState(
            canMoveBackward,
            canMoveForward,
            toggle.CanToggle,
            toggle.ToggleText,
            title,
            display.IsLoading,
            isCaching,
            cachingProgress,
            display.ErrorMessage);
    }

    /// <summary>
    /// Produces a <see cref="ViewerState"/> when no navigator is currently available.
    /// </summary>
    /// <param name="display">The display coordinator exposing presentation state.</param>
    /// <param name="isCaching">Indicates whether cache warming is currently in progress.</param>
    /// <param name="cachingProgress">The cache warming progress value.</param>
    /// <returns>The composed viewer state.</returns>
    private static ViewerState CreateEmptyState(
        ImageDisplayCoordinator display,
        bool isCaching,
        double cachingProgress)
    {
        var toggle = DetermineToggle(
            display,
            isCaching);

        return new ViewerState(
            false,
            false,
            toggle.CanToggle,
            toggle.ToggleText,
            AppLocalizedStrings.WindowTitleFallback,
            display.IsLoading,
            isCaching,
            cachingProgress,
            display.ErrorMessage);
    }

    private static ToggleInfo DetermineToggle(
        ImageDisplayCoordinator display,
        bool isCaching)
    {
        if (isCaching)
        {
            return new ToggleInfo(
                false,
                AppLocalizedStrings.ToggleShowOriginal);
        }

        var canToggle = display.LastPresented switch
        {
            PresentationKind.Reduced => display.CurrentEntry is not null,
            PresentationKind.Original => display.CurrentReduced is not null,
            _ => false,
        };
        var toggleText = display.LastPresented == PresentationKind.Original
            ? AppLocalizedStrings.ToggleShowReduced
            : AppLocalizedStrings.ToggleShowOriginal;

        return new ToggleInfo(
            canToggle,
            toggleText);
    }

    private readonly record struct ToggleInfo(bool CanToggle, string ToggleText);
}