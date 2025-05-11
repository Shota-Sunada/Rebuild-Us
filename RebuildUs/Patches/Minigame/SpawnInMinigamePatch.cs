using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class SpawnInMinigamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]
public    static void Postfix()
    {
        AntiTeleport.setPosition();
        Chameleon.lastMoved.Clear();
    }
}