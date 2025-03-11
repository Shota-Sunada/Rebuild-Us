using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class StatsManagerAmBannedPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    internal static void Postfix(out bool __result)
    {
        __result = false;
    }
}