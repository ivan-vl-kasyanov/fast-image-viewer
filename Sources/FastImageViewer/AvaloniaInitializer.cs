// <copyright file="AvaloniaInitializer.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia;

using FastImageViewer.Viewer.Ui;

using ReactiveUI.Avalonia;

namespace FastImageViewer;

/// <summary>
/// Provides helpers for configuring Avalonia application.
/// </summary>
internal static class AvaloniaInitializer
{
    /// <summary>
    /// Builds the Avalonia application for desktop previewing and runtime hosting.
    /// </summary>
    /// <returns>The configured application builder instance.</returns>
    public static AppBuilder BuildAvaloniaApplication()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions())
            .With(new X11PlatformOptions())
            .LogToTrace()
            .UseReactiveUI();
    }
}