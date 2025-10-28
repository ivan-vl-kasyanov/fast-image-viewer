// <copyright file="Program.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Runtime.Versioning;

using Avalonia;

using FastImageViewer.Shared.FastImageViewer.Configuration;

using Serilog;

namespace FastImageViewer;

/// <summary>
/// Provides the entry point for the Fast Image Viewer application.
/// </summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
internal static class Program
{
    /// <summary>
    /// Starts the application with the provided command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments supplied to the process.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        var applicationPaths = new ApplicationPaths();
        if (!LoggingInitializer.TryConfigure(applicationPaths))
        {
            return;
        }

        try
        {
            CacheInitializer.Configure(applicationPaths.CacheDirectory);

            AvaloniaInitializer
                .BuildAvaloniaApplication()
                .StartWithClassicDesktopLifetime(args);
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
}