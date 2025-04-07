using AmongUs.Data.Player;
using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class PlayerBanDataIsBannedPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    internal static void Postfix(out bool __result)
    {
        __result = false;
    }
}