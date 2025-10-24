// <copyright file="MainController.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Reactive.Threading.Tasks;

using Akavache;

using Avalonia.Controls;
using Avalonia.Threading;

using FastImageViewer.Caching;
using FastImageViewer.Configuration;
using FastImageViewer.Gallery;
using FastImageViewer.Imaging;
using FastImageViewer.Text;
using FastImageViewer.Ui;

namespace FastImageViewer.Presentation;

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

    public event Action? CloseRequested;

    public event Action<ViewerState>? StateChanged;

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

    public void RequestApplicationExit()
    {
        CloseRequested?.Invoke();
    }

    public void ClearError(CancellationToken cancellationToken)
    {
        if (_errorMessage is null)
        {
            return;
        }

        _errorMessage = null;
        UpdateState(cancellationToken);
    }

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

        BeginLoading(cancellationToken);

        try
        {
            if (_lastPresented != PresentationKind.Original)
            {
                _currentOriginal ??= await _cachePipeline.GetOriginalAsync(
                    _currentEntry,
                    cancellationToken);

                _lastPresented = PresentationKind.Original;
                await PresentAsync(
                    _currentOriginal,
                    cancellationToken);

                UpdateState(cancellationToken);

                return;
            }

            if (_currentReduced is null)
            {
                _currentReduced = await _cachePipeline.GetReducedAsync(
                    _currentEntry,
                    _screenMetrics,
                    cancellationToken);
                if (_currentReduced is null)
                {
                    return;
                }
            }

            _lastPresented = PresentationKind.Reduced;
            await PresentAsync(
                _currentReduced,
                cancellationToken);
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

        CancelCurrent();
        BeginLoading(cancellationToken);

        try
        {
            if (entry is null)
            {
                await _presenter
                    .ClearAsync(cancellationToken)
                    .ToObservable()
                    .ToTask(cancellationToken);

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
                    cancellationToken);
                if (reduced is not null)
                {
                    _currentReduced = reduced;
                    _lastPresented = PresentationKind.Reduced;
                    await PresentAsync(
                        reduced,
                        cancellationToken);

                    UpdateState(cancellationToken);

                    return;
                }
            }

            var original = await _cachePipeline.GetOriginalAsync(
                entry,
                cancellationToken);
            _currentOriginal = original;
            _lastPresented = PresentationKind.Original;
            await PresentAsync(
                original,
                cancellationToken);

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
                NonAllocationStrings.ToggleShowOriginal,
                NonAllocationStrings.WindowTitleFallback,
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
            ? NonAllocationStrings.ToggleShowReduced
            : NonAllocationStrings.ToggleShowOriginal;
        var title = _currentEntry?.FileName ?? NonAllocationStrings.WindowTitleFallback;
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
        Console
            .Error
            .WriteLine($"{exception.Message}\n{exception}");
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