using AmongUs.GameOptions;
using RebuildUs.Localization;
using RebuildUs.Utilities;

namespace RebuildUs.Modules;

internal partial class CustomOption
{
    internal static bool RestrictOptions(LegacyGameOptions __instance, ref int maxExpectedPlayers)
    {
        return __instance.MaxPlayers > maxExpectedPlayers
            || __instance.NumImpostors is < 1 or > 3
            || __instance.PlayerSpeedMod is 0f or > 3f
            || __instance.KillDistance < 0
            || __instance.KillDistance >= LegacyGameOptions.KillDistances.Count;
    }

    internal static void PreventOutOfRange(StringOption __instance)
    {
        if (__instance.Title is StringNames.GameKillDistance && __instance.Value is 3)
        {
            __instance.Value = 1;
            GameOptionsManager.Instance.currentNormalGameOptions.KillDistance = 1;
            GameManager.Instance.LogicOptions.SyncOptions();
        }
    }

    internal static void OverrideBaseOptionSelections(ref StringNames stringName, ref string value)
    {
        if (stringName is StringNames.GameKillDistance)
        {
            if (GameOptionsManager.Instance.currentGameMode is GameModes.Normal)
            {
                var index = GameOptionsManager.Instance.currentNormalGameOptions.KillDistance;
                value = LegacyGameOptions.KillDistanceStrings[index];
            }
            else
            {
                var index = GameOptionsManager.Instance.currentHideNSeekGameOptions.KillDistance;
                value = LegacyGameOptions.KillDistanceStrings[index];
            }
        }
    }

    internal static void AddKillDistance()
    {
        if (TranslationController.InstanceExists)
        {
            LegacyGameOptions.KillDistances = new([0.5f, 1f, 1.8f, 2.5f]);
            LegacyGameOptions.KillDistanceStrings = new([Tr.Get("KillRangeVeryShort"), FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingShort), FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingMedium), FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingLong)]);
        }
    }
}