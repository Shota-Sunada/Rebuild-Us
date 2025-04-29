using AmongUs.GameOptions;
using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class LegacyGameOptionsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LegacyGameOptions), nameof(LegacyGameOptions.AreInvalid))]
    public static bool AreInvalidPrefix(LegacyGameOptions __instance, ref int maxExpectedPlayers)
    {
        return CustomOption.RestrictOptions(__instance, ref maxExpectedPlayers);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LegacyGameOptions), nameof(LegacyGameOptions.Validate))]
    public static void ValiDatePostfix(LegacyGameOptions __instance)
    {
        __instance.SetInt(Int32OptionNames.NumImpostors, GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumImpostors));
    }
}