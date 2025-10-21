// <copyright file="Program.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Runtime.Versioning;

using Avalonia;

using FastImageViewer.Configuration;
using FastImageViewer.Diagnostics;

using ReactiveUI.Avalonia;

namespace FastImageViewer;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var parseResult = WarmthModeParser.Parse(args);
        AppStartupContext.SetMode(parseResult.Mode);
        BlobCache.ApplicationName = AppConstants.AppName; // `BlobCache` not found. Note that "akavache.core" library is deprecated, use "Akavache".
        Akavache
            .Registrations
            .Start(AppConstants.AppName); // `Registrations` not found in the `Akavache` namespace. Note that "akavache.core" library is deprecated, use "Akavache".
        using var logger = PerformanceLogger.Create(parseResult.Mode);
        AppStartupContext.SetLogger(logger);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(parseResult.RemainingArgs);
    }

    private static AppBuilder BuildAvaloniaApp()
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