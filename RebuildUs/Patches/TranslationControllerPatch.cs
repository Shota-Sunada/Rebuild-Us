using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
        return CustomOption.ReturnCustomString(ref __result, ref id);
    }
}