using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class StringOptionPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    internal static void InitializePrefix(StringOption __instance)
    {
        CustomOption.PreventOutOfRange(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    internal static void InitializePostfix(StringOption __instance)
    {
        CustomOption.OverrideBaseOptions(__instance);
    }
}