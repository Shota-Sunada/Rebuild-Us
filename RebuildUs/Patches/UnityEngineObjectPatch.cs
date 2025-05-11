using HarmonyLib;
using RebuildUs.Extensions;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class UnityEngineObjectPatch
{
    // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) and SpawnInMinigame of submerged
    [HarmonyPatch(typeof(Object), nameof(Object.Destroy), [typeof(GameObject)])]
    public static void Prefix(GameObject obj)
    {
        // night vision:
        if (obj != null && obj.name != null && obj.name.Contains("FungleSecurity"))
        {
            MinigameExtensions.resetNightVision();
            return;
        }

        // submerged
        if (!SubmergedCompatibility.IsSubmerged) return;
        if (obj.name.Contains("ExileCutscene"))
        {
            ExileExtensions.WrapUpPostfix(obj.GetComponent<ExileController>().initData.networkedPlayer?.Object);
        }
        else if (obj.name.Contains("SpawnInMinigame"))
        {
            AntiTeleport.setPosition();
            Chameleon.lastMoved.Clear();
        }
    }
}