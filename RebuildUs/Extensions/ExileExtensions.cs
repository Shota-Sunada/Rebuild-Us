using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace RebuildUs.Extensions;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
[HarmonyPriority(Priority.First)]
class ExileControllerBeginPatch
{
    public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref NetworkedPlayerInfo exiled)
    {

    }
}

[HarmonyPatch]
class ExileControllerWrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            var networkedPlayer = __instance.initData.networkedPlayer;
            WrapUpPostfix(networkedPlayer?.Object);
        }
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    class AirshipExileControllerPatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            NetworkedPlayerInfo networkedPlayer = __instance.initData.networkedPlayer;
            WrapUpPostfix(networkedPlayer?.Object);
        }
    }

    // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) and SpawnInMinigame of submerged
    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), [typeof(GameObject)])]
    public static void Prefix(GameObject obj)
    {
        // Night vision:
        if (obj != null && obj.name != null && obj.name.Contains("FungleSecurity"))
        {
            // SurveillanceMinigamePatch.resetNightVision();
            return;
        }

        // submerged
        if (!SubmergedCompatibility.IsSubmerged) return;
        if (obj.name.Contains("ExileCutscene"))
        {
            WrapUpPostfix(obj.GetComponent<ExileController>().initData.networkedPlayer?.Object);
        }
        else if (obj.name.Contains("SpawnInMinigame"))
        {
        }
    }

    static void WrapUpPostfix(PlayerControl exiled)
    {
    }
}

[HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), [typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)])]
class ExileControllerMessagePatch
{
    static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
    {
        try
        {
            if (ExileController.Instance != null && ExileController.Instance.initData != null)
            {
                var player = ExileController.Instance.initData.networkedPlayer.Object;
                if (player == null) return;
                // Exile role text
                if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                {
                    __result = player.Data.PlayerName + " was The " + string.Join(" ", [.. RoleInfo.GetRoleInfoForPlayer(player).Select(x => x.Name)]);
                }
                // Hide number of remaining impostors on Jester win
                if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS)
                {
                    // if (Jester.jester != null && player.PlayerId == Jester.jester.PlayerId) __result = "";
                }
            }
        }
        catch
        {
            // pass - Hopefully prevent leaving while exiling to softlock game
        }
    }
}