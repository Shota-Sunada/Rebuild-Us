using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class TuneRadioMinigamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TuneRadioMinigame), nameof(TuneRadioMinigame.Begin))]
    public static void BeginPostfix(TuneRadioMinigame __instance)
    {
        // Block Swapper from fixing comms. Still looking for a better way to do this, but deleting the task doesn't seem like a viable option since then the camera, admin table, ... work while comms are out
        if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
        {
            __instance.Close();
        }
    }
}