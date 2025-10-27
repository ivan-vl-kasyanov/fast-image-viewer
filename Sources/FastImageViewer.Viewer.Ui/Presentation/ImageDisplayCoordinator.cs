// <copyright file="ImageDisplayCoordinator.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Cache;
using FastImageViewer.ImageProcessing.Imaging;

using Serilog;

namespace FastImageViewer.Viewer.Ui.Presentation;

/// <summary>
/// Coordinates image presentation and cache lookups for the viewer.
/// </summary>
/// <param name="presenter">The presenter responsible for rendering images.</param>
/// <param name="cachePipeline">The cache pipeline used to retrieve image data.</param>
/// <param name="onStateChanged">The callback invoked when presentation state changes.</param>
internal sealed class ImageDisplayCoordinator(
    ImagePresenter presenter,
    ImageCachePipeline cachePipeline,
    Action onStateChanged) : IDisposable
{
    private readonly ImagePresenter _presenter = presenter;
    private readonly ImageCachePipeline _cachePipeline = cachePipeline;
    private readonly ImageDisplayState _state = new(onStateChanged);
    private readonly DisplayOperationManager _operations = new();

    /// <summary>
    /// Gets the entry currently managed by the coordinator.
    /// </summary>
    public ImageEntry? CurrentEntry => _state.CurrentEntry;

    /// <summary>
    /// Gets the reduced image data cached for the current entry.
    /// </summary>
    public ImageDataResult? CurrentReduced => _state.CurrentReduced;

    /// <summary>
    /// Gets the original image data cached for the current entry.
    /// </summary>
    public ImageDataResult? CurrentOriginal => _state.CurrentOriginal;

    /// <summary>
    /// Gets the last presentation kind rendered to the viewer.
    /// </summary>
    public PresentationKind LastPresented => _state.LastPresented;

    /// <summary>
    /// Gets a value indicating whether a presentation operation is currently running.
    /// </summary>
    public bool IsLoading => _state.IsLoading;

    /// <summary>
    /// Gets the latest error message, or <c>null</c> when no error occurred.
    /// </summary>
    public string? ErrorMessage => _state.ErrorMessage;

    /// <inheritdoc/>
    public void Dispose()
    {
        DisposeResources();
    }

    /// <summary>
    /// Releases resources owned by the coordinator.
    /// </summary>
    public void DisposeResources()
    {
        _operations.Cancel();
        _presenter.Dispose();
    }

    /// <summary>
    /// Clears the current error message when one is present.
    /// </summary>
    public void ClearError()
    {
        _state.ClearError();
    }

    /// <summary>
    /// Presents the supplied entry using cached data or by loading images on demand.
    /// </summary>
    /// <param name="entry">The entry to present.</param>
    /// <param name="metrics">The screen metrics that guide reduced image generation.</param>
    /// <param name="preferReduced">Indicates whether a reduced image should be attempted first.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when the presentation request finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public Task ShowEntryAsync(
        ImageEntry? entry,
        ScreenMetrics metrics,
        bool preferReduced,
        CancellationToken cancellationToken)
    {
        return RunDisplayOperationAsync(
            async token =>
            {
                await PresentEntryAsync(
                        entry,
                        metrics,
                        preferReduced,
                        token)
                    .ConfigureAwait(false);
            },
            cancellationToken);
    }

    /// <summary>
    /// Switches between the original and reduced versions of the current entry.
    /// </summary>
    /// <param name="metrics">The screen metrics used when generating reduced images.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when toggling finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task ToggleOriginalAsync(
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        if (CurrentEntry is null)
        {
            return;
        }

        await RunDisplayOperationAsync(
                async token =>
                {
                    await TogglePresentationAsync(
                            metrics,
                            token)
                        .ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Cancels any ongoing presentation operation.
    /// </summary>
    public void CancelCurrent()
    {
        _operations.Cancel();
    }

    private async Task RunDisplayOperationAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var displayToken = _operations.Start(cancellationToken);
        _state.BeginLoading();

        try
        {
            await operation(displayToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            _state.EndLoading();
        }
    }

    private async Task PresentEntryAsync(
        ImageEntry? entry,
        ScreenMetrics metrics,
        bool preferReduced,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (entry is null)
        {
            await ClearPresentationAsync(cancellationToken);
            _state.ResetEntry();

            return;
        }

        _state.SetEntry(entry);

        if (preferReduced &&
            await TryShowReducedAsync(
                entry,
                metrics,
                cancellationToken))
        {
            return;
        }

        await ShowOriginalAsync(
            entry,
            cancellationToken);
    }

    private async Task TogglePresentationAsync(
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (LastPresented != PresentationKind.Original)
        {
            var original = await EnsureOriginalAsync(cancellationToken);
            await PresentAsync(
                original,
                cancellationToken);

            return;
        }

        var reduced = await EnsureReducedAsync(
            metrics,
            cancellationToken);
        if (reduced is null)
        {
            return;
        }

        await PresentAsync(
            reduced,
            cancellationToken);
    }

    private async Task ClearPresentationAsync(CancellationToken cancellationToken)
    {
        await _presenter.ClearAsync(cancellationToken);
    }

    private async Task<bool> TryShowReducedAsync(
        ImageEntry entry,
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        var reduced = await _cachePipeline.GetReducedAsync(
            entry,
            metrics,
            cancellationToken);
        if (reduced is null)
        {
            return false;
        }

        _state.SetReduced(reduced);
        _state.SetLastPresented(PresentationKind.Reduced);
        await PresentAsync(
            reduced,
            cancellationToken);

        return true;
    }

    private async Task ShowOriginalAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        var original = await _cachePipeline.GetOriginalAsync(
            entry,
            cancellationToken);

        _state.SetOriginal(original);
        _state.SetLastPresented(PresentationKind.Original);
        await PresentAsync(
            original,
            cancellationToken);
    }

    private async Task<ImageDataResult> EnsureOriginalAsync(CancellationToken cancellationToken)
    {
        var current = _state.CurrentOriginal;
        if (current is null)
        {
            if (CurrentEntry is null)
            {
                throw new InvalidOperationException("Current entry must be set before toggling.");
            }

            current = await _cachePipeline.GetOriginalAsync(
                CurrentEntry,
                cancellationToken);
            _state.SetOriginal(current);
        }

        _state.SetLastPresented(PresentationKind.Original);

        return current;
    }

    private async Task<ImageDataResult?> EnsureReducedAsync(
        ScreenMetrics metrics,
        CancellationToken cancellationToken)
    {
        var reduced = _state.CurrentReduced;
        if (reduced is null)
        {
            if (CurrentEntry is null)
            {
                return null;
            }

            reduced = await _cachePipeline.GetReducedAsync(
                CurrentEntry,
                metrics,
                cancellationToken);
            if (reduced is null)
            {
                return null;
            }

            _state.SetReduced(reduced);
        }

        _state.SetLastPresented(PresentationKind.Reduced);

        return reduced;
    }

    private async Task PresentAsync(
        ImageDataResult data,
        CancellationToken cancellationToken)
    {
        await _presenter.ShowAsync(
            data.Bytes,
            cancellationToken);
    }

    private void HandleError(Exception exception)
    {
        Log.Error(
            exception,
            "Unexpected controller error during presentation workflow.");
        _state.SetError(exception);
    }
}