// <copyright file="WarmthModeParser.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Resources;
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
        if (args.Length == 0)
        {
            return new WarmthParseResult(
                WarmthMode.Cold,
                []);
        }

        var remaining = new List<string>(args.Length);
        var mode = WarmthMode.Cold;
        var modePrefix = AppInvariantStringConstants.CommandLineModeParameterPrefix;
        var modePrefixLength = modePrefix.Length;
        foreach (var argument in args)
        {
            if (argument.StartsWith(modePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var value = argument[modePrefixLength..].Trim();
                mode = value.ToLowerInvariant() switch
                {
                    AppInvariantStringConstants.CommandLineModeHotValue => WarmthMode.Hot,
                    AppInvariantStringConstants.CommandLineModeCleanValue => WarmthMode.Clean,
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