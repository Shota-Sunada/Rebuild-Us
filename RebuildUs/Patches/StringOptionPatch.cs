using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class StringOptionPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    internal static bool InitializePrefix(StringOption __instance)
    {
        CustomOption.PreventOutOfRange(__instance);

        return CustomOption.EnableStringOption(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    internal static void InitializePostfix(StringOption __instance)
    {
        CustomOption.OverrideBaseOptions(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    internal static bool IncreasePrefix(StringOption __instance)
    {
        return CustomOption.IncreaseStringOption(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    internal static bool DecreasePrefix(StringOption __instance)
    {
        return CustomOption.DecreaseStringOption(__instance);
    }
}