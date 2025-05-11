using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RebuildUs.Extensions;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class SurveillanceMinigamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    public static void BeginPostfix(SurveillanceMinigame __instance)
    {
        // Add securityGuard cameras
        MinigameExtensions.page = 0;
        MinigameExtensions.timer = 0;
        if (MapUtilities.CachedShipStatus.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0)
        {
            __instance.textures = __instance.textures.ToList().Concat(new RenderTexture[MapUtilities.CachedShipStatus.AllCameras.Length - 4]).ToArray();
            for (int i = 4; i < MapUtilities.CachedShipStatus.AllCameras.Length; i++)
            {
                var surv = MapUtilities.CachedShipStatus.AllCameras[i];
                var camera = UnityEngine.Object.Instantiate(__instance.CameraPrefab);
                camera.transform.SetParent(__instance.transform);
                camera.transform.position = new Vector3(surv.transform.position.x, surv.transform.position.y, 8f);
                camera.orthographicSize = 2.35f;
                var temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
                __instance.textures[i] = temporary;
                camera.targetTexture = temporary;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
    public static bool UpdatePrefix(SurveillanceMinigame __instance)
    {
        // Update normal and securityGuard cameras
        MinigameExtensions.timer += Time.deltaTime;
        int numberOfPages = Mathf.CeilToInt(MapUtilities.CachedShipStatus.AllCameras.Length / 4f);

        bool update = false;

        if (MinigameExtensions.timer > 3f || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            update = true;
            MinigameExtensions.timer = 0f;
            MinigameExtensions.page = (MinigameExtensions.page + 1) % numberOfPages;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MinigameExtensions.page = (MinigameExtensions.page + numberOfPages - 1) % numberOfPages;
            update = true;
            MinigameExtensions.timer = 0f;
        }

        if ((__instance.isStatic || update) && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
        {
            __instance.isStatic = false;
            for (int i = 0; i < __instance.ViewPorts.Length; i++)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                __instance.SabText[i].gameObject.SetActive(false);
                if (MinigameExtensions.page * 4 + i < __instance.textures.Length)
                    __instance.ViewPorts[i].material.SetTexture("_MainTex", __instance.textures[MinigameExtensions.page * 4 + i]);
                else
                    __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
            }
        }
        else if (!__instance.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
        {
            __instance.isStatic = true;
            for (int j = 0; j < __instance.ViewPorts.Length; j++)
            {
                __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                __instance.SabText[j].gameObject.SetActive(true);
            }
        }

        MinigameExtensions.nightVisionUpdate(SkeldCamsMinigame: __instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.OnDestroy))]
    public static void OnDestroyPrefix()
    {
        MinigameExtensions.resetNightVision();
    }
}