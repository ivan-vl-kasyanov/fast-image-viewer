// <copyright file="MainController.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Akavache;

using Avalonia.Controls;
using Avalonia.Threading;

using FastImageViewer.Cache;
using FastImageViewer.ImageProcessing.Gallery;
using FastImageViewer.ImageProcessing.Imaging;
using FastImageViewer.Resources;

namespace FastImageViewer.Viewer.Ui.Presentation;

/// <summary>
/// Coordinates gallery navigation, caching, and presentation logic.
/// </summary>
internal sealed class MainController : IDisposable
{
    private readonly ImageCachePipeline _cachePipeline;
    private readonly ImageDisplayCoordinator _displayCoordinator;
    private GalleryNavigator? _navigator;
    private ScreenMetrics _screenMetrics;
    private bool _initialized;
    private bool _isCaching;
    private double _cachingProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainController"/> class.
    /// </summary>
    /// <param name="presenter">The presenter responsible for showing images.</param>
    public MainController(
        ImagePresenter presenter)
    {
        var distributed = new AkavacheDistributedCacheAdapter(CacheDatabase.LocalMachine);
        var fusionCache = FusionCacheFactory.Create(distributed);
        _cachePipeline = new ImageCachePipeline(
            fusionCache);
        _displayCoordinator = new ImageDisplayCoordinator(
            presenter,
            _cachePipeline,
            NotifyDisplayStateChanged);
    }

    /// <summary>
    /// Occurs when the controller requests application shutdown.
    /// </summary>
    public event Action? CloseRequested;

    /// <summary>
    /// Occurs when the viewer state has changed.
    /// </summary>
    public event Action<ViewerState>? StateChanged;

    /// <summary>
    /// Initializes the controller and loads the initial gallery state.
    /// </summary>
    /// <param name="topLevel">The owning window used to query screen metrics.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when initialization finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task InitializeAsync(
        TopLevel topLevel,
        CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await InitializeCoreAsync(
                topLevel,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Requests that the hosting application close.
    /// </summary>
    public void RequestApplicationExit()
    {
        CloseRequested?.Invoke();
    }

    /// <summary>
    /// Clears the current error message if one is present.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    public void ClearError(CancellationToken cancellationToken)
    {
        _displayCoordinator.ClearError();
        UpdateState(cancellationToken);
    }

    /// <summary>
    /// Advances to the next gallery entry when available.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when navigation finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task MoveForwardAsync(CancellationToken cancellationToken)
    {
        if (_navigator is null)
        {
            return;
        }

        var next = _navigator.MoveNext();
        await ShowEntryAsync(
            next,
            true,
            cancellationToken);
    }

    /// <summary>
    /// Moves to the previous gallery entry when available.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when navigation finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task MoveBackwardAsync(CancellationToken cancellationToken)
    {
        if (_navigator is null)
        {
            return;
        }

        var previous = _navigator.MovePrevious();
        await ShowEntryAsync(
            previous,
            true,
            cancellationToken);
    }

    /// <summary>
    /// Toggles between reduced and original image presentations.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when toggling finishes.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task ToggleOriginalAsync(CancellationToken cancellationToken)
    {
        if (_displayCoordinator.CurrentEntry is null)
        {
            return;
        }

        await _displayCoordinator.ToggleOriginalAsync(
            _screenMetrics,
            cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _displayCoordinator.DisposeResources();
    }

    private static async Task<ScreenMetrics> FetchScreenMetricsAsync(
        TopLevel topLevel,
        CancellationToken cancellationToken)
    {
        return await Dispatcher
            .UIThread
            .InvokeAsync(
                () => ScreenMetricsProvider.GetPrimaryMetrics(topLevel),
                DispatcherPriority.MaxValue,
                cancellationToken);
    }

    private async Task ShowEntryAsync(
        ImageEntry? entry,
        bool preferReduced,
        CancellationToken cancellationToken)
    {
        await _displayCoordinator.ShowEntryAsync(
            entry,
            _screenMetrics,
            preferReduced,
            cancellationToken);
    }

    private void NotifyDisplayStateChanged()
    {
        UpdateState(CancellationToken.None);
    }

    private void UpdateState(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var state = ViewerStateComposer.Compose(
            _navigator,
            _displayCoordinator,
            _isCaching,
            _cachingProgress);

        cancellationToken.ThrowIfCancellationRequested();

        PublishState(state);
    }

    private async Task InitializeCoreAsync(
        TopLevel topLevel,
        CancellationToken cancellationToken)
    {
        _screenMetrics = await FetchScreenMetricsAsync(
            topLevel,
            cancellationToken);
        var entries = await GalleryScanner.ScanAsync(cancellationToken);

        _navigator = new GalleryNavigator(entries);
        _initialized = true;

        UpdateState(cancellationToken);

        if (entries.Count == 0)
        {
            return;
        }

        await WarmCacheAsync(
            entries,
            cancellationToken);

        await ShowEntryAsync(
            _navigator.Current,
            true,
            cancellationToken);
    }

    private void PublishState(ViewerState state)
    {
        Dispatcher
            .UIThread
            .Post(
                () => StateChanged?.Invoke(state),
                DispatcherPriority.Normal);
    }

    private async Task WarmCacheAsync(
        IReadOnlyList<ImageEntry> entries,
        CancellationToken cancellationToken)
    {
        _isCaching = true;
        _cachingProgress = AppNumericConstants.ProgressMinimum;
        UpdateState(cancellationToken);

        try
        {
            var progress = new Progress<double>(value =>
            {
                _cachingProgress = value;
                UpdateState(cancellationToken);
            });

            await _cachePipeline.WarmAllAsync(
                entries,
                _screenMetrics,
                progress,
                cancellationToken);
        }
        finally
        {
            _isCaching = false;
            _cachingProgress = AppNumericConstants.ProgressMinimum;
            UpdateState(cancellationToken);
        }
    }
}