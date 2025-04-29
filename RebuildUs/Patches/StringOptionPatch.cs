using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class StringOptionPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public static bool InitializePrefix(StringOption __instance)
    {
        CustomOption.PreventOutOfRange(__instance);

        return CustomOption.EnableStringOption(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public static void InitializePostfix(StringOption __instance)
    {
        if (__instance.Title is StringNames.GameKillDistance && __instance.Values.Count is 3)
        {
            __instance.Values = new([CustomOption.KILL_RANGE_VERY_SHORT, StringNames.SettingShort, StringNames.SettingMedium, StringNames.SettingLong]);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public static bool IncreasePrefix(StringOption __instance)
    {
        return CustomOption.IncreaseStringOption(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public static bool DecreasePrefix(StringOption __instance)
    {
        return CustomOption.DecreaseStringOption(__instance);
    }
}