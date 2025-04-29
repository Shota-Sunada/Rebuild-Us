using System;
using HarmonyLib;
using RebuildUs.Roles;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class PlayerControlPatch
{
    private static bool resetToCrewmate = false;
    private static bool resetToDead = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public static void RpcSyncSettingsPostfix()
    {
        // CustomOption.ShareOptionSelections();
        // CustomOption.SaveVanillaOptions();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void MurderPlayerPrefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Allow everyone to murder players
        resetToCrewmate = !__instance.Data.Role.IsImpostor;
        resetToDead = __instance.Data.IsDead;
        __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
        __instance.Data.IsDead = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Collect dead player info
        var deadPlayer = new DeadPlayer(target, DateTime.UtcNow, CustomDeathReason.Kill, __instance);
        GameHistory.DeadPlayers.Add(deadPlayer);

        // Reset killer to crewmate if resetToCrewmate
        if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
        if (resetToDead) __instance.Data.IsDead = true;

        // Remove fake tasks when player dies
        if (target.HasFakeTasks())
        {
            target.ClearAllTasks();
        }

        // 追記するならここ

        RoleHelpers.OnKill(__instance, target);
        RoleHelpers.OnDeath(target, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static void ExiledPostfix(PlayerControl __instance)
    {
        // Collect dead player info
        var deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, CustomDeathReason.Exile, null);
        GameHistory.DeadPlayers.Add(deadPlayer);

        // Remove fake tasks when player dies
        if (__instance.HasFakeTasks())
        {
            __instance.ClearAllTasks();
        }

        RoleHelpers.OnDeath(__instance, killer: null);

        // 追記するならここ
    }
}