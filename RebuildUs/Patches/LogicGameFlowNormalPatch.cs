using HarmonyLib;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class LogicGameFlowNormalPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static bool CheckEndCriteriaPrefix(ShipStatus __instance)
    {
        return EndGameExtensions.CheckEndCriteriaPrefix(__instance);
    }
}