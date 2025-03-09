using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
internal static class GameStartManagerStartPatch
{
    internal static void Postfix(GameStartManager __instance)
    {
    }
}