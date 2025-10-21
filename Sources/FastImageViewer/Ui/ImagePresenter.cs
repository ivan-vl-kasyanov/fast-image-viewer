// <copyright file="ImagePresenter.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace FastImageViewer.Ui;

internal sealed class ImagePresenter(Image target) : IDisposable
{
    private readonly Image _target = target;

    private Bitmap? _current;

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

    public void Dispose()
    {
        _current?.Dispose();
        _current = null;
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