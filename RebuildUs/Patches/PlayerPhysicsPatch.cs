using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class PlayerPhysicsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    internal static void CoSpawnPlayerPostfix()
    {
        if (PlayerControl.LocalPlayer != null && AmongUsClient.Instance.AmHost)
        {
            GameManager.Instance.LogicOptions.SyncOptions();
            CustomOption.ShareOptionSelections();
        }
    }
}