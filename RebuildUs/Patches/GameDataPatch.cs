using HarmonyLib;
using RebuildUs.Utilities;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class GameDataPatch
{
    [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
    [HarmonyPrefix]
    internal static bool RecomputeTaskCountsPrefix(GameData __instance)
    {
        var totalTasks = 0;
        var completedTasks = 0;

        foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            // if (playerInfo.Object)
            // {
            //     continue;
            // }

            var (playerCompleted, playerTotal) = TasksHandler.TaskInfo(playerInfo);
            totalTasks += playerTotal;
            completedTasks += playerCompleted;
        }

        __instance.TotalTasks = totalTasks;
        __instance.CompletedTasks = completedTasks;
        return false;
    }
}