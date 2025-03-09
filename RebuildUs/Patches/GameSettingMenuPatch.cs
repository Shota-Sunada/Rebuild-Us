using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
internal static class GameSettingMenuStartPatch
{
    internal static void Postfix(GameSettingMenu __instance)
    {
        CustomOption.DestroyVanillaObjects(__instance);
    }
}