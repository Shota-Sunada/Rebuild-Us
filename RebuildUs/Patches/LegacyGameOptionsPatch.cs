using AmongUs.GameOptions;
using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class LegacyGameOptionsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LegacyGameOptions), nameof(LegacyGameOptions.AreInvalid))]
    internal static bool AreInvalidPrefix(LegacyGameOptions __instance, ref int maxExpectedPlayers)
    {
        return CustomOption.RestrictOptions(__instance, ref maxExpectedPlayers);
    }
}