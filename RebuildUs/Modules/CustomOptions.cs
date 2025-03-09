using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RebuildUs.Helpers;
using TMPro;

namespace RebuildUs.Modules;

internal enum CustomOptionType
{
    General,
    Impostor,
    Crewmate,
    Neutral,
}

internal class CustomOption
{
    internal static List<GameObject> CurrentTabs { get; } = [];
    internal static List<PassiveButton> CurrentButtons { get; } = [];
    public static Dictionary<byte, GameOptionsMenu> CurrentGOMs { get; } = [];

    internal static List<CustomOption> AllOptions { get; } = [];

    internal uint Id;
    internal CustomOptionType Type;
    internal string TitleKey;
    internal object[] Options;
    internal int DefaultIndex;
    internal int SelectedIndex;
    internal OptionBehaviour Behaviour;
    internal CustomOption Parent;
    internal bool IsHeader;
    internal Action OnChange = null;
    internal string Heading;
    internal bool InvertedParent;

    internal CustomOption(uint id, CustomOptionType type, string titleKey, object[] options, object defaultValue, CustomOption parent, bool isHeader, Action onChange = null, string heading = "", bool invertedParent = false)
    {
        Id = id;
        TitleKey = parent == null ? titleKey : "- " + titleKey;
        Options = options;
        int index = Array.IndexOf(options, defaultValue);
        DefaultIndex = index >= 0 ? index : 0;
        Parent = parent;
        IsHeader = isHeader;
        Type = type;
        OnChange = onChange;
        Heading = heading;
        InvertedParent = invertedParent;

        SelectedIndex = 0;

        AllOptions.Add(this);
    }

    internal static CustomOption Create(uint id, CustomOptionType type, string titleKey, string[] options, CustomOption parent = null, bool isHeader = false, Action onChange = null, string heading = "", bool invertedParent = false)
    {
        return new CustomOption(id, type, titleKey, options, "", parent, isHeader, onChange, heading, invertedParent);
    }

    internal static CustomOption Create(uint id, CustomOptionType type, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false, Action onChange = null, string heading = "", bool invertedParent = false)
    {
        var options = new List<object>();
        for (float s = min; s <= max; s += step) options.Add(s);
        return new CustomOption(id, type, name, [.. options], defaultValue, parent, isHeader, onChange, heading, invertedParent);
    }

    internal static CustomOption Create(uint id, CustomOptionType type, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, Action onChange = null, string heading = "", bool invertedParent = false)
    {
        return new CustomOption(id, type, name, ["Off", "On"], defaultValue ? "On" : "Off", parent, isHeader, onChange, heading, invertedParent);
    }

    internal static void Initialize()
    {

    }

    internal static void DestroyVanillaObjects(GameSettingMenu menu)
    {
        GameObject.Find("What Is This?")?.Destroy();
        GameObject.Find("GamePresetButton")?.Destroy();
        GameObject.Find("RoleSettingsButton")?.Destroy();
        menu.ChangeTab(1, false);
    }

    internal static void CreateModTab(GameSettingMenu menu, int tabNum, string buttonName, string buttonTextKey)
    {
        var leftPanel = GameObject.Find("LeftPanel");
        var template = GameObject.Find("GameSettingsButton");

        if (tabNum == 3)
        {
            template.transform.localPosition -= Vector3.up * 0.85f;
            template.transform.localScale *= Vector2.one * 0.75f;
        }

        var modTab = GameObject.Find(buttonName);
        if (modTab == null)
        {
            modTab = UnityEngine.Object.Instantiate(template, leftPanel.transform);
            modTab.transform.localPosition += Vector3.up * 0.5f * (tabNum - 2);
            modTab.name = buttonName;
            menu.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { modTab.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonTextKey; })));
            var passiveButton = modTab.GetComponent<PassiveButton>();
            passiveButton.OnClick.RemoveAllListeners();
            passiveButton.OnClick.AddListener((Action)(() =>
            {
                menu.ChangeTab(tabNum, false);
            }));
            passiveButton.OnMouseOut.RemoveAllListeners();
            passiveButton.OnMouseOver.RemoveAllListeners();
            passiveButton.SelectButton(false);
            CurrentButtons.Add(passiveButton);
        }
    }

    internal static void CreateModSettingsMenu(GameSettingMenu menu, CustomOptionType type, string tabName)
    {
        var template = GameObject.Find("GAME SETTINGS TAB");
        CurrentTabs.RemoveAll(x => x == null);

        var tab = UnityEngine.Object.Instantiate(template, template.transform.parent);
        tab.name = tabName;

        var gom = tab.GetComponent<GameOptionsMenu>();

        UpdateGameOptionsMenu(type, gom);

        CurrentTabs.Add(tab);
        tab.SetActive(false);
        CurrentGOMs.Add((byte)type, gom);
    }

    internal static void UpdateGameOptionsMenu(CustomOptionType type, GameOptionsMenu gom)
    {
        foreach (var child in gom.Children)
        {
            child.Destroy();
        }
        gom.scrollBar.transform.FindChild("SliderInner").DestroyChildren();
        gom.Children.Clear();
        // var relevantOptions = options.Where(x => x.type == type).ToList();
        // CreateSettings(gom, relevantOptions);
    }
}