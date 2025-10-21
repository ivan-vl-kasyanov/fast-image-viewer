// <copyright file="WarmthParseResult.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Configuration;

internal sealed record WarmthParseResult(
    WarmthMode Mode,
    string[] RemainingArgs);