using HarmonyLib;
using System;
using Hazel;
using UnityEngine;
using System.Linq;
using static RebuildUs.GameHistory;
using static RebuildUs.MapOptions;
using System.Collections.Generic;
using AmongUs.GameOptions;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class VentButtonVisibilityPatch
{
    static void Postfix(PlayerControl __instance)
    {
        if (__instance.AmOwner && __instance.roleCanUseVents() && FastDestroyableSingleton<HudManager>.Instance.ReportButton.isActiveAndEnabled)
        {
            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Show();
        }
    }
}

[HarmonyPatch]
class SurveillanceMinigamePatch2
{

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPlayerMaterialColors))]
    public static void Postfix(PlayerControl __instance, SpriteRenderer rend)
    {
        if (!MinigameExtensions.nightVisionIsActive) return;
        foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
        {
            foreach (SpriteRenderer component in new SpriteRenderer[2] { deadBody.bodyRenderers.FirstOrDefault(), deadBody.bloodSplatter })
            {
                component.material.SetColor("_BackColor", Palette.ShadowColors[11]);
                component.material.SetColor("_BodyColor", Palette.PlayerColors[11]);
            }
        }
    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
class ShowSabotageMapPatch
{
    static bool Prefix(MapBehaviour __instance)
    {
        if (PlayerControl.LocalPlayer.Data.IsDead && CustomOptionHolder.deadImpsBlockSabotage.getBool())
        {
            __instance.ShowNormalMap();
            return false;
        }
        return true;
    }
}