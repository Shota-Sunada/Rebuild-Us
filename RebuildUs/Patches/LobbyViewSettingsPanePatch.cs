using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class LobbyViewSettingsPanePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
    internal static void UpdatePostfix(LobbyViewSettingsPane __instance)
    {
        if (CustomOption.CurrentLVSButtons.Count == 0)
        {
            CustomOption.GameModeChangedFlag = true;
            AwakePostfix(__instance);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.SetTab))]
    internal static bool SetTabPrefix(LobbyViewSettingsPane __instance)
    {
        if ((int)__instance.currentTab < 15)
        {
            CustomOption.ChangeTab(__instance, __instance.currentTab);
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.ChangeTab))]
    internal static void ChangeTabPostfix(LobbyViewSettingsPane __instance, StringNames category)
    {
        CustomOption.ChangeTab(__instance, category);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
    internal static void AwakePostfix(LobbyViewSettingsPane __instance)
    {
        CustomOption.CurrentLVSButtons.ForEach(x => x?.Destroy());
        CustomOption.CurrentLVSButtons.Clear();
        CustomOption.CurrentLVSButtonTypes.Clear();

        CustomOption.RemoveVanillaTabs(__instance);
        CustomOption.CreateSettingTabs(__instance);
    }
}