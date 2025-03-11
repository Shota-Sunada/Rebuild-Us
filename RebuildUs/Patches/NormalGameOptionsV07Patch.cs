using AmongUs.GameOptions;
using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class NormalGameOptionsV07Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NormalGameOptionsV07), nameof(NormalGameOptionsV07.AreInvalid))]
    internal static bool AreInvalidPrefix(NormalGameOptionsV07 __instance, ref int maxExpectedPlayers)
    {
        return CustomOption.RestrictOptions(__instance, ref maxExpectedPlayers);
    }
}