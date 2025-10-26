// <copyright file="WarmthParseResult.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Shared.FastImageViewer.Configuration;

namespace FastImageViewer;

/// <summary>
/// Represents the parsed warm-up mode and remaining command-line arguments.
/// </summary>
/// <param name="Mode">The parsed <see cref="WarmthMode"/> value.</param>
/// <param name="RemainingArgs">The command-line arguments that were not consumed.</param>
internal sealed record WarmthParseResult(
    WarmthMode Mode,
    string[] RemainingArgs);