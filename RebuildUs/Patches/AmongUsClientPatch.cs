using HarmonyLib;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class AmongUsClientPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static void OnGameEndPrefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        EndGameExtensions.OnGameEndPrefix(ref endGameResult);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static void OnGameEndPostfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        EndGameExtensions.OnGameEndPostfix();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static void OnPlayerJoinedPostfix(AmongUsClient __instance)
    {
        if (PlayerControl.LocalPlayer != null)
        {
            Helpers.shareGameVersion();
        }
        LobbyExtensions.sendGamemode = true;
    }
}