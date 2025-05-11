using AmongUs.Data;
using AmongUs.Data.Legacy;
using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class AccountManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.RandomizeName))]
    public static bool RandomizeNamePrefix(AccountManager __instance)
    {
        if (LegacySaveManager.lastPlayerName == null)
        {
            return true;
        }

        DataManager.Player.Customization.Name = LegacySaveManager.lastPlayerName;
        __instance.accountTab.UpdateNameDisplay();

        return false; // Don't execute original
    }
}