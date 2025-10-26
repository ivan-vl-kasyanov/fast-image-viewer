// <copyright file="FireAndForget.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using Serilog;

namespace FastImageViewer.Shared.FastImageViewer.Threading;

/// <summary>
/// Provides helpers for fire-and-forget task execution.
/// </summary>
public static class FireAndForget
{
    /// <summary>
    /// Safely observes failures for the supplied task without awaiting it.
    /// </summary>
    /// <param name="task">The task to monitor.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    public static void RunAsync(
        Task task,
        CancellationToken cancellationToken)
    {
        if (task is null)
        {
            return;
        }

        task.ContinueWith(
            static t => HandleFailure(t),
            cancellationToken,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    private static void HandleFailure(Task task)
    {
        if (!task.IsFaulted)
        {
            return;
        }

        Log.Error(
            task.Exception,
            "Unhandled exception occurred in fire-and-forget task.");
    }
}