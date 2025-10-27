// <copyright file="AppImageExtensionConstants.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using System.Collections.Immutable;

namespace FastImageViewer.Resources;

/// <summary>
/// Provides grouped image extension collections used across the application.
/// </summary>
public static class AppImageExtensionConstants
{
    /// <summary>
    /// The common file extensions that receive special processing.
    /// </summary>
    public static readonly ImmutableArray<string> ComplicatedImageExtensions = ImmutableArray.Create(
        ExtensionHeic,
        ExtensionHeif,
        ExtensionTif,
        ExtensionTiff,
        Extension3Fr,
        ExtensionArw,
        ExtensionCr2,
        ExtensionCr3,
        ExtensionCrw,
        ExtensionDcr,
        ExtensionDng,
        ExtensionErf,
        ExtensionKdc,
        ExtensionMef,
        ExtensionMos,
        ExtensionMrw,
        ExtensionNef,
        ExtensionNrw,
        ExtensionOrf,
        ExtensionPef,
        ExtensionRaf,
        ExtensionRaw,
        ExtensionRw2,
        ExtensionSr2,
        ExtensionSrf,
        ExtensionSrw);

    /// <summary>
    /// The commonly supported file extensions handled without extra processing.
    /// </summary>
    public static readonly ImmutableArray<string> CommonImageExtensions = ImmutableArray.Create(
        ExtensionPng,
        ExtensionJpg,
        ExtensionJpeg,
        ExtensionBmp,
        ExtensionGif,
        ExtensionWebp,
        ExtensionAvif);

    private const string Extension3Fr = ".3fr";
    private const string ExtensionArw = ".arw";
    private const string ExtensionAvif = ".avif";
    private const string ExtensionBmp = ".bmp";
    private const string ExtensionCr2 = ".cr2";
    private const string ExtensionCr3 = ".cr3";
    private const string ExtensionCrw = ".crw";
    private const string ExtensionDcr = ".dcr";
    private const string ExtensionDng = ".dng";
    private const string ExtensionErf = ".erf";
    private const string ExtensionGif = ".gif";
    private const string ExtensionHeic = ".heic";
    private const string ExtensionHeif = ".heif";
    private const string ExtensionJpeg = ".jpeg";
    private const string ExtensionJpg = ".jpg";
    private const string ExtensionKdc = ".kdc";
    private const string ExtensionMef = ".mef";
    private const string ExtensionMos = ".mos";
    private const string ExtensionMrw = ".mrw";
    private const string ExtensionNef = ".nef";
    private const string ExtensionNrw = ".nrw";
    private const string ExtensionOrf = ".orf";
    private const string ExtensionPef = ".pef";
    private const string ExtensionPng = ".png";
    private const string ExtensionRaf = ".raf";
    private const string ExtensionRaw = ".raw";
    private const string ExtensionRw2 = ".rw2";
    private const string ExtensionSr2 = ".sr2";
    private const string ExtensionSrf = ".srf";
    private const string ExtensionSrw = ".srw";
    private const string ExtensionTif = ".tif";
    private const string ExtensionTiff = ".tiff";
    private const string ExtensionWebp = ".webp";
}