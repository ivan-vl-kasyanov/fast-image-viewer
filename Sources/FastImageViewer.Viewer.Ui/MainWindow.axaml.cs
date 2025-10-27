// <copyright file="MainWindow.axaml.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using FastImageViewer.Resources;
using FastImageViewer.Shared.FastImageViewer.Configuration;
using FastImageViewer.Shared.FastImageViewer.Threading;
using FastImageViewer.Viewer.Ui.Presentation;

namespace FastImageViewer.Viewer.Ui;

/// <summary>
/// Represents the main application window hosting the image viewer UI.
/// </summary>
internal sealed partial class MainWindow : Window
{
    private readonly MainController _controller;
    private readonly Button _closeButton;
    private readonly Button _backwardButton;
    private readonly Button _forwardButton;
    private readonly Button _toggleOriginalButton;
    private readonly Border _loadingContainer;
    private readonly Border _cachingContainer;
    private readonly ProgressBar _cachingProgressBar;
    private readonly Border _errorContainer;
    private readonly TextBlock _errorTextBlock;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="mode">The warm-up mode determining startup behavior.</param>
    public MainWindow(
        WarmthMode mode)
    {
        InitializeComponent();

        var displayImage = this.FindControl<Image>(AppInvariantStringConstants.ControlDisplayImageName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlDisplayImageName} control not found.");
        _closeButton = this.FindControl<Button>(AppInvariantStringConstants.ControlCloseButtonName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlCloseButtonName} control not found.");
        _backwardButton = this.FindControl<Button>(AppInvariantStringConstants.ControlBackwardButtonName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlBackwardButtonName} control not found.");
        _forwardButton = this.FindControl<Button>(AppInvariantStringConstants.ControlForwardButtonName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlForwardButtonName} control not found.");
        _toggleOriginalButton = this.FindControl<Button>(AppInvariantStringConstants.ControlToggleOriginalButtonName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlToggleOriginalButtonName} control not found.");
        _loadingContainer = this.FindControl<Border>(AppInvariantStringConstants.ControlLoadingContainerName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlLoadingContainerName} control not found.");
        _cachingContainer = this.FindControl<Border>(AppInvariantStringConstants.ControlCachingContainerName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlCachingContainerName} control not found.");
        _cachingProgressBar = this.FindControl<ProgressBar>(AppInvariantStringConstants.ControlCachingProgressBarName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlCachingProgressBarName} control not found.");
        _errorContainer = this.FindControl<Border>(AppInvariantStringConstants.ControlErrorContainerName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlErrorContainerName} control not found.");
        _errorTextBlock = this.FindControl<TextBlock>(AppInvariantStringConstants.ControlErrorTextBlockName)
            ?? throw new InvalidOperationException($"{AppInvariantStringConstants.ControlErrorTextBlockName} control not found.");

        var presenter = new ImagePresenter(displayImage);
        _controller = new MainController(
            mode,
            presenter);
        _controller.StateChanged += OnStateChanged;
        _controller.CloseRequested += OnCloseRequested;

        _closeButton.Click += OnClose;
        _backwardButton.Click += OnBackward;
        _forwardButton.Click += OnForward;
        _toggleOriginalButton.Click += OnToggleOriginal;

        Opened += OnOpened;
        Closed += OnClosed;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnClose(
        object? sender,
        RoutedEventArgs e)
    {
        _controller.ClearError(default);
        _controller.RequestApplicationExit();
    }

    private void OnBackward(
        object? sender,
        RoutedEventArgs e)
    {
        _controller.ClearError(default);
        FireAndForget.RunAsync(
            _controller.MoveBackwardAsync(default),
            default);
    }

    private void OnForward(
        object? sender,
        RoutedEventArgs e)
    {
        _controller.ClearError(default);
        FireAndForget.RunAsync(
            _controller.MoveForwardAsync(default),
            default);
    }

    private void OnToggleOriginal(
        object? sender,
        RoutedEventArgs e)
    {
        _controller.ClearError(default);
        FireAndForget.RunAsync(
            _controller.ToggleOriginalAsync(default),
            default);
    }

    private void OnOpened(
        object? sender,
        EventArgs e)
    {
        FireAndForget.RunAsync(
            _controller.InitializeAsync(
                this,
                default),
            default);
    }

    private void OnClosed(
        object? sender,
        EventArgs e)
    {
        _controller.Dispose();
    }

    private void OnCloseRequested()
    {
        Close();
    }

    private void OnStateChanged(ViewerState state)
    {
        _backwardButton.IsEnabled = state.CanMoveBackward;
        _forwardButton.IsEnabled = state.CanMoveForward;
        _toggleOriginalButton.IsEnabled = state.CanToggleOriginal;
        _toggleOriginalButton.Content = state.ToggleButtonContent;
        Title = state.WindowTitle;
        _loadingContainer.Opacity = state.IsLoading
            ? AppNumericConstants.OpacityVisible
            : AppNumericConstants.OpacityHidden;
        _cachingContainer.Opacity = state.IsCaching
            ? AppNumericConstants.OpacityVisible
            : AppNumericConstants.OpacityHidden;

        var cachingProgress = state.CachingProgress;
        if (cachingProgress < AppNumericConstants.ProgressMinimum)
        {
            cachingProgress = AppNumericConstants.ProgressMinimum;
        }
        else if (cachingProgress > AppNumericConstants.ProgressMaximum)
        {
            cachingProgress = AppNumericConstants.ProgressMaximum;
        }

        _cachingProgressBar.Value = cachingProgress;
        _errorTextBlock.Text = state.ErrorMessage ?? string.Empty;
        var hasError = !string.IsNullOrWhiteSpace(state.ErrorMessage);
        _errorContainer.Opacity = hasError
            ? AppNumericConstants.OpacityVisible
            : AppNumericConstants.OpacityHidden;
        _errorContainer.IsHitTestVisible = hasError;
    }
}