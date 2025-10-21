// <copyright file="FireAndForget.cs" company="Ivan Kasyanov">
// © 2025 Ivan Kasyanov.
// This software is licensed under the GNU Affero General Public License Version 3. See LICENSE for details.
// </copyright>

using FastImageViewer.Configuration;

namespace FastImageViewer.Threading;

internal static class FireAndForget
{
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

        try
        {
            var logger = AppStartupContext.GetLogger();
            logger.LogBackgroundError(task.Exception);
        }
        catch (Exception ex)
        {
            Console
                .Error
                .WriteLine($"Failed to log background error: \"{ex}\".");
        }
    }
}