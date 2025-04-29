using HarmonyLib;
using RebuildUs.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class LogicGameFlowNormalPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static void CheckEndCriteriaPrefix(ShipStatus __instance)
    {
        EndGameExtensions.CheckEndCriteriaPrefix(__instance);
    }
}