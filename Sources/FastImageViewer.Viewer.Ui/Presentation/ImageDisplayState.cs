// <copyright file="ImageDisplayState.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Cache.Models;
using FastImageViewer.ImageProcessing.Models;
using FastImageViewer.Viewer.Ui.Models;

namespace FastImageViewer.Viewer.Ui.Presentation;

/// <summary>
/// Tracks the mutable state associated with image presentation.
/// </summary>
/// <param name="onChanged">The callback invoked when state updates occur.</param>
internal sealed class ImageDisplayState(Action onChanged)
{
    private readonly Action _onChanged = onChanged;

    /// <summary>
    /// Gets the entry currently associated with the state, or <c>null</c> when none is tracked.
    /// </summary>
    public ImageEntry? CurrentEntry { get; private set; }

    /// <summary>
    /// Gets the reduced image data cached for <see cref="CurrentEntry"/>.
    /// </summary>
    public ImageDataResult? CurrentReduced { get; private set; }

    /// <summary>
    /// Gets the original image data cached for <see cref="CurrentEntry"/>.
    /// </summary>
    public ImageDataResult? CurrentOriginal { get; private set; }

    /// <summary>
    /// Gets the last presentation kind rendered to the viewer.
    /// </summary>
    public PresentationKind LastPresented { get; private set; } = PresentationKind.None;

    /// <summary>
    /// Gets a value indicating whether a presentation operation is currently in progress.
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Gets the most recent error message, or <c>null</c> when no error has occurred.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Marks the state as loading and clears any outstanding error.
    /// </summary>
    public void BeginLoading()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        if (ErrorMessage is not null)
        {
            ErrorMessage = null;
        }

        _onChanged();
    }

    /// <summary>
    /// Marks the state as no longer loading.
    /// </summary>
    public void EndLoading()
    {
        if (!IsLoading)
        {
            return;
        }

        IsLoading = false;
        _onChanged();
    }

    /// <summary>
    /// Clears the current error message when one is present.
    /// </summary>
    public void ClearError()
    {
        if (ErrorMessage is null)
        {
            return;
        }

        ErrorMessage = null;
        _onChanged();
    }

    /// <summary>
    /// Records the supplied exception as the latest error message.
    /// </summary>
    /// <param name="exception">The exception encountered during presentation.</param>
    public void SetError(Exception exception)
    {
        IsLoading = false;
        ErrorMessage = $"{exception.Message}\n\n{exception}";
        _onChanged();
    }

    /// <summary>
    /// Clears all entry-specific state.
    /// </summary>
    public void ResetEntry()
    {
        CurrentEntry = null;
        CurrentReduced = null;
        CurrentOriginal = null;
        LastPresented = PresentationKind.None;
        _onChanged();
    }

    /// <summary>
    /// Replaces the tracked entry and resets cached data.
    /// </summary>
    /// <param name="entry">The entry that becomes active.</param>
    public void SetEntry(ImageEntry entry)
    {
        CurrentEntry = entry;
        CurrentReduced = null;
        CurrentOriginal = null;
        LastPresented = PresentationKind.None;
        _onChanged();
    }

    /// <summary>
    /// Updates the cached reduced image data.
    /// </summary>
    /// <param name="reduced">The reduced image data to store.</param>
    public void SetReduced(ImageDataResult? reduced)
    {
        CurrentReduced = reduced;
        _onChanged();
    }

    /// <summary>
    /// Updates the cached original image data.
    /// </summary>
    /// <param name="original">The original image data to store.</param>
    public void SetOriginal(ImageDataResult? original)
    {
        CurrentOriginal = original;
        _onChanged();
    }

    /// <summary>
    /// Records the presentation kind most recently rendered.
    /// </summary>
    /// <param name="presentation">The presentation kind that was rendered.</param>
    public void SetLastPresented(PresentationKind presentation)
    {
        if (LastPresented == presentation)
        {
            return;
        }

        LastPresented = presentation;
        _onChanged();
    }
}