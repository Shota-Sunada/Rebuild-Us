using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class HudManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static void StartPostfix(HudManager __instance)
    {
        Buttons.CreateButtons(__instance);
    }
}