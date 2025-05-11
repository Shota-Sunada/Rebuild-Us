using HarmonyLib;
using RebuildUs.Modules;
using TMPro;
using UnityEngine;

namespace RebuildUs.Patches;

public static class MainMenuManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static void StartPostfix(MainMenuManager __instance)
    {
        ClientOptions.MainMenuManagerStart();
    }
}