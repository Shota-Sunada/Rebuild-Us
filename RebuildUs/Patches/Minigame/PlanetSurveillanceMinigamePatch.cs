using HarmonyLib;
using RebuildUs.Extensions;
using UnityEngine;

namespace RebuildUs.Patches;

public static class PlanetSurveillanceMinigamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
    public static void UpdatePostfix(PlanetSurveillanceMinigame __instance)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            __instance.NextCamera(1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            __instance.NextCamera(-1);
        }

        MinigameExtensions.nightVisionUpdate(SwitchCamsMinigame: __instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.OnDestroy))]
    public static void OnDestroyPrefix()
    {
        MinigameExtensions.resetNightVision();
    }
}