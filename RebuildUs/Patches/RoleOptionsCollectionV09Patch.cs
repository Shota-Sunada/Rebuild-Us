using AmongUs.GameOptions;
using HarmonyLib;

namespace RebuildUs.Patches;

internal static class RoleOptionsCollectionV09Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoleOptionsCollectionV09), nameof(RoleOptionsCollectionV09.GetNumPerGame))]
    internal static void GetNumPerGamePostfix(ref int __result)
    {
        // Mod役職有効時にバニラ役職無効化
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
        {
            __result = 0;
        }
    }
}