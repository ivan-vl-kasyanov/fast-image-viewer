// <copyright file="GalleryNavigator.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.ImageProcessing.Imaging;

namespace FastImageViewer.ImageProcessing.Gallery;

/// <summary>
/// Provides navigation helpers for an ordered list of gallery entries.
/// </summary>
/// <param name="entries">The image entries to navigate.</param>
public sealed class GalleryNavigator(IReadOnlyList<ImageEntry> entries)
{
    private readonly IReadOnlyList<ImageEntry> _entries = entries;

    private int _index = 0;

    /// <summary>
    /// Gets the number of entries available for navigation.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Gets the current entry or <c>null</c> when the list is empty.
    /// </summary>
    public ImageEntry? Current => _entries.Count == 0
        ? null
        : _entries[_index];

    /// <summary>
    /// Gets the current index or -1 when the list is empty.
    /// </summary>
    public int CurrentIndex => _entries.Count == 0
        ? -1
        : _index;

    /// <summary>
    /// Gets a value indicating whether a previous entry is available.
    /// </summary>
    public bool CanMovePrevious => _index > 0;

    /// <summary>
    /// Gets a value indicating whether a next entry is available.
    /// </summary>
    public bool CanMoveNext => _index < _entries.Count - 1;

    /// <summary>
    /// Moves to the previous entry when available.
    /// </summary>
    /// <returns>The current entry after navigation.</returns>
    public ImageEntry? MovePrevious()
    {
        if (!CanMovePrevious)
        {
            return Current;
        }

        _index--;

        return Current;
    }

    /// <summary>
    /// Moves to the next entry when available.
    /// </summary>
    /// <returns>The current entry after navigation.</returns>
    public ImageEntry? MoveNext()
    {
        if (!CanMoveNext)
        {
            return Current;
        }

        _index++;

        return Current;
    }

    /// <summary>
    /// Resets navigation to the first entry.
    /// </summary>
    public void Reset()
    {
        _index = 0;
    }

    /// <summary>
    /// Peeks at the previous entry without changing the current index.
    /// </summary>
    /// <returns>The previous entry or <c>null</c> when unavailable.</returns>
    public ImageEntry? PeekPrevious()
    {
        return _index <= 0
            ? null
            : _entries[_index - 1];
    }

    /// <summary>
    /// Peeks at the next entry without changing the current index.
    /// </summary>
    /// <returns>The next entry or <c>null</c> when unavailable.</returns>
    public ImageEntry? PeekNext()
    {
        return _index >= _entries.Count - 1
            ? null
            : _entries[_index + 1];
    }

    /// <summary>
    /// Gets the entry at the specified position.
    /// </summary>
    /// <param name="position">The zero-based position to inspect.</param>
    /// <returns>The entry at the position or <c>null</c> when out of range.</returns>
    public ImageEntry? GetAt(int position)
    {
        return (position < 0) || (position >= _entries.Count)
            ? null
            : _entries[position];
    }
}