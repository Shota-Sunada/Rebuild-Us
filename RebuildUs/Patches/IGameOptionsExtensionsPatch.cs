using AmongUs.GameOptions;
using HarmonyLib;
using RebuildUs.Modules;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class IGameOptionsExtensionsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.AppendItem), [typeof(Il2CppSystem.Text.StringBuilder), typeof(StringNames), typeof(string)])]
    internal static void AppendItemPrefix(ref StringNames stringName, ref string value)
    {
        CustomOption.OverrideBaseOptionSelections(ref stringName, ref value);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    internal static void GetAdjustedNumImpostorsPostfix(ref int __result)
    {
        // インポスターの人数を制限せず
        __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumImpostors), 1, 3);
    }
}