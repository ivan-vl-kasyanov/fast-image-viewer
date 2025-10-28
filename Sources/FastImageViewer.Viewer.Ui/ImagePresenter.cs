// <copyright file="ImagePresenter.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace FastImageViewer.Viewer.Ui;

/// <summary>
/// Presents bitmap images within an <see cref="Image"/> control.
/// </summary>
/// <param name="target">The target control that displays images.</param>
internal sealed class ImagePresenter(Image target) : IDisposable
{
    private readonly Image _target = target;

    private readonly Lock _disposeLock = new();

    private bool _disposed;
    private Bitmap? _current;

    /// <summary>
    /// Displays the supplied image bytes on the associated control.
    /// </summary>
    /// <param name="bytes">The encoded image bytes to display.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when the image is shown.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public Task ShowAsync(
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        return Dispatcher
            .UIThread
            .InvokeAsync(
                () => Present(
                    bytes,
                    cancellationToken),
                DispatcherPriority.Background,
                cancellationToken)
            .GetTask();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_disposeLock)
        {
            if (_disposed)
            {
                return;
            }

            ClearInternal();

            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }

    /// <summary>
    /// Clears the displayed image.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that completes when the control is cleared.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public Task ClearAsync(CancellationToken cancellationToken)
    {
        return Dispatcher
            .UIThread
            .InvokeAsync(
                ClearInternal,
                DispatcherPriority.Background,
                cancellationToken)
            .GetTask();
    }

    private void Present(
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        using var stream = new MemoryStream(
            bytes,
            false);
        var bitmap = new Bitmap(stream);
        _current?.Dispose();
        _target.Source = bitmap;
        _current = bitmap;
    }

    private void ClearInternal()
    {
        _current?.Dispose();
        _current = null;
        _target.Source = null;
    }
}