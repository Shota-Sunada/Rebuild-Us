using HarmonyLib;
using RebuildUs.Extensions;
using RebuildUs.Roles;
using RebuildUs.Utilities;
using System;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class GameDataPatch
{
    [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
    [HarmonyPrefix]
    public static bool RecomputeTaskCountsPrefix(GameData __instance)
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), [typeof(PlayerControl), typeof(DisconnectReasons)])]
    public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
    {
        if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
        {
            Role.AllRoles.Do(x => x.HandleDisconnect(player, reason));
            Modifier.AllModifiers.Do(x => x.HandleDisconnect(player, reason));

            GameHistory.FinalStatuses[player.PlayerId] = FinalStatus.Disconnected;
        }
    }
}