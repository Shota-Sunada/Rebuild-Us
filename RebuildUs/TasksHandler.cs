using RebuildUs.Utilities;

namespace RebuildUs;

internal static class TasksHandler
{
    internal static (int completedTasks, int totalTasks) TaskInfo(NetworkedPlayerInfo playerInfo)
    {
        var totalTasks = 0;
        var completedTasks = 0;

        if (playerInfo != null && !playerInfo.Disconnected && playerInfo.Tasks != null &&
            playerInfo.Object &&
            playerInfo.Role && playerInfo.Role.TasksCountTowardProgress &&
            !playerInfo.Object.HasFakeTasks() && !playerInfo.Role.IsImpostor)
        {
            foreach (var task in playerInfo.Tasks.GetFastEnumerator())
            {
                if (task.Complete) completedTasks++;
                totalTasks++;
            }
        }

        return (completedTasks, totalTasks);
    }
}