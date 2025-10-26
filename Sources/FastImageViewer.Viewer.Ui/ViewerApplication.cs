// <copyright file="ViewerApplication.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia;

using ReactiveUI.Avalonia;

namespace FastImageViewer.Viewer.Ui;

/// <summary>
/// Provides helpers to bootstrap the Avalonia viewer application.
/// </summary>
public static class ViewerApplication
{
    /// <summary>
    /// Starts the viewer application using the classic desktop lifetime.
    /// </summary>
    /// <param name="args">The command-line arguments supplied to the process.</param>
    public static void Start(string[] args)
    {
        Build().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Builds the <see cref="AppBuilder"/> used to run the application.
    /// </summary>
    /// <returns>The configured application builder.</returns>
    public static AppBuilder Build()
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