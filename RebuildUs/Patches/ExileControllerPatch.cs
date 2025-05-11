using HarmonyLib;
using System.Linq;
using RebuildUs.Objects;
using System;
using UnityEngine;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

public static class ExileControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
    [HarmonyPriority(Priority.First)]
    public static void BeginForGameplayPrefix(ExileController __instance, [HarmonyArgument(0)] ref NetworkedPlayerInfo exiled)
    {
        ExileExtensions.ExileControllerBegin(ref exiled);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public static void WrapUpPostfix(ExileController __instance)
    {
        var networkedPlayer = __instance.initData.networkedPlayer;
        ExileExtensions.WrapUpPostfix((networkedPlayer != null) ? networkedPlayer.Object : null);
    }
}