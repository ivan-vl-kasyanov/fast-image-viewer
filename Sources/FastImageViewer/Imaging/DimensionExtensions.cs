// <copyright file="DimensionExtensions.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Imaging;

internal static class DimensionExtensions
{
    public static int EnsureDimensionWithinInt32Range(
        this double dimension,
        string propertyName)
    {
        return (dimension < int.MinValue) || (dimension > int.MaxValue)
            ? throw new OverflowException(
                $"The dimension \"{propertyName}\" value \"{dimension}\" is outside the Int32 range.")
            : Convert.ToInt32(dimension);
    }

    public static int EnsureDimensionWithinInt32Range(
        this uint dimension,
        string propertyName)
    {
        return (dimension > int.MaxValue)
            ? throw new OverflowException(
                $"The dimension \"{propertyName}\" value \"{dimension}\" is outside the Int32 range.")
            : Convert.ToInt32(dimension);
    }
}