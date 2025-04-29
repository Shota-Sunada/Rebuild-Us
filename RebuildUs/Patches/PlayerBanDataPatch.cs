using AmongUs.Data.Player;
using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class PlayerBanDataIsBannedPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    public static void Postfix(out bool __result)
    {
        __result = false;
    }
}