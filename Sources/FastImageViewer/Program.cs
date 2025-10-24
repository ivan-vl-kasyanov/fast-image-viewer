// <copyright file="Program.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Runtime.Versioning;

using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;

using Avalonia;

using FastImageViewer.Configuration;

using ReactiveUI.Avalonia;

using Serilog;
using Serilog.Events;

namespace FastImageViewer;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var logInitialized = ConfigureLogging();
        if (!logInitialized)
        {
            return;
        }

        try
        {
            var parseResult = WarmthModeParser.Parse(args);
            var cacheDirectory = AppPaths.CacheDirectory;

            if (parseResult.Mode == WarmthMode.Clean)
            {
                RunCleanMode(cacheDirectory);

                return;
            }

            ConfigureCache(cacheDirectory);

            AppStartupContext.SetMode(parseResult.Mode);

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(parseResult.RemainingArgs);
        }
        catch (Exception ex)
        {
            Log.Fatal(
                ex,
                "Unhandled fatal error in application entry point.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
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

    private static void ConfigureCache(string cacheDirectory)
    {
        var localMachinePath = Path.Combine(
            cacheDirectory,
            AppConstants.LocalMachineCacheFileName);
        var scheduler = CacheDatabase.TaskpoolScheduler ?? TaskPoolScheduler.Default;
        var serializer = new SystemJsonSerializer();
        var customCache = new SqliteBlobCache(
            localMachinePath,
            serializer,
            scheduler,
            false);

        Splat
            .Builder
            .AppBuilder
            .CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(builder =>
            {
                builder
                    .WithApplicationName(AppConstants.AppName)
                    .WithSqliteProvider()
                    .WithSqliteDefaults()
                    .WithLocalMachine(customCache);
            });
    }

    private static void RunCleanMode(string cacheDirectory)
    {
        try
        {
            var scheduler = CacheDatabase.TaskpoolScheduler ?? TaskPoolScheduler.Default;
            var localMachinePath = Path.Combine(
                cacheDirectory,
                AppConstants.LocalMachineCacheFileName);

            using var cache = new SqliteBlobCache(
                localMachinePath,
                new SystemJsonSerializer(),
                scheduler,
                false);

            cache
                .InvalidateAll()
                .ToTask()
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Failed to invalidate cache entries.");
        }

        try
        {
            if (Directory.Exists(cacheDirectory))
            {
                Directory.Delete(
                    cacheDirectory,
                    true);
            }
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Failed to delete cache directory \"{CacheDirectory}\".",
                cacheDirectory);
        }
    }

    private static bool ConfigureLogging()
    {
        try
        {
            var logFilePath = Path.Combine(
                AppPaths.CacheDirectory,
                AppConstants.LogFileName);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.File(
                    logFilePath,
                    fileSizeLimitBytes: 1_048_576,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Month,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 3,
                    shared: true)
                .CreateLogger();

            Log.Information("Program starting...");

            return true;
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            Console.ReadKey();

            return false;
        }
    }
}