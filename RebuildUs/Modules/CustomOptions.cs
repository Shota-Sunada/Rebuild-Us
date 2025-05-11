using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using System.Text;
using static RebuildUs.CustomOption;
using AmongUs.GameOptions;
using TMPro;
using RebuildUs.Localization;

namespace RebuildUs;

public class CustomOption
{
    public const int ROLE_OVERVIEW_ID = 99;
    public const StringNames KILL_RANGE_VERY_SHORT = (StringNames)49999;

    public static Dictionary<int, CustomOption> options = [];
    public static ConfigEntry<string> vanillaSettings;
    public static int preset = 0;

    public string titleKey;
    public Color? titleColor;
    public object[] selections;
    public OptionBehaviour optionBehaviour;
    public ConfigEntry<int> entry;

    public int defaultSelection;
    public int selection;
    public CustomOption parent;
    public bool isHeader;
    public CustomOptionType type;
    public string headerKey;
    public Color? headerColor;
    public UnitType unitType = UnitType.None;
    public List<CustomOption> children;

    public virtual bool Enabled => Helpers.RolesEnabled && getBool();

    public CustomOption(int id, CustomOptionType type, (string key, Color? color) title, object[] selections, object defaultValue, CustomOption parent, bool isHeader, (string Key, Color? color)? header, UnitType unitType)
    {
        this.type = type;
        titleKey = title.key;
        titleColor = title.color;
        this.selections = selections;
        var index = Array.IndexOf(selections, defaultValue);
        defaultSelection = index >= 0 ? index : 0;
        this.parent = parent;
        this.isHeader = isHeader;
        headerKey = header?.Key ?? "";
        headerColor = header?.color ?? null;
        this.unitType = unitType;

        this.children = new List<CustomOption>();
        if (parent != null)
        {
            parent.children.Add(this);
        }

        this.selection = 0;

        if (options.ContainsKey(id))
        {
            RebuildUs.Instance.Logger.LogWarning($"The custom option id ({id}) is already exists!");
            RebuildUs.Instance.Logger.LogWarning("The old one will be replaced by the new!");
        }

        options[id] = this;
    }

    public static CustomOption Create(
        int id,
        CustomOptionType type,
        (string key, Color? color) title,
        string[] selections,
        CustomOption parent = null,
        bool isHeader = false,
        (string key, Color? color)? header = null,
        UnitType unitType = UnitType.None)
    {
        return new(id, type, title, selections, "", parent, isHeader, header, unitType);
    }

    public static CustomOption Create(
        int id,
        CustomOptionType type,
        (string key, Color? color) title,
        float defaultValue,
        float min,
        float max,
        float interval,
        CustomOption parent = null,
        bool isHeader = false,
        (string key, Color? color)? header = null,
        UnitType unitType = UnitType.None)
    {
        var selections = new List<object>();
        for (float i = min; i <= max; i += interval)
        {
            selections.Add(i);
        }
        return new(id, type, title, [.. selections], defaultValue, parent, isHeader, header, unitType);
    }

    public static CustomOption Create(
        int id,
        CustomOptionType type,
        (string key, Color? color) title,
        bool defaultValue,
        CustomOption parent = null,
        bool isHeader = false,
        (string key, Color? color)? header = null,
        UnitType unitType = UnitType.None)
    {
        return new(id, type, title, [Tr.Get("OptionOff"), Tr.Get("OptionOn")], defaultValue ? Tr.Get("OptionOn") : Tr.Get("OptionOff"), parent, isHeader, header, unitType);
    }

    public static CustomOption Create(
        int id,
        CustomOptionType type,
        string title,
        string[] selections,
        CustomOption parent = null,
        bool isHeader = false,
        string header = "",
        UnitType unitType = UnitType.None)
    {
        return new(id, type, (title, Color.white), selections, "", parent, isHeader, (header, Color.white), unitType);
    }

    public static CustomOption Create(
        int id,
        CustomOptionType type,
        string title,
        float defaultValue,
        float min,
        float max,
        float interval,
        CustomOption parent = null,
        bool isHeader = false,
        string header = "",
        UnitType unitType = UnitType.None)
    {
        var selections = new List<object>();
        for (float i = min; i <= max; i += interval)
        {
            selections.Add(i);
        }
        return new(id, type, (title, Color.white), [.. selections], defaultValue, parent, isHeader, (header, Color.white), unitType);
    }

    public static CustomOption Create(
        int id,
        CustomOptionType type,
        string title,
        bool defaultValue,
        CustomOption parent = null,
        bool isHeader = false,
        string header = "",
        UnitType unitType = UnitType.None)
    {
        return new(id, type, (title, Color.white), [Tr.Get("OptionOff"), Tr.Get("OptionOn")], defaultValue ? Tr.Get("OptionOn") : Tr.Get("OptionOff"), parent, isHeader, (header, Color.white), unitType);
    }

    // Static behaviour

    public static void switchPreset(int newPreset)
    {
        saveVanillaOptions();
        CustomOption.preset = newPreset;
        vanillaSettings = RebuildUs.Instance.Config.Bind($"Preset{preset}", "GameOptions", "");
        loadVanillaOptions();
        foreach (var (id, option) in CustomOption.options)
        {
            if (id == 0) continue;

            option.entry = RebuildUs.Instance.Config.Bind($"Preset{preset}", id.ToString(), option.defaultSelection);
            option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
            if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = option.selection;
                stringOption.ValueText.text = option.selections[option.selection].ToString();
            }
        }

        // make sure to reload all tabs, even the ones in the background, because they might have changed when the preset was switched!
        if (AmongUsClient.Instance?.AmHost == true)
        {
            foreach (var entry in GameOptionsMenuStartPatch.currentGOMs)
            {
                CustomOptionType optionType = (CustomOptionType)entry.Key;
                GameOptionsMenu gom = entry.Value;
                if (gom != null)
                {
                    GameOptionsMenuStartPatch.updateGameOptionsMenu(optionType, gom);
                }
            }
        }
    }

    public static void saveVanillaOptions()
    {
        vanillaSettings.Value = Convert.ToBase64String(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameManager.Instance.LogicOptions.currentGameOptions, false));
    }

    public static bool loadVanillaOptions()
    {
        string optionsString = vanillaSettings.Value;
        if (optionsString == "") return false;
        IGameOptions gameOptions = GameOptionsManager.Instance.gameOptionsFactory.FromBytes(Convert.FromBase64String(optionsString));
        if (gameOptions.Version < 8)
        {
            RebuildUs.Instance.Logger.LogMessage("tried to paste old settings, not doing this!");
            return false;
        }
        GameOptionsManager.Instance.GameHostOptions = gameOptions;
        GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.GameHostOptions;
        GameManager.Instance.LogicOptions.SetGameOptions(GameOptionsManager.Instance.CurrentGameOptions);
        GameManager.Instance.LogicOptions.SyncOptions();
        return true;
    }

    public static void ShareOptionChange(CustomOption option)
    {
        if (option is null)
        {
            return;
        }

        var id = options.FirstOrDefault(x => x.Value == option).Key;

        using var rpc = RPCProcedure.SendRPC(PlayerControl.LocalPlayer.NetId, CustomRPC.ShareOptions);
        rpc.Write((byte)1);
        rpc.Write((uint)id, true);
        rpc.Write(Convert.ToUInt32(option.selection), true);
    }

    public static void ShareOptionSelections()
    {
        if (PlayerControl.AllPlayerControls.Count <= 1 ||
             !AmongUsClient.Instance.AmHost &&
             PlayerControl.LocalPlayer == null)
        {
            return;
        }

        var optionsList = new Dictionary<int, CustomOption>(options);
        while (optionsList.Count != 0)
        {
            // takes less than 3 bytes per option on average
            var amount = (byte)Math.Min(optionsList.Count, 200);
            using var rpc = RPCProcedure.SendRPC(PlayerControl.LocalPlayer.NetId, CustomRPC.ShareOptions);
            rpc.Write(amount);
            for (int i = 0; i < amount; i++)
            {
                var option = optionsList.ElementAt(0);
                rpc.Write((uint)option.Key, true);
                rpc.Write(Convert.ToUInt32(option.Value.selection), true);
                optionsList.Remove(option.Key);
            }
        }
    }

    // Getter

    public int getSelection()
    {
        return selection;
    }

    public bool getBool()
    {
        return selection > 0;
    }

    public float getFloat()
    {
        return (float)selections[selection];
    }

    public int getInt()
    {
        return Mathf.RoundToInt(getFloat());
    }

    public int getQuantity()
    {
        return selection + 1;
    }

    public string getString()
    {
        string sel = selections[selection].ToString();
        if (unitType != UnitType.None)
        {
            return string.Format(Tr.Get(Enum.GetName(unitType)), sel);
        }
        return Tr.Get(sel);
    }

    public string getName()
    {
        return Tr.Get(titleKey);
    }

    public void updateSelection(int id, int newSelection, bool notifyUsers = true)
    {
        newSelection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
        if (AmongUsClient.Instance.AmClient && notifyUsers && selection != newSelection)
        {
            FastDestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage((StringNames)(6000 + id),
                unitType == UnitType.None
                    ? selections[newSelection].ToString()
                    : new StringBuilder(selections[newSelection].ToString()).Append(Tr.Get(Enum.GetName(unitType))).ToString(),
                false
            );
            try
            {
                selection = newSelection;
                if (FastDestroyableSingleton<GameStartManager>.Instance != null &&
                    FastDestroyableSingleton<GameStartManager>.Instance.LobbyInfoPane != null &&
                    FastDestroyableSingleton<GameStartManager>.Instance.LobbyInfoPane.LobbyViewSettingsPane != null &&
                    FastDestroyableSingleton<GameStartManager>.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.activeSelf)
                {
                    LobbyViewSettingsPaneChangeTabPatch.Postfix(FastDestroyableSingleton<GameStartManager>.Instance.LobbyInfoPane.LobbyViewSettingsPane, FastDestroyableSingleton<GameStartManager>.Instance.LobbyInfoPane.LobbyViewSettingsPane.currentTab);
                }
            }
            catch { }
        }
        selection = newSelection;

        if (optionBehaviour != null && optionBehaviour is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = selection;
            stringOption.ValueText.text = unitType == UnitType.None
                    ? selections[selection].ToString()
                    : new StringBuilder(selections[selection].ToString()).Append(Tr.Get(Enum.GetName(unitType))).ToString();
            if (AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer)
            {
                if (options[CustomOptionHolder.PRESET_OPTION_ID] == this && selection != preset)
                {
                    // Switch presets
                    switchPreset(selection);
                    ShareOptionSelections();
                }
                else if (entry != null)
                {
                    // Save selection to config
                    entry.Value = selection;
                    // Share single selection
                    ShareOptionChange(this);
                }
            }
        }
        else if (options[CustomOptionHolder.PRESET_OPTION_ID] == this && AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer)
        {
            // Share the preset switch for random maps, even if the menu isn't open!
            switchPreset(selection);
            // Share all selections
            ShareOptionSelections();
        }

        if (AmongUsClient.Instance?.AmHost == true)
        {
            var currentTab = GameOptionsMenuStartPatch.currentTabs.FirstOrDefault(x => x.active).GetComponent<GameOptionsMenu>();
            if (currentTab != null)
            {
                var optionType = options.Values.First(x => x.optionBehaviour == currentTab.Children[0]).type;
                GameOptionsMenuStartPatch.updateGameOptionsMenu(optionType, currentTab);
            }

        }
    }

    public static byte[] serializeOptions()
    {
        using (MemoryStream memoryStream = new())
        {
            using (BinaryWriter binaryWriter = new(memoryStream))
            {
                int lastId = -1;
                foreach (var option in CustomOption.options.OrderBy(x => x.Key))
                {
                    if (option.Key == CustomOptionHolder.PRESET_OPTION_ID) continue;
                    bool consecutive = lastId + 1 == option.Key;
                    lastId = option.Key;

                    binaryWriter.Write((byte)(option.Value.selection + (consecutive ? 128 : 0)));
                    if (!consecutive) binaryWriter.Write((ushort)option.Key);
                }
                binaryWriter.Flush();
                memoryStream.Position = 0L;
                return memoryStream.ToArray();
            }
        }
    }

    public static int deserializeOptions(byte[] inputValues)
    {
        BinaryReader reader = new(new MemoryStream(inputValues));
        int lastId = -1;
        bool somethingApplied = false;
        int errors = 0;
        while (reader.BaseStream.Position < inputValues.Length)
        {
            try
            {
                int selection = reader.ReadByte();
                int id = -1;
                bool consecutive = selection >= 128;
                if (consecutive)
                {
                    selection -= 128;
                    id = lastId + 1;
                }
                else
                {
                    id = reader.ReadUInt16();
                }
                if (id == CustomOptionHolder.PRESET_OPTION_ID) continue;
                lastId = id;
                var option = options.First(option => option.Key == id);
                option.Value.entry = RebuildUs.Instance.Config.Bind($"Preset{preset}", option.Key.ToString(), option.Value.defaultSelection);
                option.Value.selection = selection;
                if (option.Value.optionBehaviour != null && option.Value.optionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.Value.selection;
                    stringOption.ValueText.text = option.Value.selections[option.Value.selection].ToString();
                }
                somethingApplied = true;
            }
            catch (Exception e)
            {
                RebuildUs.Instance.Logger.LogWarning($"id:{lastId}:{e}: while deserializing - tried to paste invalid settings!");
                errors++;
            }
        }
        return Convert.ToInt32(somethingApplied) + (errors > 0 ? 0 : 1);
    }

    // Copy to or paste from clipboard (as string)
    public static void copyToClipboard()
    {
        GUIUtility.systemCopyBuffer = $"{RebuildUs.MOD_VERSION}!{Convert.ToBase64String(serializeOptions())}!{vanillaSettings.Value}";
    }

    public static int pasteFromClipboard()
    {
        string allSettings = GUIUtility.systemCopyBuffer;
        int ruOptionsFine = 0;
        bool vanillaOptionsFine = false;
        try
        {
            var settingsSplit = allSettings.Split("!");
            Version versionInfo = Version.Parse(settingsSplit[0]);
            string torSettings = settingsSplit[1];
            string vanillaSettingsSub = settingsSplit[2];
            ruOptionsFine = deserializeOptions(Convert.FromBase64String(torSettings));
            ShareOptionSelections();
            if (RebuildUs.Instance.Version > versionInfo && versionInfo < Version.Parse("4.6.0"))
            {
                vanillaOptionsFine = false;
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Host Info: Pasting vanilla settings failed, RU Options applied!");
            }
            else
            {
                vanillaSettings.Value = vanillaSettingsSub;
                vanillaOptionsFine = loadVanillaOptions();
            }
        }
        catch (Exception e)
        {
            RebuildUs.Instance.Logger.LogWarning($"{e}: tried to paste invalid settings!\n{allSettings}");
            string errorStr = allSettings.Length > 2 ? allSettings.Substring(0, 3) : "(empty clipboard) ";
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Host Info: You tried to paste invalid settings: \"{errorStr}...\"");
        }
        return Convert.ToInt32(vanillaOptionsFine) + ruOptionsFine;
    }
}




[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
class GameOptionsMenuChangeTabPatch
{
    public static void Postfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if (previewOnly) return;
        foreach (var tab in GameOptionsMenuStartPatch.currentTabs)
        {
            if (tab != null)
                tab.SetActive(false);
        }
        foreach (var pbutton in GameOptionsMenuStartPatch.currentButtons)
        {
            pbutton.SelectButton(false);
        }
        if (tabNum > 2)
        {
            tabNum -= 3;
            GameOptionsMenuStartPatch.currentTabs[tabNum].SetActive(true);
            GameOptionsMenuStartPatch.currentButtons[tabNum].SelectButton(true);
        }
    }
}

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.SetTab))]
class LobbyViewSettingsPaneRefreshTabPatch
{
    public static bool Prefix(LobbyViewSettingsPane __instance)
    {
        if ((int)__instance.currentTab < 15)
        {
            LobbyViewSettingsPaneChangeTabPatch.Postfix(__instance, __instance.currentTab);
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.ChangeTab))]
class LobbyViewSettingsPaneChangeTabPatch
{
    public static void Postfix(LobbyViewSettingsPane __instance, StringNames category)
    {
        int tabNum = (int)category;

        foreach (var pbutton in LobbyViewSettingsPatch.currentButtons)
        {
            pbutton.SelectButton(false);
        }
        if (tabNum > 20) // StringNames are in the range of 3000+
            return;
        __instance.taskTabButton.SelectButton(false);

        if (tabNum > 2)
        {
            tabNum -= 3;
            //GameOptionsMenuStartPatch.currentTabs[tabNum].SetActive(true);
            LobbyViewSettingsPatch.currentButtons[tabNum].SelectButton(true);
            LobbyViewSettingsPatch.drawTab(__instance, LobbyViewSettingsPatch.currentButtonTypes[tabNum]);
        }
    }
}

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
class LobbyViewSettingsPaneUpdatePatch
{
    public static void Postfix(LobbyViewSettingsPane __instance)
    {
        if (LobbyViewSettingsPatch.currentButtons.Count == 0)
        {
            LobbyViewSettingsPatch.gameModeChangedFlag = true;
            LobbyViewSettingsPatch.Postfix(__instance);

        }
    }
}


[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
class LobbyViewSettingsPatch
{
    public static List<PassiveButton> currentButtons = [];
    public static List<CustomOptionType> currentButtonTypes = [];
    public static bool gameModeChangedFlag = false;

    public static void createCustomButton(LobbyViewSettingsPane __instance, int targetMenu, string buttonName, string buttonText, CustomOptionType optionType)
    {
        buttonName = "View" + buttonName;
        var buttonTemplate = GameObject.Find("OverviewTab");
        var torSettingsButton = GameObject.Find(buttonName);
        if (torSettingsButton == null)
        {
            torSettingsButton = GameObject.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
            torSettingsButton.transform.localPosition += Vector3.right * 1.75f * (targetMenu - 2);
            torSettingsButton.name = buttonName;
            __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));
            var torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
            torSettingsPassiveButton.OnClick.RemoveAllListeners();
            torSettingsPassiveButton.OnClick.AddListener((System.Action)(() =>
            {
                __instance.ChangeTab((StringNames)targetMenu);
            }));
            torSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
            torSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
            torSettingsPassiveButton.SelectButton(false);
            currentButtons.Add(torSettingsPassiveButton);
            currentButtonTypes.Add(optionType);
        }
    }

    public static void Postfix(LobbyViewSettingsPane __instance)
    {
        currentButtons.ForEach(x => x?.Destroy());
        currentButtons.Clear();
        currentButtonTypes.Clear();

        removeVanillaTabs(__instance);

        createSettingTabs(__instance);

    }

    public static void removeVanillaTabs(LobbyViewSettingsPane __instance)
    {
        GameObject.Find("RolesTabs")?.Destroy();
        var overview = GameObject.Find("OverviewTab");
        if (!gameModeChangedFlag)
        {
            overview.transform.localScale = new Vector3(0.5f * overview.transform.localScale.x, overview.transform.localScale.y, overview.transform.localScale.z);
            overview.transform.localPosition += new Vector3(-1.2f, 0f, 0f);

        }
        overview.transform.Find("FontPlacer").transform.localScale = new Vector3(1.35f, 1f, 1f);
        overview.transform.Find("FontPlacer").transform.localPosition = new Vector3(-0.6f, -0.1f, 0f);
        gameModeChangedFlag = false;
    }

    public static void drawTab(LobbyViewSettingsPane __instance, CustomOptionType optionType)
    {

        var relevantOptions = options.Values.Where(x => x.type == optionType).ToList();

        if ((int)optionType == 99)
        {
            // Create 4 Groups with Role settings only
            relevantOptions.Clear();
            relevantOptions.AddRange(options.Values.Where(x => x.type == CustomOptionType.Impostor && x.isHeader));
            relevantOptions.AddRange(options.Values.Where(x => x.type == CustomOptionType.Neutral && x.isHeader));
            relevantOptions.AddRange(options.Values.Where(x => x.type == CustomOptionType.Crewmate && x.isHeader));
            relevantOptions.AddRange(options.Values.Where(x => x.type == CustomOptionType.Modifier && x.isHeader));
            foreach (var (id, option) in options)
            {
                if (option.parent != null && option.parent.getSelection() > 0)
                {
                    if (id == 103) //Deputy
                        relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.sheriffSpawnRate) + 1, option);
                    else if (id == 224) //Sidekick
                        relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.jackalSpawnRate) + 1, option);
                    else if (id == 358) //Prosecutor
                        relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.lawyerSpawnRate) + 1, option);
                }
            }
        }

        for (int j = 0; j < __instance.settingsInfo.Count; j++)
        {
            __instance.settingsInfo[j].gameObject.Destroy();
        }
        __instance.settingsInfo.Clear();

        float num = 1.44f;
        int i = 0;
        int singles = 1;
        int headers = 0;
        int lines = 0;
        var curType = CustomOptionType.Modifier;
        int numBonus = 0;

        foreach (var option in relevantOptions)
        {
            if (option.isHeader && (int)optionType != 99 || (int)optionType == 99 && curType != option.type)
            {
                curType = option.type;
                if (i != 0)
                {
                    num -= 0.85f;
                    numBonus++;
                }
                if (i % 2 != 0) singles++;
                headers++; // for header

                var categoryHeaderMasked = UnityEngine.Object.Instantiate(__instance.categoryHeaderOrigin);
                categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 61);
                categoryHeaderMasked.Title.text = option.headerKey != "" ?
                    option.headerColor == null ? Tr.Get(option.headerKey) : Helpers.cs((Color)option.headerColor, Tr.Get(option.headerKey)) :
                    option.titleColor == null ? Tr.Get(option.titleKey) : Helpers.cs((Color)option.titleColor, Tr.Get(option.titleKey));

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
                categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
                __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
                num -= 1.05f;
                i = 0;
            }
            else if (option.parent != null && (option.parent.selection == 0 || option.parent.parent != null && option.parent.parent.selection == 0)) continue;  // Hides options, for which the parent is disabled!
            if (option == CustomOptionHolder.crewmateRolesCountMax || option == CustomOptionHolder.neutralRolesCountMax || option == CustomOptionHolder.impostorRolesCountMax || option == CustomOptionHolder.modifiersCountMax || option == CustomOptionHolder.crewmateRolesFill)
                continue;

            ViewSettingsInfoPanel viewSettingsInfoPanel = UnityEngine.Object.Instantiate<ViewSettingsInfoPanel>(__instance.infoPanelOrigin);
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
            var value = option.getSelection();
            var settingTuple = handleSpecialOptionsView(option, option.titleKey, option.unitType == UnitType.None
                    ? option.selections[value].ToString()
                    : new StringBuilder(option.selections[value].ToString()).Append(Tr.Get(Enum.GetName(option.unitType))).ToString()
            );
            viewSettingsInfoPanel.SetInfo(StringNames.ImpostorsCategory, settingTuple.Item2, 61);
            viewSettingsInfoPanel.titleText.text = settingTuple.Item1;

            if (option.isHeader &&
                option.headerKey == "" &&
                (int)optionType is not ROLE_OVERVIEW_ID &&
                (option.type is CustomOptionType.Neutral or CustomOptionType.Crewmate or CustomOptionType.Impostor or CustomOptionType.Modifier))
            {
                viewSettingsInfoPanel.titleText.text = "Spawn Chance";
            }

            if ((int)optionType == 99)
            {
                viewSettingsInfoPanel.titleText.outlineColor = Color.white;
                viewSettingsInfoPanel.titleText.outlineWidth = 0.2f;
                if (option.type == CustomOptionType.Modifier)
                    viewSettingsInfoPanel.settingText.text = viewSettingsInfoPanel.settingText.text + GameOptionsDataPatch.buildModifierExtras(option);
            }
            __instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

            i++;
        }
        float actual_spacing = (headers * 1.05f + lines * 0.85f) / (headers + lines) * 1.01f;
        __instance.scrollBar.CalculateAndSetYBounds((float)(__instance.settingsInfo.Count + singles * 2 + headers), 2f, 5f, actual_spacing);

    }

    private static Tuple<string, string> handleSpecialOptionsView(CustomOption option, string defaultString, string defaultVal)
    {
        string name = defaultString;
        string val = defaultVal;
        if (option == CustomOptionHolder.crewmateRolesCountMin)
        {
            val = "";
            name = "Crewmate Roles";
            var min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
            var max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
            if (CustomOptionHolder.crewmateRolesFill.getBool())
            {
                var crewCount = PlayerControl.AllPlayerControls.Count - GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                int minNeutral = CustomOptionHolder.neutralRolesCountMin.getSelection();
                int maxNeutral = CustomOptionHolder.neutralRolesCountMax.getSelection();
                if (minNeutral > maxNeutral) minNeutral = maxNeutral;
                min = crewCount - maxNeutral;
                max = crewCount - minNeutral;
                if (min < 0) min = 0;
                if (max < 0) max = 0;
                val = "Fill: ";
            }
            if (min > max) min = max;
            val += (min == max) ? $"{max}" : $"{min} - {max}";
        }
        if (option == CustomOptionHolder.neutralRolesCountMin)
        {
            name = "Neutral Roles";
            var min = CustomOptionHolder.neutralRolesCountMin.getSelection();
            var max = CustomOptionHolder.neutralRolesCountMax.getSelection();
            if (min > max) min = max;
            val = (min == max) ? $"{max}" : $"{min} - {max}";
        }
        if (option == CustomOptionHolder.impostorRolesCountMin)
        {
            name = "Impostor Roles";
            var min = CustomOptionHolder.impostorRolesCountMin.getSelection();
            var max = CustomOptionHolder.impostorRolesCountMax.getSelection();
            if (max > GameOptionsManager.Instance.currentGameOptions.NumImpostors) max = GameOptionsManager.Instance.currentGameOptions.NumImpostors;
            if (min > max) min = max;
            val = (min == max) ? $"{max}" : $"{min} - {max}";
        }
        if (option == CustomOptionHolder.modifiersCountMin)
        {
            name = "Modifiers";
            var min = CustomOptionHolder.modifiersCountMin.getSelection();
            var max = CustomOptionHolder.modifiersCountMax.getSelection();
            if (min > max) min = max;
            val = (min == max) ? $"{max}" : $"{min} - {max}";
        }
        return new(name, val);
    }

    public static void createSettingTabs(LobbyViewSettingsPane __instance)
    {
        // Handle different gamemodes and tabs needed therein.
        int next = 3;
        if (MapOptions.gameMode == CustomGamemodes.Classic)
        {
            // create RU settings
            createCustomButton(__instance, next++, "RUSettings", "RU Settings", CustomOptionType.General);
            // create Role overview
            createCustomButton(__instance, next++, "RoleOverview", "Role Overview", (CustomOptionType)99);
            // IMp
            createCustomButton(__instance, next++, "ImpostorSettings", "Impostor Roles", CustomOptionType.Impostor);

            // Neutral
            createCustomButton(__instance, next++, "NeutralSettings", "Neutral Roles", CustomOptionType.Neutral);
            // Crew
            createCustomButton(__instance, next++, "CrewmateSettings", "Crewmate Roles", CustomOptionType.Crewmate);
            // Modifier
            createCustomButton(__instance, next++, "ModifierSettings", "Modifiers", CustomOptionType.Modifier);
        }
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
class GameOptionsMenuCreateSettingsPatch
{
    public static void Postfix(GameOptionsMenu __instance)
    {
        if (__instance.gameObject.name == "GAME SETTINGS TAB")
            adaptTaskCount(__instance);
    }

    private static void adaptTaskCount(GameOptionsMenu __instance)
    {
        // Adapt task count for main options
        var commonTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumCommonTasks).Cast<NumberOption>();
        if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);
        var shortTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumShortTasks).TryCast<NumberOption>();
        if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);
        var longTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumLongTasks).TryCast<NumberOption>();
        if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
    }
}


[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
class GameOptionsMenuStartPatch
{
    public static List<GameObject> currentTabs = [];
    public static List<PassiveButton> currentButtons = [];
    public static Dictionary<byte, GameOptionsMenu> currentGOMs = [];
    public static void Postfix(GameSettingMenu __instance)
    {
        currentTabs.ForEach(x => x?.Destroy());
        currentButtons.ForEach(x => x?.Destroy());
        currentTabs = [];
        currentButtons = [];
        currentGOMs.Clear();

        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        removeVanillaTabs(__instance);

        createSettingTabs(__instance);

        var GOMGameObject = GameObject.Find("GAME SETTINGS TAB");


        // create copy to clipboard and paste from clipboard buttons.
        var template = GameObject.Find("PlayerOptionsMenu(Clone)").transform.Find("CloseButton").gameObject;
        var holderGO = new GameObject("copyPasteButtonParent");
        var bgrenderer = holderGO.AddComponent<SpriteRenderer>();
        bgrenderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.CopyPasteBG.png", 175f);
        holderGO.transform.SetParent(template.transform.parent, false);
        holderGO.transform.localPosition = template.transform.localPosition + new Vector3(-8.3f, 0.73f, -2f);
        holderGO.layer = template.layer;
        holderGO.SetActive(true);
        var copyButton = GameObject.Instantiate(template, holderGO.transform);
        copyButton.transform.localPosition = new Vector3(-0.3f, 0.02f, -2f);
        var copyButtonPassive = copyButton.GetComponent<PassiveButton>();
        var copyButtonRenderer = copyButton.GetComponentInChildren<SpriteRenderer>();
        var copyButtonActiveRenderer = copyButton.transform.GetChild(1).GetComponent<SpriteRenderer>();
        copyButtonRenderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Copy.png", 100f);
        copyButton.transform.GetChild(1).transform.localPosition = Vector3.zero;
        copyButtonActiveRenderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.CopyActive.png", 100f);
        copyButtonPassive.OnClick.RemoveAllListeners();
        copyButtonPassive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        copyButtonPassive.OnClick.AddListener((System.Action)(() =>
        {
            copyToClipboard();
            copyButtonRenderer.color = Color.green;
            copyButtonActiveRenderer.color = Color.green;
            __instance.StartCoroutine(Effects.Lerp(1f, new System.Action<float>((p) =>
            {
                if (p > 0.95)
                {
                    copyButtonRenderer.color = Color.white;
                    copyButtonActiveRenderer.color = Color.white;
                }
            })));
        }));
        var pasteButton = GameObject.Instantiate(template, holderGO.transform);
        pasteButton.transform.localPosition = new Vector3(0.3f, 0.02f, -2f);
        var pasteButtonPassive = pasteButton.GetComponent<PassiveButton>();
        var pasteButtonRenderer = pasteButton.GetComponentInChildren<SpriteRenderer>();
        var pasteButtonActiveRenderer = pasteButton.transform.GetChild(1).GetComponent<SpriteRenderer>();
        pasteButtonRenderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Paste.png", 100f);
        pasteButtonActiveRenderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.PasteActive.png", 100f);
        pasteButtonPassive.OnClick.RemoveAllListeners();
        pasteButtonPassive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        pasteButtonPassive.OnClick.AddListener((System.Action)(() =>
        {
            pasteButtonRenderer.color = Color.yellow;
            int success = pasteFromClipboard();
            pasteButtonRenderer.color = success == 3 ? Color.green : success == 0 ? Color.red : Color.yellow;
            pasteButtonActiveRenderer.color = success == 3 ? Color.green : success == 0 ? Color.red : Color.yellow;
            __instance.StartCoroutine(Effects.Lerp(1f, new System.Action<float>((p) =>
            {
                if (p > 0.95)
                {
                    pasteButtonRenderer.color = Color.white;
                    pasteButtonActiveRenderer.color = Color.white;
                }
            })));
        }));
    }

    private static void createSettings(GameOptionsMenu menu, List<CustomOption> options)
    {
        float num = 1.5f;
        foreach (CustomOption option in options)
        {
            if (option.isHeader)
            {
                var categoryHeaderMasked = UnityEngine.Object.Instantiate(menu.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
                categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 20);
                categoryHeaderMasked.Title.text = option.headerKey != "" ?
                    option.headerColor == null ? Tr.Get(option.headerKey) : Helpers.cs((Color)option.headerColor, Tr.Get(option.headerKey)) :
                    option.titleColor == null ? Tr.Get(option.titleKey) : Helpers.cs((Color)option.titleColor, Tr.Get(option.titleKey));
                categoryHeaderMasked.Title.outlineColor = Color.white;
                categoryHeaderMasked.Title.outlineWidth = 0.2f;
                categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                categoryHeaderMasked.transform.localPosition = new(-0.903f, num, -2f);
                num -= 0.63f;
            }
            else if (option.parent != null && (option.parent.selection == 0 || (option.parent.parent != null && option.parent.parent.selection == 0)))
            {
                continue;
            }

            var optionBehaviour = UnityEngine.Object.Instantiate<StringOption>(menu.stringOptionOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
            optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
            optionBehaviour.SetClickMask(menu.ButtonClickMask);

            // "SetUpFromData"
            SpriteRenderer[] componentsInChildren = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
            }
            foreach (TextMeshPro textMeshPro in optionBehaviour.GetComponentsInChildren<TextMeshPro>(true))
            {
                textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
                textMeshPro.fontMaterial.SetFloat("_Stencil", (float)20);
            }

            var stringOption = optionBehaviour as StringOption;
            stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            stringOption.TitleText.text = option.titleColor == null ? Tr.Get(option.titleKey) : Helpers.cs((Color)option.titleColor, Tr.Get(option.titleKey));

            if (option.isHeader &&
                option.headerKey == "" &&
                (option.type is CustomOptionType.Neutral or CustomOptionType.Crewmate or CustomOptionType.Impostor or CustomOptionType.Modifier))
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

            stringOption.Value = stringOption.oldValue = option.selection;
            stringOption.ValueText.text = option.unitType == UnitType.None
                    ? option.selections[option.selection].ToString()
                    : new StringBuilder(option.selections[option.selection].ToString()).Append(Tr.Get(Enum.GetName(option.unitType))).ToString();
            option.optionBehaviour = stringOption;

            menu.Children.Add(optionBehaviour);
            num -= 0.45f;
            menu.scrollBar.SetYBoundsMax(-num - 1.65f);
        }

        for (int i = 0; i < menu.Children.Count; i++)
        {
            OptionBehaviour optionBehaviour = menu.Children[i];
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                optionBehaviour.SetAsPlayer();
            }
        }
    }

    private static void removeVanillaTabs(GameSettingMenu __instance)
    {
        GameObject.Find("What Is This?")?.Destroy();
        GameObject.Find("GamePresetButton")?.Destroy();
        GameObject.Find("RoleSettingsButton")?.Destroy();
        __instance.ChangeTab(1, false);
    }

    public static void createCustomButton(GameSettingMenu __instance, int targetMenu, string buttonName, string buttonText)
    {
        var leftPanel = GameObject.Find("LeftPanel");
        var buttonTemplate = GameObject.Find("GameSettingsButton");
        if (targetMenu == 3)
        {
            buttonTemplate.transform.localPosition -= Vector3.up * 0.85f;
            buttonTemplate.transform.localScale *= Vector2.one * 0.75f;
        }
        var torSettingsButton = GameObject.Find(buttonName);
        if (torSettingsButton == null)
        {
            torSettingsButton = GameObject.Instantiate(buttonTemplate, leftPanel.transform);
            torSettingsButton.transform.localPosition += Vector3.up * 0.5f * (targetMenu - 2);
            torSettingsButton.name = buttonName;
            __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));
            var torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
            torSettingsPassiveButton.OnClick.RemoveAllListeners();
            torSettingsPassiveButton.OnClick.AddListener((System.Action)(() =>
            {
                __instance.ChangeTab(targetMenu, false);
            }));
            torSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
            torSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
            torSettingsPassiveButton.SelectButton(false);
            currentButtons.Add(torSettingsPassiveButton);
        }
    }

    public static void createGameOptionsMenu(GameSettingMenu __instance, CustomOptionType optionType, string settingName)
    {
        var tabTemplate = GameObject.Find("GAME SETTINGS TAB");
        currentTabs.RemoveAll(x => x == null);

        var torSettingsTab = GameObject.Instantiate(tabTemplate, tabTemplate.transform.parent);
        torSettingsTab.name = settingName;

        var torSettingsGOM = torSettingsTab.GetComponent<GameOptionsMenu>();

        updateGameOptionsMenu(optionType, torSettingsGOM);

        currentTabs.Add(torSettingsTab);
        torSettingsTab.SetActive(false);
        currentGOMs.Add((byte)optionType, torSettingsGOM);
    }

    public static void updateGameOptionsMenu(CustomOptionType optionType, GameOptionsMenu torSettingsGOM)
    {
        foreach (var child in torSettingsGOM.Children)
        {
            child.Destroy();
        }
        torSettingsGOM.scrollBar.transform.FindChild("SliderInner").DestroyChildren();
        torSettingsGOM.Children.Clear();
        var relevantOptions = options.Values.Where(x => x.type == optionType).ToList();
        createSettings(torSettingsGOM, relevantOptions);
    }

    private static void createSettingTabs(GameSettingMenu __instance)
    {
        // Handle different gamemodes and tabs needed therein.
        int next = 3;
        if (MapOptions.gameMode == CustomGamemodes.Classic)
        {
            // create RU settings
            createCustomButton(__instance, next++, "RUSettings", "RU Settings");
            createGameOptionsMenu(__instance, CustomOptionType.General, "RUSettings");

            // Imp
            createCustomButton(__instance, next++, "ImpostorSettings", "Impostor Roles");
            createGameOptionsMenu(__instance, CustomOptionType.Impostor, "ImpostorSettings");

            // Neutral
            createCustomButton(__instance, next++, "NeutralSettings", "Neutral Roles");
            createGameOptionsMenu(__instance, CustomOptionType.Neutral, "NeutralSettings");
            // Crew
            createCustomButton(__instance, next++, "CrewmateSettings", "Crewmate Roles");
            createGameOptionsMenu(__instance, CustomOptionType.Crewmate, "CrewmateSettings");
            // Modifier
            createCustomButton(__instance, next++, "ModifierSettings", "Modifiers");
            createGameOptionsMenu(__instance, CustomOptionType.Modifier, "ModifierSettings");
        }
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
public class StringOptionEnablePatch
{
    public static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.options.Values.FirstOrDefault(option => option.optionBehaviour == __instance);
        if (option == null) return true;

        __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
        //__instance.TitleText.text = option.name;
        __instance.Value = __instance.oldValue = option.selection;
        __instance.ValueText.text = option.selections[option.selection].ToString();

        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
public class StringOptionIncreasePatch
{
    public static bool Prefix(StringOption __instance)
    {
        var option = options.FirstOrDefault(option => option.Value.optionBehaviour == __instance);
        if (option.Value == null) return true;
        option.Value.updateSelection(option.Key, option.Value.selection + 1);
        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
public class StringOptionDecreasePatch
{
    public static bool Prefix(StringOption __instance)
    {
        var option = options.FirstOrDefault(option => option.Value.optionBehaviour == __instance);
        if (option.Value == null) return true;
        option.Value.updateSelection(option.Key, option.Value.selection + 1);
        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
public class StringOptionFixedUpdate
{
    public static void Postfix(StringOption __instance)
    {
        // if (!IL2CPPChainloader.Instance.Plugins.TryGetValue("com.DigiWorm.LevelImposter", out PluginInfo _)) return;
        // CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
        // if (option == null || !CustomOptionHolder.isMapSelectionOption(option)) return;
        // if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 6)
        //     if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
        //     {
        //         stringOption.ValueText.text = option.selections[option.selection].ToString();
        //     }
        //     else if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOptionToo)
        //     {
        //         stringOptionToo.oldValue = stringOptionToo.Value = option.selection;
        //         stringOptionToo.ValueText.text = option.selections[option.selection].ToString();
        //     }
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public class RpcSyncSettingsPatch
{
    public static void Postfix()
    {
        //CustomOption.ShareOptionSelections();
        CustomOption.saveVanillaOptions();
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
public class AmongUsClientOnPlayerJoinedPatch
{
    public static void Postfix()
    {
        if (PlayerControl.LocalPlayer != null && AmongUsClient.Instance.AmHost)
        {
            GameManager.Instance.LogicOptions.SyncOptions();
            CustomOption.ShareOptionSelections();
        }
    }
}


[HarmonyPatch]
class GameOptionsDataPatch
{


    public static string optionToString(CustomOption option)
    {
        if (option == null) return "";
        return $"{option.getName()}: {option.getString()}";
    }

    public static string optionsToString(CustomOption option, bool skipFirst = false)
    {
        if (option == null)
        {
            RebuildUs.Instance.Logger.LogInfo("no option?");
            return "";
        }

        var options = new List<string>();
        if (!skipFirst) options.Add(optionToString(option));
        if (option.Enabled)
        {
            foreach (var op in option.children)
            {
                string str = optionsToString(op);
                if (str != "") options.Add(str);
            }
        }
        return string.Join("\n", options);
    }

    private static string buildRoleOptions()
    {
        var impRoles = buildOptionsOfType(CustomOptionType.Impostor, true) + "\n";
        var neutralRoles = buildOptionsOfType(CustomOptionType.Neutral, true) + "\n";
        var crewRoles = buildOptionsOfType(CustomOptionType.Crewmate, true) + "\n";
        var modifiers = buildOptionsOfType(CustomOptionType.Modifier, true);
        return impRoles + neutralRoles + crewRoles + modifiers;
    }
    public static string buildModifierExtras(CustomOption customOption)
    {
        // find options children with quantity
        var children = CustomOption.options.Values.Where(o => o.parent == customOption);
        var quantity = children.Where(o => o.titleKey.Contains("Quantity")).ToList();
        if (customOption.getSelection() == 0) return "";
        if (quantity.Count == 1) return $" ({quantity[0].getQuantity()})";
        return "";
    }

    private static string buildOptionsOfType(CustomOptionType type, bool headerOnly)
    {
        StringBuilder sb = new("\n");
        var options = CustomOption.options.Where(o => o.Value.type == type);
        if (MapOptions.gameMode == CustomGamemodes.Classic)
        {
            options = options.Where(x => !(x.Value == CustomOptionHolder.crewmateRolesFill));
        }

        foreach (var (id, option) in options)
        {
            if (option.parent == null)
            {
                string line = $"{option.titleKey}: {option.selections[option.selection].ToString()}";
                if (type == CustomOptionType.Modifier) line += buildModifierExtras(option);
                sb.AppendLine(line);
            }
            else if (option.parent.getSelection() > 0)
            {
                if (id == 224) //Sidekick
                    sb.AppendLine($"- {Helpers.cs(TeamJackal.Color, "Sidekick")}: {option.selections[option.selection].ToString()}");
                else if (id == 358) //Prosecutor
                    sb.AppendLine($"- {Helpers.cs(Lawyer.color, "Prosecutor")}: {option.selections[option.selection].ToString()}");
            }
        }
        if (headerOnly) return sb.ToString();
        else sb = new StringBuilder();

        foreach (var (id, option) in options)
        {
            if (option.parent != null)
            {
                bool isIrrelevant = (option.parent.getSelection() == 0) || (option.parent.parent != null && option.parent.parent.getSelection() == 0);

                Color c = isIrrelevant ? Color.grey : Color.white;  // No use for now
                if (isIrrelevant) continue;
                sb.AppendLine(Helpers.cs(c, $"{option.titleKey}: {option.selections[option.selection].ToString()}"));
            }
            else
            {
                if (option == CustomOptionHolder.crewmateRolesCountMin)
                {
                    var optionName = Helpers.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Crewmate Roles");
                    var min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
                    var max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
                    string optionValue = "";
                    if (CustomOptionHolder.crewmateRolesFill.getBool())
                    {
                        var crewCount = PlayerControl.AllPlayerControls.Count - GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                        int minNeutral = CustomOptionHolder.neutralRolesCountMin.getSelection();
                        int maxNeutral = CustomOptionHolder.neutralRolesCountMax.getSelection();
                        if (minNeutral > maxNeutral) minNeutral = maxNeutral;
                        min = crewCount - maxNeutral;
                        max = crewCount - minNeutral;
                        if (min < 0) min = 0;
                        if (max < 0) max = 0;
                        optionValue = "Fill: ";
                    }
                    if (min > max) min = max;
                    optionValue += (min == max) ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.neutralRolesCountMin)
                {
                    var optionName = Helpers.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Neutral Roles");
                    var min = CustomOptionHolder.neutralRolesCountMin.getSelection();
                    var max = CustomOptionHolder.neutralRolesCountMax.getSelection();
                    if (min > max) min = max;
                    var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.impostorRolesCountMin)
                {
                    var optionName = Helpers.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Impostor Roles");
                    var min = CustomOptionHolder.impostorRolesCountMin.getSelection();
                    var max = CustomOptionHolder.impostorRolesCountMax.getSelection();
                    if (max > GameOptionsManager.Instance.currentGameOptions.NumImpostors) max = GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                    if (min > max) min = max;
                    var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.modifiersCountMin)
                {
                    var optionName = Helpers.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Modifiers");
                    var min = CustomOptionHolder.modifiersCountMin.getSelection();
                    var max = CustomOptionHolder.modifiersCountMax.getSelection();
                    if (min > max) min = max;
                    var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if ((option == CustomOptionHolder.crewmateRolesCountMax) || (option == CustomOptionHolder.neutralRolesCountMax) || (option == CustomOptionHolder.impostorRolesCountMax) || option == CustomOptionHolder.modifiersCountMax)
                {
                    continue;
                }
                else
                {
                    sb.AppendLine($"\n{option.titleKey}: {option.selections[option.selection].ToString()}");
                }
            }
        }
        return sb.ToString();
    }

    public static int maxPage = 7;
    public static string buildAllOptions(string vanillaSettings = "", bool hideExtras = false)
    {
        if (vanillaSettings == "")
            vanillaSettings = GameOptionsManager.Instance.CurrentGameOptions.ToHudString(PlayerControl.AllPlayerControls.Count);
        int counter = RebuildUs.optionsPage;
        string hudString = counter != 0 && !hideExtras ? Helpers.cs(DateTime.Now.Second % 2 == 0 ? Color.white : Color.red, "(Use scroll wheel if necessary)\n\n") : "";

        maxPage = 7;
        switch (counter)
        {
            case 0:
                hudString += (!hideExtras ? "" : "Page 1: Vanilla Settings \n\n") + vanillaSettings;
                break;
            case 1:
                hudString += "Page 2: Rebuild Us Settings \n" + buildOptionsOfType(CustomOptionType.General, false);
                break;
            case 2:
                hudString += "Page 3: Role and Modifier Rates \n" + buildRoleOptions();
                break;
            case 3:
                hudString += "Page 4: Impostor Role Settings \n" + buildOptionsOfType(CustomOptionType.Impostor, false);
                break;
            case 4:
                hudString += "Page 5: Neutral Role Settings \n" + buildOptionsOfType(CustomOptionType.Neutral, false);
                break;
            case 5:
                hudString += "Page 6: Crewmate Role Settings \n" + buildOptionsOfType(CustomOptionType.Crewmate, false);
                break;
            case 6:
                hudString += "Page 7: Modifier Settings \n" + buildOptionsOfType(CustomOptionType.Modifier, false);
                break;
        }

        if (!hideExtras || counter != 0) hudString += $"\n Press TAB or Page Number for more... ({counter + 1}/{maxPage})";
        return hudString;
    }


    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    private static void Postfix(ref string __result)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek) return; // Allow Vanilla Hide N Seek
        __result = buildAllOptions(vanillaSettings: __result);
    }
}

[HarmonyPatch]
public class AddToKillDistanceSetting
{
    [HarmonyPatch(typeof(LegacyGameOptions), nameof(LegacyGameOptions.AreInvalid))]
    [HarmonyPrefix]

    public static bool Prefix(LegacyGameOptions __instance, ref int maxExpectedPlayers)
    {
        //making the killdistances bound check higher since extra short is added
        return __instance.MaxPlayers > maxExpectedPlayers || __instance.NumImpostors < 1
                || __instance.NumImpostors > 3 || __instance.KillDistance < 0
                || __instance.KillDistance >= LegacyGameOptions.KillDistances.Count
                || __instance.PlayerSpeedMod <= 0f || __instance.PlayerSpeedMod > 3f;
    }

    [HarmonyPatch(typeof(NormalGameOptionsV09), nameof(NormalGameOptionsV09.AreInvalid))]
    [HarmonyPrefix]

    public static bool Prefix(NormalGameOptionsV09 __instance, ref int maxExpectedPlayers)
    {
        return __instance.MaxPlayers > maxExpectedPlayers || __instance.NumImpostors < 1
                || __instance.NumImpostors > 3 || __instance.KillDistance < 0
                || __instance.KillDistance >= LegacyGameOptions.KillDistances.Count
                || __instance.PlayerSpeedMod <= 0f || __instance.PlayerSpeedMod > 3f;
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    [HarmonyPrefix]

    public static void Prefix(StringOption __instance)
    {
        //prevents indexoutofrange exception breaking the setting if long happens to be selected
        //when host opens the laptop
        if (__instance.Title == StringNames.GameKillDistance && __instance.Value == 3)
        {
            __instance.Value = 1;
            GameOptionsManager.Instance.currentNormalGameOptions.KillDistance = 1;
            GameManager.Instance.LogicOptions.SyncOptions();
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    [HarmonyPostfix]

    public static void Postfix(StringOption __instance)
    {
        if (__instance.Title == StringNames.GameKillDistance && __instance.Values.Count == 3)
        {
            __instance.Values = new(
                    new StringNames[] { (StringNames)49999, StringNames.SettingShort, StringNames.SettingMedium, StringNames.SettingLong });
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.AppendItem),
        new Type[] { typeof(Il2CppSystem.Text.StringBuilder), typeof(StringNames), typeof(string) })]
    [HarmonyPrefix]

    public static void Prefix(ref StringNames stringName, ref string value)
    {
        if (stringName == StringNames.GameKillDistance)
        {
            int index;
            if (GameOptionsManager.Instance.currentGameMode == GameModes.Normal)
            {
                index = GameOptionsManager.Instance.currentNormalGameOptions.KillDistance;
            }
            else
            {
                index = GameOptionsManager.Instance.currentHideNSeekGameOptions.KillDistance;
            }
            value = LegacyGameOptions.KillDistanceStrings[index];
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
        new[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    [HarmonyPriority(Priority.Last)]

    public static bool Prefix(ref string __result, ref StringNames id)
    {
        if ((int)id == 49999)
        {
            __result = "Very Short";
            return false;
        }
        return true;
    }

    public static void addKillDistance()
    {
        LegacyGameOptions.KillDistances = new(new float[] { 0.5f, 1f, 1.8f, 2.5f });
        LegacyGameOptions.KillDistanceStrings = new(new string[] { "Very Short", "Short", "Medium", "Long" });
    }

    [HarmonyPatch(typeof(StringGameSetting), nameof(StringGameSetting.GetValueString))]
    [HarmonyPrefix]
    public static bool AjdustStringForViewPanel(StringGameSetting __instance, float value, ref string __result)
    {
        if (__instance.OptionName != Int32OptionNames.KillDistance) return true;
        __result = LegacyGameOptions.KillDistanceStrings[(int)value];
        return false;
    }
}

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class GameOptionsNextPagePatch
{
    public static void Postfix(KeyboardJoystick __instance)
    {
        int page = RebuildUs.optionsPage;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            RebuildUs.optionsPage = (RebuildUs.optionsPage + 1) % 7;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            RebuildUs.optionsPage = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            RebuildUs.optionsPage = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            RebuildUs.optionsPage = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            RebuildUs.optionsPage = 3;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            RebuildUs.optionsPage = 4;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            RebuildUs.optionsPage = 5;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            RebuildUs.optionsPage = 6;
        }
        if (Input.GetKeyDown(KeyCode.F1))
            HudManagerUpdate.ToggleSettings(HudManager.Instance);
        if (Input.GetKeyDown(KeyCode.F2) && LobbyBehaviour.Instance)
            HudManagerUpdate.ToggleSummary(HudManager.Instance);
        if (RebuildUs.optionsPage >= GameOptionsDataPatch.maxPage) RebuildUs.optionsPage = 0;
    }
}


//This class is taken and adapted from Town of Us Reactivated, https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/CustomOption/Patches.cs, Licensed under GPLv3
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public class HudManagerUpdate
{
    private static TextMeshPro GameSettings;
    public static float
        MinX,/*-5.3F*/
        OriginalY = 2.9F,
        MinY = 2.9F;

    public static Scroller Scroller;
    private static Vector3 LastPosition;
    private static float lastAspect;
    private static bool setLastPosition = false;

    public static void Prefix(HudManager __instance)
    {
        if (GameSettings?.transform == null) return;

        // Sets the MinX position to the left edge of the screen + 0.1 units
        Rect safeArea = Screen.safeArea;
        float aspect = Mathf.Min((Camera.main).aspect, safeArea.width / safeArea.height);
        float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
        MinX = 0.1f - safeOrthographicSize * aspect;

        if (!setLastPosition || aspect != lastAspect)
        {
            LastPosition = new Vector3(MinX, MinY);
            lastAspect = aspect;
            setLastPosition = true;
            if (Scroller != null) Scroller.ContentXBounds = new FloatRange(MinX, MinX);
        }

        CreateScroller(__instance);

        Scroller.gameObject.SetActive(GameSettings.gameObject.activeSelf);

        if (!Scroller.gameObject.active) return;

        var rows = GameSettings.text.Count(c => c == '\n');
        float LobbyTextRowHeight = 0.06F;
        var maxY = Mathf.Max(MinY, rows * LobbyTextRowHeight + (rows - 38) * LobbyTextRowHeight);

        Scroller.ContentYBounds = new FloatRange(MinY, maxY);

        // Prevent scrolling when the player is interacting with a menu
        if (PlayerControl.LocalPlayer.CanMove != true)
        {
            GameSettings.transform.localPosition = LastPosition;

            return;
        }

        if (GameSettings.transform.localPosition.x != MinX ||
            GameSettings.transform.localPosition.y < MinY) return;

        LastPosition = GameSettings.transform.localPosition;
    }

    private static void CreateScroller(HudManager __instance)
    {
        if (Scroller != null) return;

        Transform target = GameSettings.transform;

        Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
        Scroller.transform.SetParent(GameSettings.transform.parent);
        Scroller.gameObject.layer = 5;

        Scroller.transform.localScale = Vector3.one;
        Scroller.allowX = false;
        Scroller.allowY = true;
        Scroller.active = true;
        Scroller.velocity = new Vector2(0, 0);
        Scroller.ScrollbarYBounds = new FloatRange(0, 0);
        Scroller.ContentXBounds = new FloatRange(MinX, MinX);
        Scroller.enabled = true;

        Scroller.Inner = target;
        target.SetParent(Scroller.transform);
    }

    [HarmonyPrefix]
    public static void Prefix2(HudManager __instance)
    {
        if (!settingsTMPs[0]) return;
        foreach (var tmp in settingsTMPs) tmp.text = "";
        var settingsString = GameOptionsDataPatch.buildAllOptions(hideExtras: true);
        var blocks = settingsString.Split("\n\n", StringSplitOptions.RemoveEmptyEntries); ;
        string curString = "";
        string curBlock;
        int j = 0;
        for (int i = 0; i < blocks.Length; i++)
        {
            curBlock = blocks[i];
            if (Helpers.lineCount(curBlock) + Helpers.lineCount(curString) < 43)
            {
                curString += curBlock + "\n\n";
            }
            else
            {
                settingsTMPs[j].text = curString;
                j++;

                curString = "\n" + curBlock + "\n\n";
                if (curString.Substring(0, 2) != "\n\n") curString = "\n" + curString;
            }
        }
        if (j < settingsTMPs.Length) settingsTMPs[j].text = curString;
        int blockCount = 0;
        foreach (var tmp in settingsTMPs)
        {
            if (tmp.text != "")
                blockCount++;
        }
        for (int i = 0; i < blockCount; i++)
        {
            settingsTMPs[i].transform.localPosition = new Vector3(-blockCount * 1.2f + 2.7f * i, 2.2f, -500f);
        }
    }

    private static TMPro.TextMeshPro[] settingsTMPs = new TMPro.TextMeshPro[4];
    private static GameObject settingsBackground;
    public static void OpenSettings(HudManager __instance)
    {
        if (__instance.FullScreen == null || MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) return;
        if (summaryTMP)
        {
            CloseSummary();
        }
        settingsBackground = GameObject.Instantiate(__instance.FullScreen.gameObject, __instance.transform);
        settingsBackground.SetActive(true);
        var renderer = settingsBackground.GetComponent<SpriteRenderer>();
        renderer.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        renderer.enabled = true;

        for (int i = 0; i < settingsTMPs.Length; i++)
        {
            settingsTMPs[i] = GameObject.Instantiate(__instance.KillButton.cooldownTimerText, __instance.transform);
            settingsTMPs[i].alignment = TMPro.TextAlignmentOptions.TopLeft;
            settingsTMPs[i].enableWordWrapping = false;
            settingsTMPs[i].transform.localScale = Vector3.one * 0.25f;
            settingsTMPs[i].gameObject.SetActive(true);
        }
    }

    public static void CloseSettings()
    {
        foreach (var tmp in settingsTMPs)
            if (tmp) tmp.gameObject.Destroy();

        if (settingsBackground) settingsBackground.Destroy();
    }

    public static void ToggleSettings(HudManager __instance)
    {
        if (settingsTMPs[0]) CloseSettings();
        else OpenSettings(__instance);
    }

    [HarmonyPrefix]
    public static void Prefix3(HudManager __instance)
    {
        if (!summaryTMP) return;
        summaryTMP.text = Helpers.previousEndGameSummary;

        summaryTMP.transform.localPosition = new Vector3(-3 * 1.2f, 2.2f, -500f);

    }

    private static TMPro.TextMeshPro summaryTMP = null;
    private static GameObject summaryBackground;
    public static void OpenSummary(HudManager __instance)
    {
        if (__instance.FullScreen == null || MapBehaviour.Instance && MapBehaviour.Instance.IsOpen || Helpers.previousEndGameSummary.IsNullOrWhiteSpace()) return;
        if (settingsTMPs[0])
        {
            CloseSettings();
        }
        summaryBackground = GameObject.Instantiate(__instance.FullScreen.gameObject, __instance.transform);
        summaryBackground.SetActive(true);
        var renderer = summaryBackground.GetComponent<SpriteRenderer>();
        renderer.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        renderer.enabled = true;


        summaryTMP = GameObject.Instantiate(__instance.KillButton.cooldownTimerText, __instance.transform);
        summaryTMP.alignment = TMPro.TextAlignmentOptions.TopLeft;
        summaryTMP.enableWordWrapping = false;
        summaryTMP.transform.localScale = Vector3.one * 0.3f;
        summaryTMP.gameObject.SetActive(true);

    }

    public static void CloseSummary()
    {
        summaryTMP?.gameObject.Destroy();
        summaryTMP = null;
        if (summaryBackground) summaryBackground.Destroy();
    }

    public static void ToggleSummary(HudManager __instance)
    {
        if (summaryTMP) CloseSummary();
        else OpenSummary(__instance);
    }

    static PassiveButton toggleSettingsButton;
    static GameObject toggleSettingsButtonObject;

    static PassiveButton toggleSummaryButton;
    static GameObject toggleSummaryButtonObject;

    static GameObject toggleZoomButtonObject;
    static PassiveButton toggleZoomButton;

    [HarmonyPostfix]
    public static void Postfix(HudManager __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        if (!toggleSettingsButton || !toggleSettingsButtonObject)
        {
            // add a special button for settings viewing:
            toggleSettingsButtonObject = GameObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
            toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -1.25f, -500f);
            toggleSettingsButtonObject.name = "TOGGLESETTINGSBUTTON";
            SpriteRenderer renderer = toggleSettingsButtonObject.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            SpriteRenderer rendererActive = toggleSettingsButtonObject.transform.Find("Active").GetComponent<SpriteRenderer>();
            toggleSettingsButtonObject.transform.Find("Background").localPosition = Vector3.zero;
            renderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Settings_Button.png", 100f);
            rendererActive.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Settings_ButtonActive.png", 100);
            toggleSettingsButton = toggleSettingsButtonObject.GetComponent<PassiveButton>();
            toggleSettingsButton.OnClick.RemoveAllListeners();
            toggleSettingsButton.OnClick.AddListener((Action)(() => ToggleSettings(__instance)));
        }
        toggleSettingsButtonObject.SetActive(__instance.MapButton.gameObject.active && !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) && GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek);
        toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -0.8f, -500f);

        if (!toggleZoomButton || !toggleZoomButtonObject)
        {
            // add a special button for settings viewing:
            toggleZoomButtonObject = GameObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
            toggleZoomButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -1.25f, -500f);
            toggleZoomButtonObject.name = "TOGGLEZOOMBUTTON";
            SpriteRenderer tZrenderer = toggleZoomButtonObject.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            SpriteRenderer tZArenderer = toggleZoomButtonObject.transform.Find("Active").GetComponent<SpriteRenderer>();
            toggleZoomButtonObject.transform.Find("Background").localPosition = Vector3.zero;
            tZrenderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Minus_Button.png", 100f);
            tZArenderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Minus_ButtonActive.png", 100);
            toggleZoomButton = toggleZoomButtonObject.GetComponent<PassiveButton>();
            toggleZoomButton.OnClick.RemoveAllListeners();
            toggleZoomButton.OnClick.AddListener((Action)(() => Helpers.toggleZoom()));
        }
        var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
        int numberOfLeftTasks = playerTotal - playerCompleted;
        bool zoomButtonActive = !(PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.Data.IsDead || (PlayerControl.LocalPlayer.Data.Role.IsImpostor && !CustomOptionHolder.deadImpsBlockSabotage.getBool()) || MeetingHud.Instance);
        zoomButtonActive &= numberOfLeftTasks <= 0 || !CustomOptionHolder.finishTasksBeforeHauntingOrZoomingOut.getBool();
        toggleZoomButtonObject.SetActive(zoomButtonActive);
        var posOffset = Helpers.zoomOutStatus ? new Vector3(-1.27f, -7.92f, -52f) : new Vector3(0, -1.6f, -52f);
        toggleZoomButtonObject.transform.localPosition = HudManager.Instance.MapButton.transform.localPosition + posOffset;
    }

    [HarmonyPostfix]
    public static void Postfix2(HudManager __instance)
    {
        if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
        {
            if (toggleSummaryButtonObject != null)
            {
                toggleSummaryButtonObject.SetActive(false);
                toggleSummaryButtonObject.Destroy();
                toggleSummaryButton.Destroy();
            }
            return;
        }
        if (!toggleSummaryButton || !toggleSummaryButtonObject)
        {
            // add a special button for settings viewing:
            toggleSummaryButtonObject = GameObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
            toggleSummaryButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -1.25f, -500f);
            toggleSummaryButtonObject.name = "TOGGLESUMMARYSBUTTON";
            SpriteRenderer renderer = toggleSummaryButtonObject.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            SpriteRenderer rendererActive = toggleSummaryButtonObject.transform.Find("Active").GetComponent<SpriteRenderer>();
            toggleSummaryButtonObject.transform.Find("Background").localPosition = Vector3.zero;
            renderer.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Endscreen.png", 100f);
            rendererActive.sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.EndscreenActive.png", 100f);
            toggleSummaryButton = toggleSummaryButtonObject.GetComponent<PassiveButton>();
            toggleSummaryButton.OnClick.RemoveAllListeners();
            toggleSummaryButton.OnClick.AddListener((Action)(() => ToggleSummary(__instance)));
        }
        toggleSummaryButtonObject.SetActive(__instance.SettingsButton.gameObject.active && LobbyBehaviour.Instance && !Helpers.previousEndGameSummary.IsNullOrWhiteSpace() && GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek
            && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started);
        toggleSummaryButtonObject.transform.localPosition = __instance.SettingsButton.transform.localPosition + new Vector3(-1.45f, 0.03f, -500f);
    }
}