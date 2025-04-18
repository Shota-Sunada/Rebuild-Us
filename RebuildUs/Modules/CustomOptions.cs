using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;
using TMPro;
using BepInEx.Configuration;
using RebuildUs.Localization;

namespace RebuildUs.Modules;

internal partial class CustomOption
{
    internal const int ROLE_OVERVIEW_ID = 99;
    internal const StringNames KILL_RANGE_VERY_SHORT = (StringNames)49999;

    internal static Dictionary<int, CustomOption> AllOptions = [];
    internal static ConfigEntry<string> VanillaSettings;
    internal static int Preset = 0;

    internal string TitleKey;
    internal Color? TitleColor;
    internal object[] Selections;
    internal OptionBehaviour OptionBehaviour;
    internal ConfigEntry<int> Entry;

    internal int DefaultIndex;
    internal int SelectedIndex;
    internal CustomOption Parent;
    internal bool IsHeader;
    internal CustomOptionType Type;
    internal string HeaderKey;
    internal Color? HeaderColor;

    internal CustomOption(int id, CustomOptionType type, (string key, Color? color) title, object[] selections, object defaultValue, CustomOption parent, bool isHeader, (string Key, Color? color)? header)
    {
        Type = type;
        TitleKey = title.key;
        TitleColor = title.color;
        Selections = selections;
        var index = Array.IndexOf(selections, defaultValue);
        DefaultIndex = index >= 0 ? index : 0;
        Parent = parent;
        IsHeader = isHeader;
        HeaderKey = header?.Key ?? "";
        HeaderColor = header?.color ?? null;

        SelectedIndex = 0;

        if (AllOptions.ContainsKey(id))
        {
            RebuildUsPlugin.Instance.Logger.LogWarning($"The custom option id ({id}) is already exists!");
            RebuildUsPlugin.Instance.Logger.LogWarning("The old one will be replaced by the new!");
        }

        AllOptions[id] = this;
    }

    internal static CustomOption Create(
        int id,
        CustomOptionType type,
        (string key, Color? color) title,
        string[] selections,
        CustomOption parent = null,
        bool isHeader = false,
        (string key, Color? color)? header = null)
    {
        return new(id, type, title, selections, "", parent, isHeader, header);
    }

    internal static CustomOption Create(
        int id,
        CustomOptionType type,
        (string key, Color? color) title,
        float defaultValue,
        float min,
        float max,
        float interval,
        CustomOption parent = null,
        bool isHeader = false,
        (string key, Color? color)? header = null)
    {
        var selections = new List<object>();
        for (float i = min; i <= max; i += interval)
        {
            selections.Add(i);
        }
        return new(id, type, title, [.. selections], defaultValue, parent, isHeader, header);
    }

    internal static CustomOption Create(
        int id,
        CustomOptionType type,
        (string key, Color? color) title,
        bool defaultValue,
        CustomOption parent = null,
        bool isHeader = false,
        (string key, Color? color)? header = null)
    {
        return new(id, type, title, [Tr.Get("OptionOff"), Tr.Get("OptionOn")], defaultValue ? Tr.Get("OptionOn") : Tr.Get("OptionOff"), parent, isHeader, header);
    }

    internal static void SwitchPreset(int newPreset)
    {
        SaveVanillaOptions();
        Preset = newPreset;
        VanillaSettings = RebuildUsPlugin.Instance.Config.Bind($"Preset{Preset}", "GameOptions", "");
        LoadVanillaOptions();
        foreach (var (id, option) in AllOptions)
        {
            if (id == 0) continue;

            option.Entry = RebuildUsPlugin.Instance.Config.Bind($"Preset{Preset}", id.ToString(), option.DefaultIndex);
            option.SelectedIndex = Mathf.Clamp(option.Entry.Value, 0, option.Selections.Length - 1);
            if (option.OptionBehaviour != null && option.OptionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = option.SelectedIndex;
                stringOption.ValueText.text = option.Selections[option.SelectedIndex].ToString();
            }
        }

        // make sure to reload all tabs, even the ones in the background, because they might have changed when the preset was switched!
        if (AmongUsClient.Instance.AmHost)
        {
            foreach (var entry in CurrentGOMs)
            {
                var optionType = (CustomOptionType)entry.Key;
                var gom = entry.Value;
                if (gom is not null)
                {
                    UpdateGameOptionsMenu(optionType, gom);
                }
            }
        }
    }

    internal static void SaveVanillaOptions()
    {
        VanillaSettings.Value = Convert.ToBase64String(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameManager.Instance.LogicOptions.currentGameOptions, false));
    }

    internal static bool LoadVanillaOptions()
    {
        var optionsString = VanillaSettings.Value;
        if (optionsString == "")
        {
            return false;
        }

        var gameOptions = GameOptionsManager.Instance.gameOptionsFactory.FromBytes(Convert.FromBase64String(optionsString));
        if (gameOptions.Version < 8)
        {
            RebuildUsPlugin.Instance.Logger.LogMessage("tried to paste old settings, not doing this!");
            return false;
        }

        GameOptionsManager.Instance.GameHostOptions = gameOptions;
        GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.GameHostOptions;
        GameManager.Instance.LogicOptions.SetGameOptions(GameOptionsManager.Instance.CurrentGameOptions);
        GameManager.Instance.LogicOptions.SyncOptions();

        return true;
    }

    internal static void ShareOptionChange(CustomOption option)
    {
        if (option is null)
        {
            return;
        }

        var id = AllOptions.FirstOrDefault(x => x.Value == option).Key;

        using var rpc = new RPCSender(PlayerControl.LocalPlayer.NetId, CustomRPC.ShareOptions);
        rpc.Write((byte)1);
        rpc.Write((uint)id, true);
        rpc.Write(Convert.ToUInt32(option.SelectedIndex), true);
    }

    internal static void ShareOptionSelections()
    {
        if (PlayerControl.AllPlayerControls.Count <= 1 ||
            !AmongUsClient.Instance.AmHost &&
            PlayerControl.LocalPlayer == null)
        {
            return;
        }

        var optionsList = new Dictionary<int, CustomOption>(AllOptions);
        while (optionsList.Count != 0)
        {
            // takes less than 3 bytes per option on average
            var amount = (byte)Math.Min(optionsList.Count, 200);
            using var rpc = new RPCSender(PlayerControl.LocalPlayer.NetId, CustomRPC.ShareOptions);
            rpc.Write(amount);
            for (int i = 0; i < amount; i++)
            {
                var option = optionsList.ElementAt(0);
                rpc.Write((uint)option.Key, true);
                rpc.Write(Convert.ToUInt32(option.Value.SelectedIndex), true);
                optionsList.Remove(option.Key);
            }
        }
    }

    internal int GetSelection()
    {
        return SelectedIndex;
    }

    internal bool GetBool()
    {
        return SelectedIndex > 0;
    }

    internal float GetFloat()
    {
        return (float)Selections[SelectedIndex];
    }

    internal int GetQuantity()
    {
        return SelectedIndex + 1;
    }

    internal void UpdateSelection(int newSelection, bool notifyUsers = true)
    {
        newSelection = Mathf.Clamp((newSelection + Selections.Length) % Selections.Length, 0, Selections.Length - 1);
        if (AmongUsClient.Instance.AmClient && notifyUsers && SelectedIndex != newSelection)
        {
            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage((StringNames)6000, Selections[newSelection].ToString(), false);
            try
            {
                SelectedIndex = newSelection;
                if (GameStartManager.Instance != null &&
                    GameStartManager.Instance.LobbyInfoPane != null &&
                    GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane != null &&
                    GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.activeSelf)
                {
                    ChangeTab(GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane, GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.currentTab);
                }
            }
            catch { }
        }
        SelectedIndex = newSelection;

        if (OptionBehaviour != null && OptionBehaviour is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = SelectedIndex;
            stringOption.ValueText.text = Selections[SelectedIndex].ToString();
            if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer)
            {
                if (AllOptions[0] == this && SelectedIndex != Preset)
                {
                    // Switch presets
                    SwitchPreset(SelectedIndex);
                    ShareOptionSelections();
                }
                else if (Entry != null)
                {
                    // Save selection to config
                    Entry.Value = SelectedIndex;
                    // Share single selection
                    ShareOptionChange(this);
                }
            }
        }
        else if (AllOptions[0] == this && AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer)
        {
            // Share the preset switch for random maps, even if the menu isn't open!
            SwitchPreset(SelectedIndex);
            // Share all selections
            ShareOptionSelections();
        }

        if (AmongUsClient.Instance.AmHost)
        {
            var currentTab = CurrentGOMTabs.FirstOrDefault(x => x.active).GetComponent<GameOptionsMenu>();
            if (currentTab is not null)
            {
                var optionType = AllOptions.Values.FirstOrDefault(x => x.OptionBehaviour == currentTab.Children[0]).Type;
                UpdateGameOptionsMenu(optionType, currentTab);
            }
        }
    }

    internal static void ChangeTab(LobbyViewSettingsPane __instance, StringNames category)
    {
        int tabNum = (int)category;

        foreach (var button in CurrentLVSButtons)
        {
            button.SelectButton(false);
        }

        // StringNames are in the range of 3000+
        if (tabNum > 20)
        {
            return;
        }
        __instance.taskTabButton.SelectButton(false);

        if (tabNum > 2)
        {
            tabNum -= 3;
            //GameOptionsMenuStartPatch.currentTabs[tabNum].SetActive(true);
            CurrentLVSButtons[tabNum].SelectButton(true);
            DrawTab(__instance, CurrentLVSButtonTypes[tabNum]);
        }
    }

    internal static List<PassiveButton> CurrentLVSButtons = [];
    internal static List<CustomOptionType> CurrentLVSButtonTypes = [];
    internal static bool GameModeChangedFlag = false;

    internal static void CreateCustomButton(LobbyViewSettingsPane __instance, int targetMenu, string buttonName, string buttonText, CustomOptionType optionType)
    {
        buttonName = "View" + buttonName;
        var buttonTemplate = GameObject.Find("OverviewTab");
        var modSettingsButton = GameObject.Find(buttonName);
        if (modSettingsButton is null)
        {
            modSettingsButton = UnityEngine.Object.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
            modSettingsButton.transform.localPosition += Vector3.right * 1.75f * (targetMenu - 2);
            modSettingsButton.name = buttonName;
            __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { modSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));
            var modSettingsPassiveButton = modSettingsButton.GetComponent<PassiveButton>();
            modSettingsPassiveButton.OnClick.RemoveAllListeners();
            modSettingsPassiveButton.OnClick.AddListener((Action)(() =>
            {
                __instance.ChangeTab((StringNames)targetMenu);
            }));
            modSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
            modSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
            modSettingsPassiveButton.SelectButton(false);
            CurrentLVSButtons.Add(modSettingsPassiveButton);
            CurrentLVSButtonTypes.Add(optionType);
        }
    }

    internal static void RemoveVanillaTabs(LobbyViewSettingsPane __instance)
    {
        GameObject.Find("RolesTabs")?.Destroy();
        var overview = GameObject.Find("OverviewTab");
        if (!GameModeChangedFlag)
        {
            overview.transform.localScale = new(0.5f * overview.transform.localScale.x, overview.transform.localScale.y, overview.transform.localScale.z);
            overview.transform.localPosition += new Vector3(-1.2f, 0f, 0f);

        }
        overview.transform.Find("FontPlacer").transform.localScale = new(1.35f, 1f, 1f);
        overview.transform.Find("FontPlacer").transform.localPosition = new(-0.6f, -0.1f, 0f);
        GameModeChangedFlag = false;
    }

    internal static void DrawTab(LobbyViewSettingsPane __instance, CustomOptionType optionType)
    {
        var relevantOptions = AllOptions.Values.Where(x => x.Type == optionType || optionType == CustomOptionType.General).ToList();

        if ((int)optionType is ROLE_OVERVIEW_ID)
        {
            // Create 4 Groups with Role settings only
            relevantOptions.Clear();
            relevantOptions.AddRange(AllOptions.Values.Where(x => x.Type == CustomOptionType.Impostor && x.IsHeader));
            relevantOptions.AddRange(AllOptions.Values.Where(x => x.Type == CustomOptionType.Neutral && x.IsHeader));
            relevantOptions.AddRange(AllOptions.Values.Where(x => x.Type == CustomOptionType.Crewmate && x.IsHeader));
            relevantOptions.AddRange(AllOptions.Values.Where(x => x.Type == CustomOptionType.Modifier && x.IsHeader));

            foreach (var (id, option) in AllOptions)
            {
                if (option.Parent != null && option.Parent.GetSelection() > 0)
                {
                    // ここに役職概要ページに追加で載せる設定を登録
                }
            }
        }

        for (int j = 0; j < __instance.settingsInfo.Count; j++)
        {
            __instance.settingsInfo[j].gameObject.Destroy();
        }
        __instance.settingsInfo.Clear();

        var num = 1.44f;
        var i = 0;
        var singles = 1;
        var headers = 0;
        var lines = 0;
        var curType = CustomOptionType.General;
        var numBonus = 0;

        foreach (var option in relevantOptions)
        {
            if (option.IsHeader && (int)optionType != ROLE_OVERVIEW_ID || (int)optionType == ROLE_OVERVIEW_ID && curType != option.Type)
            {
                curType = option.Type;
                if (i != 0)
                {
                    num -= 0.85f;
                    numBonus++;
                }

                if (i % 2 != 0)
                {
                    singles++;
                }
                // for header
                headers++;

                var categoryHeaderMasked = UnityEngine.Object.Instantiate(__instance.categoryHeaderOrigin);
                categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 61);
                categoryHeaderMasked.Title.text = option.HeaderKey != "" ?
                    option.HeaderColor == null ? Tr.Get(option.HeaderKey) : Helpers.Cs(Tr.Get(option.HeaderKey), (Color)option.HeaderColor) :
                    option.TitleColor == null ? Tr.Get(option.TitleKey) : Helpers.Cs(Tr.Get(option.TitleKey), (Color)option.TitleColor);

                if ((int)optionType is ROLE_OVERVIEW_ID)
                {
                    categoryHeaderMasked.Title.text = new Dictionary<CustomOptionType, string>()
                    {
                        { CustomOptionType.Impostor, Tr.Get("CategoryHeaderCrewmateRoles") },
                        { CustomOptionType.Neutral, Tr.Get("CategoryHeaderNeutralRoles") },
                        { CustomOptionType.Crewmate, Tr.Get("CategoryHeaderImpostorRoles") },
                        { CustomOptionType.Modifier, Tr.Get("CategoryHeaderModifier")}
                    }[curType];
                }
                categoryHeaderMasked.Title.outlineColor = Color.white;
                categoryHeaderMasked.Title.outlineWidth = 0.2f;
                categoryHeaderMasked.transform.SetParent(__instance.settingsContainer);
                categoryHeaderMasked.transform.localScale = Vector3.one;
                categoryHeaderMasked.transform.localPosition = new(-9.77f, num, -2f);
                __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
                num -= 1.05f;
                i = 0;
            }
            // Hides options, for which the parent is disabled!
            else if (option.Parent != null && (option.Parent.SelectedIndex == 0 || option.Parent.Parent != null && option.Parent.Parent.SelectedIndex == 0))
            {
                continue;
            }

            if (option == CustomOptionHolders.CrewmateRolesCountMax ||
                option == CustomOptionHolders.NeutralRolesCountMax ||
                option == CustomOptionHolders.ImpostorRolesCountMax ||
                option == CustomOptionHolders.ModifiersCountMax ||
                option == CustomOptionHolders.CrewmateRolesFill)
            {
                continue;
            }

            var viewSettingsInfoPanel = UnityEngine.Object.Instantiate(__instance.infoPanelOrigin);
            viewSettingsInfoPanel.transform.SetParent(__instance.settingsContainer);
            viewSettingsInfoPanel.transform.localScale = Vector3.one;

            float num2;
            if (i % 2 == 0)
            {
                lines++;
                num2 = -8.95f;
                if (i > 0)
                {
                    num -= 0.85f;
                }
            }
            else
            {
                num2 = -3f;
            }

            viewSettingsInfoPanel.transform.localPosition = new(num2, num, -2f);
            var value = option.GetSelection();
            var settingTuple = HandleSpecialOptionsView(option, option.TitleKey, option.Selections[value].ToString());
            viewSettingsInfoPanel.SetInfo(StringNames.ImpostorsCategory, settingTuple.Item2, 61);
            viewSettingsInfoPanel.titleText.text = settingTuple.Item1;

            if (option.IsHeader &&
                option.HeaderKey == "" &&
                (int)optionType is not ROLE_OVERVIEW_ID &&
                (option.Type is CustomOptionType.Neutral or CustomOptionType.Crewmate or CustomOptionType.Impostor or CustomOptionType.Modifier))
            {
                viewSettingsInfoPanel.titleText.text = "Spawn Chance";
            }

            if ((int)optionType is ROLE_OVERVIEW_ID)
            {
                viewSettingsInfoPanel.titleText.outlineColor = Color.white;
                viewSettingsInfoPanel.titleText.outlineWidth = 0.2f;
                // if (option.Type == CustomOptionType.Modifier)
                //     viewSettingsInfoPanel.settingText.text = viewSettingsInfoPanel.settingText.text + GameOptionsDataPatch.buildModifierExtras(option);
            }
            __instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

            i++;
        }

        var actual_spacing = (headers * 1.05f + lines * 0.85f) / (headers + lines) * 1.01f;
        __instance.scrollBar.CalculateAndSetYBounds(__instance.settingsInfo.Count + singles * 2 + headers, 2f, 5f, actual_spacing);
    }

    private static (string, string) HandleSpecialOptionsView(CustomOption option, string defaultString, string defaultVal)
    {
        var name = defaultString;
        var val = defaultVal;

        if (option == CustomOptionHolders.CrewmateRolesCountMin)
        {
            val = "";
            name = Tr.Get("CategoryHeaderCrewmateRoles");
            var min = CustomOptionHolders.CrewmateRolesCountMin.GetSelection();
            var max = CustomOptionHolders.CrewmateRolesCountMax.GetSelection();

            if (CustomOptionHolders.CrewmateRolesFill.GetBool())
            {
                var crewCount = PlayerControl.AllPlayerControls.Count - GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                var minNeutral = CustomOptionHolders.NeutralRolesCountMin.GetSelection();
                var maxNeutral = CustomOptionHolders.NeutralRolesCountMax.GetSelection();

                if (minNeutral > maxNeutral)
                {
                    minNeutral = maxNeutral;
                }
                min = crewCount - maxNeutral;
                max = crewCount - minNeutral;

                if (min < 0)
                {
                    min = 0;
                }
                if (max < 0)
                {
                    max = 0;
                }
                val = "Fill: ";
            }
            if (min > max)
            {
                min = max;
            }
            val += (min == max) ? $"{max}" : $"{min} - {max}";
        }

        if (option == CustomOptionHolders.NeutralRolesCountMin)
        {
            name = Tr.Get("CategoryHeaderNeutralRoles");
            var min = CustomOptionHolders.NeutralRolesCountMin.GetSelection();
            var max = CustomOptionHolders.NeutralRolesCountMax.GetSelection();
            if (min > max)
            {
                min = max;
            }
            val = (min == max) ? $"{max}" : $"{min} - {max}";
        }

        if (option == CustomOptionHolders.ImpostorRolesCountMin)
        {
            name = Tr.Get("CategoryHeaderImpostorRoles");
            var min = CustomOptionHolders.ImpostorRolesCountMin.GetSelection();
            var max = CustomOptionHolders.ImpostorRolesCountMax.GetSelection();
            if (max > GameOptionsManager.Instance.currentGameOptions.NumImpostors)
            {
                max = GameOptionsManager.Instance.currentGameOptions.NumImpostors;
            }
            if (min > max)
            {
                min = max;
            }
            val = (min == max) ? $"{max}" : $"{min} - {max}";
        }

        if (option == CustomOptionHolders.ModifiersCountMin)
        {
            name = Tr.Get("CategoryHeaderModifier");
            var min = CustomOptionHolders.ModifiersCountMin.GetSelection();
            var max = CustomOptionHolders.ModifiersCountMax.GetSelection();
            if (min > max)
            {
                min = max;
            }
            val = (min == max) ? $"{max}" : $"{min} - {max}";
        }

        return (name, val);
    }

    internal static void CreateSettingTabs(LobbyViewSettingsPane __instance)
    {
        // Handle different gamemodes and tabs needed therein.
        int next = 3;
        if (MapOptions.GameMode is CustomGameMode.Classic)
        {
            // create RU settings
            CreateCustomButton(__instance, next++, "RUSettings", Tr.Get("CategoryHeaderModSettings"), CustomOptionType.General);
            // create RU settings
            CreateCustomButton(__instance, next++, "RoleOverview", Tr.Get("CategoryHeaderRoleOverview"), (CustomOptionType)ROLE_OVERVIEW_ID);
            // Imp
            CreateCustomButton(__instance, next++, "ImpostorSettings", Tr.Get("CategoryHeaderImpostorRoles"), CustomOptionType.Impostor);
            // Neutral
            CreateCustomButton(__instance, next++, "NeutralSettings", Tr.Get("CategoryHeaderNeutralRoles"), CustomOptionType.Neutral);
            // Crew
            CreateCustomButton(__instance, next++, "CrewmateSettings", Tr.Get("CategoryHeaderCrewmateRoles"), CustomOptionType.Crewmate);
            // Modifier
            CreateCustomButton(__instance, next++, "ModifierSettings", Tr.Get("CategoryHeaderModifier"), CustomOptionType.Modifier);
        }
        // else if (MapOptions.GameMode is CustomGameMode.HideNSeek)
        // {
        //     // create Main HNS settings
        //     createCustomButton(__instance, next++, "HideNSeekMain", "Hide 'N' Seek", CustomOptionType.HideNSeekMain);
        //     // create HNS Role settings
        //     createCustomButton(__instance, next++, "HideNSeekRoles", "Hide 'N' Seek Roles", CustomOptionType.HideNSeekRoles);
        // }
    }

    internal static void AdaptTaskCount(GameOptionsMenu __instance)
    {
        // Adapt task count for main options
        var commonTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumCommonTasks).Cast<NumberOption>();
        if (commonTasksOption != null)
        {
            commonTasksOption.ValidRange = new(0f, 4f);
        }
        var shortTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumShortTasks).TryCast<NumberOption>();
        if (shortTasksOption != null)
        {
            shortTasksOption.ValidRange = new(0f, 23f);
        }
        var longTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumLongTasks).TryCast<NumberOption>();
        if (longTasksOption != null)
        {
            longTasksOption.ValidRange = new(0f, 15f);
        }
    }

    internal static List<GameObject> CurrentGOMTabs = [];
    internal static List<PassiveButton> CurrentGOMButtons = [];
    internal static Dictionary<byte, GameOptionsMenu> CurrentGOMs = [];

    private static void CreateSettings(GameOptionsMenu menu, List<CustomOption> options)
    {
        var num = 1.5f;
        foreach (var option in options)
        {
            if (option.IsHeader)
            {
                var categoryHeaderMasked = UnityEngine.Object.Instantiate(menu.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
                categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 20);
                categoryHeaderMasked.Title.text = option.HeaderKey != "" ?
                    option.HeaderColor == null ? Tr.Get(option.HeaderKey) : Helpers.Cs(Tr.Get(option.HeaderKey), (Color)option.HeaderColor) :
                    option.TitleColor == null ? Tr.Get(option.TitleKey) : Helpers.Cs(Tr.Get(option.TitleKey), (Color)option.TitleColor);
                categoryHeaderMasked.Title.outlineColor = Color.white;
                categoryHeaderMasked.Title.outlineWidth = 0.2f;
                categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                categoryHeaderMasked.transform.localPosition = new(-0.903f, num, -2f);
                num -= 0.63f;
            }
            // Hides options, for which the parent is disabled!
            else if (option.Parent != null && (option.Parent.SelectedIndex == 0 || option.Parent.Parent != null && option.Parent.Parent.SelectedIndex == 0))
            {
                continue;
            }
            else if (option.Parent != null && option.Parent.SelectedIndex != 0)
            {
                continue;
            }

            var optionBehaviour = UnityEngine.Object.Instantiate(menu.stringOptionOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
            optionBehaviour.transform.localPosition = new(0.952f, num, -2f);
            optionBehaviour.SetClickMask(menu.ButtonClickMask);

            // "SetUpFromData"
            var componentsInChildren = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
            }
            foreach (var textMeshPro in optionBehaviour.GetComponentsInChildren<TextMeshPro>(true))
            {
                textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
                textMeshPro.fontMaterial.SetFloat("_Stencil", 20);
            }

            var stringOption = optionBehaviour as StringOption;
            stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            stringOption.TitleText.text = option.TitleColor == null ? Tr.Get(option.TitleKey) : Helpers.Cs(Tr.Get(option.TitleKey), (Color)option.TitleColor);

            if (option.IsHeader &&
                option.HeaderKey == "" &&
                (option.Type is CustomOptionType.Neutral or CustomOptionType.Crewmate or CustomOptionType.Impostor or CustomOptionType.Modifier))
            {
                stringOption.TitleText.text = Tr.Get("RoleOverviewTitle");
            }

            if (stringOption.TitleText.text.Length > 25)
            {
                stringOption.TitleText.fontSize = 2.2f;
            }

            if (stringOption.TitleText.text.Length > 40)
            {
                stringOption.TitleText.fontSize = 2f;
            }

            stringOption.Value = stringOption.oldValue = option.SelectedIndex;
            stringOption.ValueText.text = option.Selections[option.SelectedIndex].ToString();
            option.OptionBehaviour = stringOption;

            menu.Children.Add(optionBehaviour);
            num -= 0.45f;
            menu.scrollBar.SetYBoundsMax(-num - 1.65f);
        }

        for (int i = 0; i < menu.Children.Count; i++)
        {
            var optionBehaviour = menu.Children[i];
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                optionBehaviour.SetAsPlayer();
            }
        }
    }

    internal static void RemoveVanillaTabs(GameSettingMenu __instance)
    {
        GameObject.Find("What Is This?")?.Destroy();
        GameObject.Find("GamePresetButton")?.Destroy();
        GameObject.Find("RoleSettingsButton")?.Destroy();
        __instance.ChangeTab(1, false);
    }

    internal static void CreateCustomButton(GameSettingMenu __instance, int targetMenu, string buttonName, string buttonText)
    {
        var leftPanel = GameObject.Find("LeftPanel");
        var buttonTemplate = GameObject.Find("GameSettingsButton");
        if (targetMenu == 3)
        {
            buttonTemplate.transform.localPosition -= Vector3.up * 0.85f;
            buttonTemplate.transform.localScale *= Vector2.one * 0.75f;
        }

        var modSettingsButton = GameObject.Find(buttonName);
        if (modSettingsButton is null)
        {
            modSettingsButton = UnityEngine.Object.Instantiate(buttonTemplate, leftPanel.transform);
            modSettingsButton.transform.localPosition += Vector3.up * 0.5f * (targetMenu - 2);
            modSettingsButton.name = buttonName;
            __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { modSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));

            var modSettingsPassiveButton = modSettingsButton.GetComponent<PassiveButton>();
            modSettingsPassiveButton.OnClick.RemoveAllListeners();
            modSettingsPassiveButton.OnClick.AddListener((Action)(() => { __instance.ChangeTab(targetMenu, false); }));
            modSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
            modSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
            modSettingsPassiveButton.SelectButton(false);
            CurrentGOMButtons.Add(modSettingsPassiveButton);
        }
    }

    internal static void CreateGameOptionsMenu(GameSettingMenu __instance, CustomOptionType optionType, string settingName)
    {
        var template = GameObject.Find("GAME SETTINGS TAB");
        CurrentGOMTabs.RemoveAll(x => x == null);

        var modSettingsTab = UnityEngine.Object.Instantiate(template, template.transform.parent);
        modSettingsTab.name = settingName;

        var modSettingsGOM = modSettingsTab.GetComponent<GameOptionsMenu>();

        UpdateGameOptionsMenu(optionType, modSettingsGOM);

        CurrentGOMTabs.Add(modSettingsTab);
        modSettingsTab.SetActive(false);
        CurrentGOMs.Add((byte)optionType, modSettingsGOM);
    }

    internal static void UpdateGameOptionsMenu(CustomOptionType optionType, GameOptionsMenu modSettingsGOM)
    {
        foreach (var child in modSettingsGOM.Children)
        {
            child.Destroy();
        }
        modSettingsGOM.scrollBar.transform.FindChild("SliderInner").DestroyChildren();
        modSettingsGOM.Children.Clear();
        var relevantOptions = AllOptions.Values.Where(x => x.Type == optionType).ToList();
        CreateSettings(modSettingsGOM, relevantOptions);
    }

    internal static void CreateSettingTabs(GameSettingMenu __instance)
    {
        // Handle different gamemodes and tabs needed therein.
        var next = 3;
        if (MapOptions.GameMode is CustomGameMode.Classic)
        {
            // create RU settings
            CreateCustomButton(__instance, next++, "RUSettings", Tr.Get("CategoryHeaderModSettings"));
            CreateGameOptionsMenu(__instance, CustomOptionType.General, "RUSettings");
            // Imp
            CreateCustomButton(__instance, next++, "ImpostorSettings", Tr.Get("CategoryHeaderImpostorRoles"));
            CreateGameOptionsMenu(__instance, CustomOptionType.Impostor, "ImpostorSettings");
            // Neutral
            CreateCustomButton(__instance, next++, "NeutralSettings", Tr.Get("CategoryHeaderNeutralRoles"));
            CreateGameOptionsMenu(__instance, CustomOptionType.Neutral, "NeutralSettings");
            // Crew
            CreateCustomButton(__instance, next++, "CrewmateSettings", Tr.Get("CategoryHeaderCrewmateRoles"));
            CreateGameOptionsMenu(__instance, CustomOptionType.Crewmate, "CrewmateSettings");
            // Modifier
            CreateCustomButton(__instance, next++, "ModifierSettings", Tr.Get("CategoryHeaderModifier"));
            CreateGameOptionsMenu(__instance, CustomOptionType.Modifier, "ModifierSettings");
        }
        // else if (MapOptions.GameMode == CustomGameMode.HideNSeek)
        // {
        //     // create Main HNS settings
        //     createCustomButton(__instance, next++, "HideNSeekMain", "Hide 'N' Seek");
        //     createGameOptionsMenu(__instance, CustomOptionType.HideNSeekMain, "HideNSeekMain");
        //     // create HNS Role settings
        //     createCustomButton(__instance, next++, "HideNSeekRoles", "Hide 'N' Seek Roles");
        //     createGameOptionsMenu(__instance, CustomOptionType.HideNSeekRoles, "HideNSeekRoles");
        // }
    }

    internal static bool EnableStringOption(StringOption __instance)
    {
        var option = AllOptions.Values.FirstOrDefault(option => option.OptionBehaviour == __instance);
        if (option == null)
        {
            return true;
        }

        __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
        //__instance.TitleText.text = option.name;
        __instance.Value = __instance.oldValue = option.SelectedIndex;
        __instance.ValueText.text = option.Selections[option.SelectedIndex].ToString();

        return false;
    }

    internal static bool IncreaseStringOption(StringOption __instance)
    {
        var option = AllOptions.Values.FirstOrDefault(option => option.OptionBehaviour == __instance);
        if (option == null)
        {
            return true;
        }

        option.UpdateSelection(option.SelectedIndex + 1);
        // if (CustomOptionHolder.isMapSelectionOption(option))
        // {
        //     IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        //     currentGameOptions.SetByte(ByteOptionNames.MapId, (byte)option.selection);
        //     GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
        //     GameManager.Instance.LogicOptions.SyncOptions();
        // }
        return false;
    }

    internal static bool DecreaseStringOption(StringOption __instance)
    {
        var option = AllOptions.Values.FirstOrDefault(option => option.OptionBehaviour == __instance);
        if (option == null)
        {
            return true;
        }
        option.UpdateSelection(option.SelectedIndex - 1);
        // if (CustomOptionHolder.isMapSelectionOption(option))
        // {
        //     IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        //     currentGameOptions.SetByte(ByteOptionNames.MapId, (byte)option.selection);
        //     GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
        //     GameManager.Instance.LogicOptions.SyncOptions();
        // }
        return false;
    }
}