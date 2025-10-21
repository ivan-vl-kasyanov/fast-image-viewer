// <copyright file="Program.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Runtime.Versioning;

using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;

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

        Splat
            .Builder
            .AppBuilder
            .CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(builder => builder
                .WithApplicationName(AppConstants.AppName)
                .WithSqliteProvider()
                .WithSqliteDefaults());

        using var logger = PerformanceLogger.Create(parseResult.Mode);
        AppStartupContext.SetLogger(logger);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(parseResult.RemainingArgs);
    }

    private static Avalonia.AppBuilder BuildAvaloniaApp()
    {
        return Avalonia
            .AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions())
            .With(new X11PlatformOptions())
            .LogToTrace()
            .UseReactiveUI();
    }
}