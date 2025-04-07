using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using RebuildUs.Localization;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class TranslationControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), [typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)])]
    [HarmonyPriority(Priority.Last)]
    internal static bool GetStringPrefix(ref string __result, ref StringNames id)
    {
        if (id is CustomOption.KILL_RANGE_VERY_SHORT)
        {
            __result = Tr.Get("KillRangeVeryShort");
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize))]
    internal static void InitializePostfix(TranslationController __instance)
    {
        CustomOption.AddKillDistance();
    }
}