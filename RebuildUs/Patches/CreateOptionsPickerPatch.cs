using AmongUs.GameOptions;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(CreateOptionsPicker))]
class CreateOptionsPickerPatch
{
    private static List<SpriteRenderer> renderers = [];

    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.SetGameMode))]
    public static bool Prefix(CreateOptionsPicker __instance, ref GameModes mode)
    {
        if (mode <= GameModes.HideNSeek)
        {
            MapOptions.gameMode = CustomGamemodes.Classic;
            return true;
        }

        __instance.SetGameMode(GameModes.Normal);
        CustomGamemodes gm = (CustomGamemodes)((int)mode - 2);
        return false;
    }


    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Refresh))]
    public static void Postfix(CreateOptionsPicker __instance)
    {
    }
}

[HarmonyPatch(typeof(GameModeMenu))]
class GameModeMenuPatch
{
    [HarmonyPatch(typeof(GameModeMenu), nameof(GameModeMenu.OnEnable))]
    public static bool Prefix(GameModeMenu __instance)
    {
        uint gameMode = (uint)__instance.Parent.GetTargetOptions().GameMode;
        float num = ((float)Mathf.CeilToInt(4f / 10f) / 2f - 0.5f) * -2.5f;   // 4 for 4 buttons!
        __instance.controllerSelectable.Clear();
        int num2 = 0;
        __instance.ButtonPool.poolSize = 5;
        for (int i = 0; i <= 5; i++)
        {
            GameModes entry = (GameModes)i;
            if (entry != GameModes.None)
            {
                ChatLanguageButton chatLanguageButton = __instance.ButtonPool.Get<ChatLanguageButton>();
                chatLanguageButton.transform.localPosition = new Vector3(num + (float)(num2 / 10) * 2.5f, 2f - (float)(num2 % 10) * 0.5f, 0f);
                if (i <= 2)
                {
                    chatLanguageButton.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(GameModesHelpers.ModeToName[entry], new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                }

                chatLanguageButton.Button.OnClick.RemoveAllListeners();
                chatLanguageButton.Button.OnClick.AddListener((System.Action)delegate
                {
                    __instance.ChooseOption(entry);
                });

                bool isCurrentMode = i <= 2 && MapOptions.gameMode == CustomGamemodes.Classic && (long)entry == (long)(ulong)gameMode;
                chatLanguageButton.SetSelected(isCurrentMode);
                __instance.controllerSelectable.Add(chatLanguageButton.Button);
                if (isCurrentMode)
                {
                    __instance.defaultButtonSelected = chatLanguageButton.Button;
                }
                num2++;
            }
        }
        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.defaultButtonSelected, __instance.controllerSelectable, false);
        return false;
    }
}