using HarmonyLib;
using AmongUs.GameOptions;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class GameOptionsDataPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.AreInvalid))]
    internal static bool AreInvalidPrefix(GameOptionsData __instance, ref int maxExpectedPlayers)
    {
        return CustomOption.RestrictOptions(__instance, ref maxExpectedPlayers);
    }
}