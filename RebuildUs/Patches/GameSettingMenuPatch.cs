using HarmonyLib;
using RebuildUs.Modules;
using RebuildUs.Helpers;
using AmongUs.GameOptions;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class GameSettingMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
    internal static void ChangeTabPostfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if (previewOnly)
        {
            return;
        }

        foreach (var tab in CustomOption.CurrentGOMTabs)
        {
            tab?.SetActive(false);
        }
        foreach (var button in CustomOption.CurrentGOMButtons)
        {
            button.SelectButton(false);
        }

        if (tabNum > 2)
        {
            tabNum -= 3;
            CustomOption.CurrentGOMTabs[tabNum].SetActive(true);
            CustomOption.CurrentGOMButtons[tabNum].SelectButton(true);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    internal static void StartPostfix(GameSettingMenu __instance)
    {
        CustomOption.CurrentGOMTabs.ForEach(x => x?.Destroy());
        CustomOption.CurrentGOMButtons.ForEach(x => x?.Destroy());
        CustomOption.CurrentGOMTabs = [];
        CustomOption.CurrentGOMButtons = [];
        CustomOption.CurrentGOMs.Clear();

        if (GameOptionsManager.Instance.currentGameOptions.GameMode is GameModes.HideNSeek)
        {
            return;
        }

        CustomOption.RemoveVanillaTabs(__instance);
        CustomOption.CreateSettingTabs(__instance);
    }
}