using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class MedScanMinigamePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
    public static void FixedUpdatePrefix(MedScanMinigame __instance)
    {
        if (MapOptions.allowParallelMedBayScans)
        {
            __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
            __instance.medscan.UsersList.Clear();
        }
    }
}