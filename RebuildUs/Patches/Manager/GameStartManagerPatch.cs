
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class GameStartManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static void StartPostfix(GameStartManager __instance)
    {
        LobbyExtensions.GameStartPostfix();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static void UpdatePrefix(GameStartManager __instance)
    {
        LobbyExtensions.LobbyUpdatePrefix(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static void UpdatePostfix(GameStartManager __instance)
    {
        LobbyExtensions.LobbyUpdatePostfix(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public static bool BeginGamePrefix(GameStartManager __instance)
    {
        return LobbyExtensions.IsBeginningGame();
    }
}