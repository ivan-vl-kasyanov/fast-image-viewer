// <copyright file="PerformanceMeasureInput.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Configuration;

namespace FastImageViewer.Diagnostics;

internal sealed record PerformanceMeasureInput(
    string Operation,
    string FileName,
    string Source,
    WarmthMode Mode);