using HarmonyLib;
using AmongUs.GameOptions;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class GameOptionsDataPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LegacyGameOptions), nameof(LegacyGameOptions.AreInvalid))]
    internal static bool AreInvalidPrefix(LegacyGameOptions __instance, ref int maxExpectedPlayers)
    {
        return CustomOption.RestrictOptions(__instance, ref maxExpectedPlayers);
    }
}