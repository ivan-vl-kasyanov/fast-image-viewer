// <copyright file="IFireAndForget.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

namespace FastImageViewer.Shared.FastImageViewer.Threading;

/// <summary>
/// Defines helpers for observing fire-and-forget task execution.
/// </summary>
public interface IFireAndForget
{
    /// <summary>
    /// Safely observes failures for the supplied task without awaiting it.
    /// </summary>
    /// <param name="task">The task to monitor.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    void RunAsync(
        Task task,
        CancellationToken cancellationToken);
}