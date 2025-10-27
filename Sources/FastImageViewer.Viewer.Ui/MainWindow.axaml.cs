// <copyright file="MainWindow.axaml.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using FastImageViewer.Resources;
using FastImageViewer.Shared.FastImageViewer.Threading;
using FastImageViewer.Viewer.Ui.Models;
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
    public MainWindow()
    {
        InitializeComponent();

        var controls = ResolveControlReferences();
        var displayImage = controls.DisplayImage;
        _closeButton = controls.CloseButton;
        _backwardButton = controls.BackwardButton;
        _forwardButton = controls.ForwardButton;
        _toggleOriginalButton = controls.ToggleOriginalButton;
        _loadingContainer = controls.LoadingContainer;
        _cachingContainer = controls.CachingContainer;
        _cachingProgressBar = controls.CachingProgressBar;
        _errorContainer = controls.ErrorContainer;
        _errorTextBlock = controls.ErrorTextBlock;

        var presenter = new ImagePresenter(displayImage);
        _controller = new MainController(
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

    private (
        Image DisplayImage,
        Button CloseButton,
        Button BackwardButton,
        Button ForwardButton,
        Button ToggleOriginalButton,
        Border LoadingContainer,
        Border CachingContainer,
        ProgressBar CachingProgressBar,
        Border ErrorContainer,
        TextBlock ErrorTextBlock)
        ResolveControlReferences()
    {
        return (
            FindRequiredControl<Image>(AppInvariantStringConstants.ControlDisplayImageName),
            FindRequiredControl<Button>(AppInvariantStringConstants.ControlCloseButtonName),
            FindRequiredControl<Button>(AppInvariantStringConstants.ControlBackwardButtonName),
            FindRequiredControl<Button>(AppInvariantStringConstants.ControlForwardButtonName),
            FindRequiredControl<Button>(AppInvariantStringConstants.ControlToggleOriginalButtonName),
            FindRequiredControl<Border>(AppInvariantStringConstants.ControlLoadingContainerName),
            FindRequiredControl<Border>(AppInvariantStringConstants.ControlCachingContainerName),
            FindRequiredControl<ProgressBar>(AppInvariantStringConstants.ControlCachingProgressBarName),
            FindRequiredControl<Border>(AppInvariantStringConstants.ControlErrorContainerName),
            FindRequiredControl<TextBlock>(AppInvariantStringConstants.ControlErrorTextBlockName));
    }

    private T FindRequiredControl<T>(string name)
        where T : Control
    {
        return this.FindControl<T>(name)
            ?? throw new InvalidOperationException($"{name} control not found.");
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