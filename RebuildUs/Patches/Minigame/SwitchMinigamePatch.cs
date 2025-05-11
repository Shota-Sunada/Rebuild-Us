using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class SwitchMinigamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
    public static void BeginPostfix(SwitchMinigame __instance)
    {
        // Block Swapper from fixing lights. One could also just delete the PlayerTask, but I wanted to do it the same way as with comms for now.
        if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
        {
            __instance.Close();
        }
    }
}