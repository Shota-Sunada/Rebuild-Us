using HarmonyLib;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class EndGameManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static void SetEverythingUpPostfix(EndGameManager __instance)
    {
        EndGameExtensions.SetEverythingUpPostfix(__instance);
    }
}