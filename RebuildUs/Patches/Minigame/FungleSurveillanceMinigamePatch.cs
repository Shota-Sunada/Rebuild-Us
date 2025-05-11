using HarmonyLib;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class FungleSurveillanceMinigamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Update))]
    public static void Postfix(FungleSurveillanceMinigame __instance)
    {
        MinigameExtensions.nightVisionUpdate(FungleCamMinigame: __instance);
    }
}