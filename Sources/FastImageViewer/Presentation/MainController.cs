// <copyright file="MainController.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Diagnostics;
using System.Reactive.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;

using FastImageViewer.Caching;
using FastImageViewer.Configuration;
using FastImageViewer.Diagnostics;
using FastImageViewer.Gallery;
using FastImageViewer.Imaging;
using FastImageViewer.Text;
using FastImageViewer.Threading;
using FastImageViewer.Ui;

namespace FastImageViewer.Presentation;

internal sealed class MainController : IDisposable
{
    private readonly WarmthMode _mode;
    private readonly PerformanceLogger _logger;
    private readonly ImagePresenter _presenter;
    private readonly ScreenMetricsProvider _screenMetricsProvider = new();
    private readonly GalleryScanner _galleryScanner = new();
    private readonly ICachePipeline _cachePipeline;
    private IReadOnlyList<ImageEntry> _entries = Array.Empty<ImageEntry>();
    private GalleryNavigator? _navigator;
    private ScreenMetrics _screenMetrics;
    private CancellationTokenSource? _displayCancellationTokenSource;
    private ImageEntry? _currentEntry;
    private ImageDataResult? _currentReduced;
    private ImageDataResult? _currentOriginal;
    private PresentationKind _lastPresented = PresentationKind.None;
    private bool _initialized;

    public MainController(
        WarmthMode mode,
        PerformanceLogger logger,
        ImagePresenter presenter)
    {
        _mode = mode;
        _logger = logger;
        _presenter = presenter;

        var distributed = new AkavacheDistributedCacheAdapter(BlobCache.LocalMachine); // FIXME: Invalid argument type. Note that "akavache.core" library is deprecated, use "Akavache".
        var fusionCache = FusionCacheFactory.Create(distributed);
        var reducer = new ImageReducer();
        var originalLoader = new OriginalImageLoader();
        _cachePipeline = new ImageCachePipeline(
            fusionCache,
            distributed,
            reducer,
            originalLoader,
            logger,
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

        _screenMetrics = await Dispatcher.UIThread.InvokeAsync(
            () => _screenMetricsProvider.GetPrimaryMetrics(topLevel));
        _entries = await _galleryScanner
            .ScanAsync()
            .ToObservable()
            .ToTask(cancellationToken);
        _navigator = new GalleryNavigator(_entries);
        _initialized = true;
        UpdateState();
        if (_entries.Count == 0)
        {
            return;
        }

        if (_mode == WarmthMode.Hot)
        {
            await _cachePipeline.WarmAllAsync(
                _entries,
                _screenMetrics,
                cancellationToken);
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

    public async Task MoveForwardAsync(CancellationToken cancellationToken)
    {
        if (_navigator is null)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var next = _navigator.MoveNext();
        cancellationToken.ThrowIfCancellationRequested();

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
        cancellationToken.ThrowIfCancellationRequested();

        await ShowEntryAsync(
            previous,
            _mode != WarmthMode.Cold,
            cancellationToken);
    }

    public async Task ToggleOriginalAsync(CancellationToken cancellationToken)
    {
        if (_currentEntry is null)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (_lastPresented == PresentationKind.Original) // TODO: Simplify nesting.
        {
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
                _currentEntry,
                _currentReduced,
                cancellationToken);
            UpdateState();

            return;
        }

        _currentOriginal ??= await _cachePipeline.GetOriginalAsync(
            _currentEntry,
            cancellationToken);

        _lastPresented = PresentationKind.Original;
        await PresentAsync(
            _currentEntry,
            _currentOriginal,
            cancellationToken);
        UpdateState();
    }

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
        if (entry is null)
        {
            await _presenter
                .ClearAsync()
                .ToObservable()
                .ToTask(cancellationToken);
            _currentEntry = null;
            _currentReduced = null;
            _currentOriginal = null;
            _lastPresented = PresentationKind.None;
            UpdateState();

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
                    entry,
                    reduced,
                    cancellationToken);
                WarmNeighbors(cancellationToken);
                UpdateState();

                return;
            }
        }

        var original = await _cachePipeline.GetOriginalAsync(
            entry,
            cancellationToken);
        _currentOriginal = original;
        _lastPresented = PresentationKind.Original;
        await PresentAsync(
            entry,
            original,
            cancellationToken);
        if ((_mode == WarmthMode.Cold) &&
            entry.IsDiskCacheEligible)
        {
            FireAndForget.RunAsync(
                EnsureCurrentReducedAsync(
                    entry,
                    cancellationToken),
                cancellationToken);
        }

        WarmNeighbors(cancellationToken);
        UpdateState();
    }

    private async Task EnsureCurrentReducedAsync(
        ImageEntry entry,
        CancellationToken cancellationToken)
    {
        var reduced = await _cachePipeline.GetReducedAsync(
            entry,
            _screenMetrics,
            cancellationToken);
        if (reduced is null)
        {
            return;
        }

        if ((_currentEntry is not null) &&
            string.Equals(_currentEntry.CacheKey, entry.CacheKey, StringComparison.Ordinal))
        {
            _currentReduced = reduced;
            UpdateState();
        }
    }

    private async Task PresentAsync(
        ImageEntry entry,
        ImageDataResult data,
        CancellationToken cancellationToken)
    {
        const string PerformanceOperationName = "Display";

        var stopwatch = Stopwatch.StartNew();
        await _presenter.ShowAsync(
            data.Bytes,
            cancellationToken);
        stopwatch.Stop();

        _logger.LogDuration(
            new PerformanceMeasureInput(
                PerformanceOperationName,
                entry.FileName,
                data.Source,
                _mode),
            data.Metadata,
            stopwatch.Elapsed.TotalMilliseconds);
    }

    private void WarmNeighbors(CancellationToken cancellationToken)
    {
        if (_navigator is null)
        {
            return;
        }

        if ((_mode == WarmthMode.Cool) ||
            (_mode == WarmthMode.Hot))
        {
            var task = _cachePipeline.WarmNeighborsAsync(
                _entries,
                _navigator.CurrentIndex,
                _screenMetrics,
                cancellationToken);

            FireAndForget.RunAsync(
                task,
                cancellationToken);
        }
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

    private void UpdateState()
    {
        if (_navigator is null)
        {
            PublishState(new ViewerState(
                false,
                false,
                false,
                NonAllocStrings.ToggleShowOriginal,
                NonAllocStrings.WindowTitleFallback));

            return;
        }

        var canToggle = _lastPresented switch
        {
            PresentationKind.Reduced => _currentEntry is not null,
            PresentationKind.Original => _currentReduced is not null,
            _ => false,
        };
        var toggleText = _lastPresented == PresentationKind.Original
            ? NonAllocStrings.ToggleShowReduced
            : NonAllocStrings.ToggleShowOriginal;
        var title = _currentEntry?.FileName ?? NonAllocStrings.WindowTitleFallback;
        PublishState(new ViewerState(
            _navigator.CanMovePrevious,
            _navigator.CanMoveNext,
            canToggle,
            toggleText,
            title));
    }

    private void PublishState(ViewerState state)
    {
        Dispatcher.UIThread.Post(() => StateChanged?.Invoke(state));
    }
}