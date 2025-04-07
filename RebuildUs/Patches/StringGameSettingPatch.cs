using AmongUs.GameOptions;
using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class StringGameSettingPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringGameSetting), nameof(StringGameSetting.GetValueString))]
    internal static bool GetValueStringPrefix(StringGameSetting __instance, float value, ref string __result)
    {
        switch (__instance.OptionName)
        {
            case Int32OptionNames.KillDistance:
                __result = LegacyGameOptions.KillDistanceStrings[(int)value];
                return false;
            default:
                return true;
        }
    }
}