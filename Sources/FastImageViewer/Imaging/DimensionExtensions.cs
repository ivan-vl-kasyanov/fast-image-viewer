// <copyright file="DimensionExtensions.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Imaging;

/// <summary>
/// Provides helpers for validating dimension values.
/// </summary>
internal static class DimensionExtensions
{
    /// <summary>
    /// Ensures that a <see cref="double"/> dimension fits within the <see cref="int"/> range.
    /// </summary>
    /// <param name="dimension">The dimension value to validate.</param>
    /// <param name="propertyName">The name of the associated property.</param>
    /// <returns>The validated dimension converted to <see cref="int"/>.</returns>
    /// <exception cref="OverflowException">Thrown when the dimension exceeds the <see cref="int"/> range.</exception>
    public static int EnsureDimensionWithinInt32Range(
        this double dimension,
        string propertyName)
    {
        return (dimension < int.MinValue) || (dimension > int.MaxValue)
            ? throw new OverflowException(
                $"The dimension \"{propertyName}\" value \"{dimension}\" is outside the Int32 range.")
            : Convert.ToInt32(dimension);
    }

    /// <summary>
    /// Ensures that a <see cref="uint"/> dimension fits within the <see cref="int"/> range.
    /// </summary>
    /// <param name="dimension">The dimension value to validate.</param>
    /// <param name="propertyName">The name of the associated property.</param>
    /// <returns>The validated dimension converted to <see cref="int"/>.</returns>
    /// <exception cref="OverflowException">Thrown when the dimension exceeds the <see cref="int"/> range.</exception>
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