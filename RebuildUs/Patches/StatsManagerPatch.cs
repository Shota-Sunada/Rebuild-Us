using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
internal static class StatsManagerAmBannedPatch
{
    internal static void Postfix(out bool __result)
    {
        __result = false;
    }
}