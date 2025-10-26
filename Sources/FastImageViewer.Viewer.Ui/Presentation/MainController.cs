// <copyright file="MainController.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Reactive.Threading.Tasks;

using Akavache;

using Avalonia.Controls;
using Avalonia.Threading;

using FastImageViewer.Cache;
using FastImageViewer.ImageProcessing.Gallery;
using FastImageViewer.ImageProcessing.Imaging;
using FastImageViewer.Resources;
using FastImageViewer.Shared.FastImageViewer.Configuration;

namespace FastImageViewer.Viewer.Ui.Presentation;

/// <summary>
/// Coordinates gallery navigation, caching, and presentation logic.
/// </summary>
internal sealed class MainController : IDisposable
{
    private readonly WarmthMode _mode;
    private readonly ImagePresenter _presenter;
    private readonly ImageCachePipeline _cachePipeline;
    private GalleryNavigator? _navigator;
    private ScreenMetrics _screenMetrics;
    private CancellationTokenSource? _displayCancellationTokenSource;
    private ImageEntry? _currentEntry;
    private ImageDataResult? _currentReduced;
    private ImageDataResult? _currentOriginal;
    private PresentationKind _lastPresented = PresentationKind.None;
    private bool _initialized;
    private bool _isLoading;
    private bool _isCaching;
    private double _cachingProgress;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainController"/> class.
    /// </summary>
    /// <param name="mode">The current warm-up mode.</param>
    /// <param name="presenter">The presenter responsible for showing images.</param>
    public MainController(
        WarmthMode mode,
        ImagePresenter presenter)
    {
        _mode = mode;
        _presenter = presenter;

        var distributed = new AkavacheDistributedCacheAdapter(CacheDatabase.LocalMachine);
        var fusionCache = FusionCacheFactory.Create(distributed);
        _cachePipeline = new ImageCachePipeline(
            fusionCache,
            mode);
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

        cancellationToken.ThrowIfCancellationRequested();

        _screenMetrics = await Dispatcher
            .UIThread
            .InvokeAsync(
                () => ScreenMetricsProvider.GetPrimaryMetrics(topLevel),
                DispatcherPriority.MaxValue,
                cancellationToken);
        var entries = await GalleryScanner.ScanAsync(cancellationToken);
        _navigator = new GalleryNavigator(entries);
        _initialized = true;

        UpdateState(cancellationToken);

        if (entries.Count == 0)
        {
            return;
        }

        if (_mode == WarmthMode.Hot)
        {
            _isCaching = true;
            _cachingProgress = 0;
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
                _cachingProgress = 0;
                UpdateState(cancellationToken);
            }
        }

        await ShowEntryAsync(
            _navigator.Current,
            _mode != WarmthMode.Cold,
            cancellationToken);
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
        if (_errorMessage is null)
        {
            return;
        }

        _errorMessage = null;
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

        cancellationToken.ThrowIfCancellationRequested();

        var next = _navigator.MoveNext();

        await ShowEntryAsync(
            next,
            _mode != WarmthMode.Cold,
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

        cancellationToken.ThrowIfCancellationRequested();

        var previous = _navigator.MovePrevious();

        await ShowEntryAsync(
            previous,
            _mode != WarmthMode.Cold,
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
        if (_mode == WarmthMode.Cold)
        {
            return;
        }

        if (_currentEntry is null)
        {
            return;
        }

        var displayToken = StartDisplayOperation(cancellationToken);
        BeginLoading(cancellationToken);

        try
        {
            if (_lastPresented != PresentationKind.Original)
            {
                _currentOriginal ??= await _cachePipeline.GetOriginalAsync(
                    _currentEntry,
                    displayToken);

                _lastPresented = PresentationKind.Original;
                await PresentAsync(
                    _currentOriginal,
                    displayToken);

                UpdateState(cancellationToken);

                return;
            }

            if (_currentReduced is null)
            {
                _currentReduced = await _cachePipeline.GetReducedAsync(
                    _currentEntry,
                    _screenMetrics,
                    displayToken);
                if (_currentReduced is null)
                {
                    return;
                }
            }

            _lastPresented = PresentationKind.Reduced;
            await PresentAsync(
                _currentReduced,
                displayToken);
            UpdateState(cancellationToken);
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
            EndLoading(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        CancelCurrent();
        _presenter.Dispose();
    }

    private async Task ShowEntryAsync(
        ImageEntry? entry,
        bool preferReduced,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var displayToken = StartDisplayOperation(cancellationToken);
        BeginLoading(cancellationToken);

        try
        {
            if (entry is null)
            {
                await _presenter
                    .ClearAsync(displayToken)
                    .ToObservable()
                    .ToTask(displayToken);

                _currentEntry = null;
                _currentReduced = null;
                _currentOriginal = null;
                _lastPresented = PresentationKind.None;
                UpdateState(cancellationToken);

                return;
            }

            _currentEntry = entry;
            _currentReduced = null;
            _currentOriginal = null;
            if (preferReduced)
            {
                var reduced = await _cachePipeline.GetReducedAsync(
                    entry,
                    _screenMetrics,
                    displayToken);
                if (reduced is not null)
                {
                    _currentReduced = reduced;
                    _lastPresented = PresentationKind.Reduced;
                    await PresentAsync(
                        reduced,
                        displayToken);

                    UpdateState(cancellationToken);

                    return;
                }
            }

            var original = await _cachePipeline.GetOriginalAsync(
                entry,
                displayToken);
            _currentOriginal = original;
            _lastPresented = PresentationKind.Original;
            await PresentAsync(
                original,
                displayToken);

            UpdateState(cancellationToken);
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
            EndLoading(cancellationToken);
        }
    }

    private async Task PresentAsync(
        ImageDataResult data,
        CancellationToken cancellationToken)
    {
        await _presenter.ShowAsync(
            data.Bytes,
            cancellationToken);
    }

    private CancellationToken StartDisplayOperation(CancellationToken cancellationToken)
    {
        CancelCurrent();

        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _displayCancellationTokenSource = source;

        return source.Token;
    }

    private void CancelCurrent()
    {
        if (_displayCancellationTokenSource is null)
        {
            return;
        }

        _displayCancellationTokenSource.Cancel();
        _displayCancellationTokenSource.Dispose();
        _displayCancellationTokenSource = null;
    }

    private void UpdateState(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_navigator is null)
        {
            PublishState(new ViewerState(
                false,
                false,
                false,
                AppConstants.ToggleShowOriginal,
                AppConstants.WindowTitleFallback,
                _isLoading,
                _isCaching,
                _cachingProgress,
                _errorMessage));

            return;
        }

        var canToggle = (_mode != WarmthMode.Cold) &&
            _lastPresented switch
            {
                PresentationKind.Reduced => _currentEntry is not null,
                PresentationKind.Original => _currentReduced is not null,
                _ => false,
            };
        var toggleText = _lastPresented == PresentationKind.Original
            ? AppConstants.ToggleShowReduced
            : AppConstants.ToggleShowOriginal;
        var title = _currentEntry?.FileName ?? AppConstants.WindowTitleFallback;
        var canMoveBackward = _navigator.CanMovePrevious;
        var canMoveForward = _navigator.CanMoveNext;
        if (_isCaching)
        {
            canToggle = false;
            canMoveBackward = false;
            canMoveForward = false;
        }

        cancellationToken.ThrowIfCancellationRequested();

        PublishState(new ViewerState(
            canMoveBackward,
            canMoveForward,
            canToggle,
            toggleText,
            title,
            _isLoading,
            _isCaching,
            _cachingProgress,
            _errorMessage));
    }

    private void BeginLoading(CancellationToken cancellationToken)
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        if (_errorMessage is not null)
        {
            _errorMessage = null;
        }

        UpdateState(cancellationToken);
    }

    private void EndLoading(CancellationToken cancellationToken)
    {
        if (!_isLoading)
        {
            return;
        }

        _isLoading = false;
        UpdateState(cancellationToken);
    }

    private void HandleError(Exception exception)
    {
        Serilog.Log.Error(
            exception,
            "Unexpected controller error during presentation workflow.");
        _isLoading = false;
        _errorMessage = $"{exception.Message}\n\n{exception}";
        UpdateState(default);
    }

    private void PublishState(ViewerState state)
    {
        Dispatcher
            .UIThread
            .Post(
                () => StateChanged?.Invoke(state),
                DispatcherPriority.Normal);
    }
}