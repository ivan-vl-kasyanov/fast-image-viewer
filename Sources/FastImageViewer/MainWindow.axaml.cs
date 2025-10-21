// <copyright file="MainWindow.axaml.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using FastImageViewer.Configuration;
using FastImageViewer.Diagnostics;
using FastImageViewer.Presentation;
using FastImageViewer.Threading;
using FastImageViewer.Ui;

namespace FastImageViewer;

internal sealed partial class MainWindow : Window
{
    private const string ImageDisplayName = "DisplayImage";
    private const string ButtonCloseName = "CloseButton";
    private const string ButtonBackwardName = "BackwardButton";
    private const string ButtonForwardName = "ForwardButton";
    private const string ButtonToggleOriginalName = "ToggleOriginalButton";

    private readonly MainController _controller;
    private readonly Button _closeButton;
    private readonly Button _backwardButton;
    private readonly Button _forwardButton;
    private readonly Button _toggleOriginalButton;

    public MainWindow(
        WarmthMode mode,
        PerformanceLogger logger)
    {
        InitializeComponent();

        var displayImage = this.FindControl<Image>(ImageDisplayName)
            ?? throw new InvalidOperationException($"{ImageDisplayName} control not found.");
        _closeButton = this.FindControl<Button>(ButtonCloseName)
            ?? throw new InvalidOperationException($"{ButtonCloseName} control not found.");
        _backwardButton = this.FindControl<Button>(ButtonBackwardName)
            ?? throw new InvalidOperationException($"{ButtonBackwardName} control not found.");
        _forwardButton = this.FindControl<Button>(ButtonForwardName)
            ?? throw new InvalidOperationException($"{ButtonForwardName} control not found.");
        _toggleOriginalButton = this.FindControl<Button>(ButtonToggleOriginalName)
            ?? throw new InvalidOperationException($"{ButtonToggleOriginalName} control not found.");

        var presenter = new ImagePresenter(displayImage);
        _controller = new MainController(
            mode,
            logger,
            presenter);
        _controller.StateChanged += OnStateChanged;
        _controller.CloseRequested += OnCloseRequested;

        _closeButton.Click += (_, _) => _controller.RequestApplicationExit();
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

    private void OnBackward(object? sender, RoutedEventArgs e)
    {
        FireAndForget.RunAsync(
            _controller.MoveBackwardAsync(default),
            default);
    }

    private void OnForward(object? sender, RoutedEventArgs e)
    {
        FireAndForget.RunAsync(
            _controller.MoveForwardAsync(default),
            default);
    }

    private void OnToggleOriginal(object? sender, RoutedEventArgs e)
    {
        FireAndForget.RunAsync(
            _controller.ToggleOriginalAsync(default),
            default);
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        FireAndForget.RunAsync(
            _controller.InitializeAsync(
                this,
                default),
            default);
    }

    private void OnClosed(object? sender, EventArgs e)
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
    }
}