using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class GameOptionsMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
    public static void CreateSettingsPostfix(GameOptionsMenu __instance)
    {
        if (__instance.gameObject.name is "GAME SETTINGS TAB")
        {
            CustomOption.AdaptTaskCount(__instance);
        }
    }
}