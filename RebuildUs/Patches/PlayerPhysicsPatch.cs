using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Patches;

public static class PlayerPhysicsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.Awake))]
    public static void AwakePostfix(PlayerPhysics __instance)
    {
        if (!__instance.body) return;
        __instance.body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
}