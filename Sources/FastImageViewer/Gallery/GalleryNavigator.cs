// <copyright file="GalleryNavigator.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Imaging;

namespace FastImageViewer.Gallery;

internal sealed class GalleryNavigator(IReadOnlyList<ImageEntry> entries)
{
    private readonly IReadOnlyList<ImageEntry> _entries = entries;

    public int Count => _entries.Count;

    public ImageEntry? Current => _entries.Count == 0
        ? null
        : _entries[_index];

    public int CurrentIndex => _entries.Count == 0
        ? -1
        : _index;

    public bool CanMovePrevious => _index > 0;

    public bool CanMoveNext => _index < _entries.Count - 1;

    private int _index = 0;

    public ImageEntry? MovePrevious()
    {
        if (!CanMovePrevious)
        {
            return Current;
        }

        _index--;

        return Current;
    }

    public ImageEntry? MoveNext()
    {
        if (!CanMoveNext)
        {
            return Current;
        }

        _index++;

        return Current;
    }

    public void Reset()
    {
        _index = 0;
    }

    public ImageEntry? PeekPrevious()
    {
        return _index <= 0
            ? null
            : _entries[_index - 1];
    }

    public ImageEntry? PeekNext()
    {
        return _index >= _entries.Count - 1
            ? null
            : _entries[_index + 1];
    }

    public ImageEntry? GetAt(int position)
    {
        return (position < 0) || (position >= _entries.Count)
            ? null
            : _entries[position];
    }
}