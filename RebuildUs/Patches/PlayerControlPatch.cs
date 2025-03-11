using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class PlayerControlPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    internal static void RpcSyncSettingsPostfix()
    {
        // CustomOption.ShareOptionSelections();
        // CustomOption.SaveVanillaOptions();
    }
}