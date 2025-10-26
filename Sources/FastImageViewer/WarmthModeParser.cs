// <copyright file="WarmthModeParser.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Shared.FastImageViewer.Configuration;

namespace FastImageViewer;

/// <summary>
/// Provides utilities for parsing the application warm-up mode.
/// </summary>
internal static class WarmthModeParser
{
    /// <summary>
    /// Parses the command-line arguments for a requested <see cref="WarmthMode"/>.
    /// </summary>
    /// <param name="args">The command-line arguments to inspect.</param>
    /// <returns>The parsed warm-up mode and remaining arguments.</returns>
    public static WarmthParseResult Parse(string[] args)
    {
        const string ParameterNameMode = "--mode=";
        const string ParameterNameModeHot = "hot";
        const string ParameterNameModeClean = "clean";

        if (args.Length == 0)
        {
            return new WarmthParseResult(
                WarmthMode.Cold,
                []);
        }

        var remaining = new List<string>(args.Length);
        var mode = WarmthMode.Cold;
        foreach (var argument in args)
        {
            if (argument.StartsWith(ParameterNameMode, StringComparison.OrdinalIgnoreCase))
            {
                var value = argument[7..].Trim();
                mode = value.ToLowerInvariant() switch
                {
                    ParameterNameModeHot => WarmthMode.Hot,
                    ParameterNameModeClean => WarmthMode.Clean,
                    _ => WarmthMode.Cold,
                };
            }
            else
            {
                remaining.Add(argument);
            }
        }

        return new WarmthParseResult(
            mode,
            [.. remaining]);
    }
}