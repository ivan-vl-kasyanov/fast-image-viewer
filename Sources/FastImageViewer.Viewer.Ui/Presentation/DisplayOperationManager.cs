// <copyright file="DisplayOperationManager.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Viewer.Ui.Presentation;

/// <summary>
/// Manages cancellation for in-flight display operations.
/// </summary>
internal sealed class DisplayOperationManager
{
    private CancellationTokenSource? _current;

    /// <summary>
    /// Starts a new display operation linked to the provided cancellation token.
    /// </summary>
    /// <param name="cancellationToken">The token that controls cancellation for the operation.</param>
    /// <returns>The cancellation token to use for the started operation.</returns>
    public CancellationToken Start(CancellationToken cancellationToken)
    {
        Cancel();

        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _current = source;

        return source.Token;
    }

    /// <summary>
    /// Cancels the currently running display operation, if any.
    /// </summary>
    public void Cancel()
    {
        if (_current is null)
        {
            return;
        }

        _current.Cancel();
        _current.Dispose();
        _current = null;
    }
}