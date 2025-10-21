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
    private readonly MainController _controller;

    public MainWindow(
        WarmthMode mode,
        PerformanceLogger logger)
    {
        InitializeComponent();
        var presenter = new ImagePresenter(DisplayImage);
        _controller = new MainController(
            mode,
            logger,
            presenter);
        _controller.StateChanged += OnStateChanged;
        _controller.CloseRequested += OnCloseRequested;

        CloseButton.Click += (_, _) => _controller.RequestApplicationExit();
        BackwardButton.Click += OnBackward;
        ForwardButton.Click += OnForward;
        ToggleOriginalButton.Click += OnToggleOriginal;

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
        BackwardButton.IsEnabled = state.CanMoveBackward;
        ForwardButton.IsEnabled = state.CanMoveForward;
        ToggleOriginalButton.IsEnabled = state.CanToggleOriginal;
        ToggleOriginalButton.Content = state.ToggleButtonContent;
        Title = state.WindowTitle;
    }
}