using System;
using System.Collections.Generic;
using RebuildUs.Localization;
using RebuildUs.Utilities;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Button;
using UnityEngine.Events;

namespace RebuildUs.Modules;

internal static class ClientOptions
{
    private static readonly SelectionBehaviour[] AllOptions = {
        new("GhostsCanSeeRoles", () => Plugin.Instance.GhostsCanSeeRoles.Value = !Plugin.Instance.GhostsCanSeeRoles.Value, Plugin.Instance.GhostsCanSeeRoles.Value),
        new("GhostsCanSeeModifiers", () => Plugin.Instance.GhostsCanSeeModifiers.Value = !Plugin.Instance.GhostsCanSeeModifiers.Value, Plugin.Instance.GhostsCanSeeModifiers.Value),
        new("GhostsCanSeeInformation", () => Plugin.Instance.GhostsCanSeeInformation.Value = !Plugin.Instance.GhostsCanSeeInformation.Value, Plugin.Instance.GhostsCanSeeInformation.Value),
        new("GhostsCanSeeVotes", () => Plugin.Instance.GhostsCanSeeVotes.Value = !Plugin.Instance.GhostsCanSeeVotes.Value, Plugin.Instance.GhostsCanSeeVotes.Value),
        new("ShowGameOverview", () => Plugin.Instance.ShowGameOverview.Value = !Plugin.Instance.ShowGameOverview.Value, Plugin.Instance.ShowGameOverview.Value),
    };

    private static GameObject popup;
    private static TextMeshPro titleText;

    private static ToggleButtonBehaviour buttonPrefab;
    private static Vector3? _origin;

    internal static void SetupTitleText()
    {
        var go = new GameObject("TitleTextRU");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.transform.localPosition += Vector3.left * 0.2f;
        titleText = UnityEngine.Object.Instantiate(tmp);
        titleText.gameObject.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(titleText);
    }

    internal static void Initialize(OptionsMenuBehaviour __instance)
    {
        if (!__instance.CensorChatButton) return;

        if (!popup)
        {
            CreateCustom(__instance);
        }
        if (!buttonPrefab)
        {
            buttonPrefab = UnityEngine.Object.Instantiate(__instance.CensorChatButton);
            UnityEngine.Object.DontDestroyOnLoad(buttonPrefab);
            buttonPrefab.name = "CensorChatPrefab";
            buttonPrefab.gameObject.SetActive(false);
        }
        SetUpOptions();
        InitializeMoreButton(__instance);
    }

    private static void CreateCustom(OptionsMenuBehaviour prefab)
    {
        popup = UnityEngine.Object.Instantiate(prefab.gameObject);
        UnityEngine.Object.DontDestroyOnLoad(popup);
        var transform = popup.transform;
        var pos = transform.localPosition;
        pos.z = -810f;
        transform.localPosition = pos;

        UnityEngine.Object.Destroy(popup.GetComponent<OptionsMenuBehaviour>());
        foreach (var gObj in popup.gameObject.GetAllChilds())
        {
            if (gObj.name != "Background" && gObj.name != "CloseButton")
            {
                UnityEngine.Object.Destroy(gObj);
            }
        }

        popup.SetActive(false);
    }

    private static void InitializeMoreButton(OptionsMenuBehaviour __instance)
    {
        var moreOptions = UnityEngine.Object.Instantiate(buttonPrefab, __instance.CensorChatButton.transform.parent);
        var transform = __instance.CensorChatButton.transform;
        __instance.CensorChatButton.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
        _origin ??= transform.localPosition;

        transform.localPosition = _origin.Value + Vector3.left * 0.45f;
        transform.localScale = new Vector3(0.66f, 1, 1);
        __instance.EnableFriendInvitesButton.transform.localScale = new Vector3(0.66f, 1, 1);
        __instance.EnableFriendInvitesButton.transform.localPosition += Vector3.right * 0.5f;
        __instance.EnableFriendInvitesButton.Text.transform.localScale = new Vector3(1.2f, 1, 1);

        moreOptions.transform.localPosition = _origin.Value + Vector3.right * 4f / 3f;
        moreOptions.transform.localScale = new Vector3(0.66f, 1, 1);

        moreOptions.gameObject.SetActive(true);
        moreOptions.Text.text = Tr.Get("ModOptions");
        moreOptions.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
        var moreOptionsButton = moreOptions.GetComponent<PassiveButton>();
        moreOptionsButton.OnClick = new ButtonClickedEvent();
        moreOptionsButton.OnClick.AddListener((Action)(() =>
        {
            bool closeUnderlying = false;
            if (!popup) return;

            if (__instance.transform.parent && __instance.transform.parent == FastDestroyableSingleton<HudManager>.Instance.transform)
            {
                popup.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                popup.transform.localPosition = new Vector3(0, 0, -800f);
                closeUnderlying = true;
            }
            else
            {
                popup.transform.SetParent(null);
                UnityEngine.Object.DontDestroyOnLoad(popup);
            }

            CheckSetTitle();
            RefreshOpen();
            if (closeUnderlying)
                __instance.Close();
        }));
    }

    private static void RefreshOpen()
    {
        popup.gameObject.SetActive(false);
        popup.gameObject.SetActive(true);
        SetUpOptions();
    }

    private static void CheckSetTitle()
    {
        if (!popup || popup.GetComponentInChildren<TextMeshPro>() || !titleText) return;

        var title = UnityEngine.Object.Instantiate(titleText, popup.transform);
        title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
        title.gameObject.SetActive(true);
        title.text = Tr.Get("MoreOptions");
        title.name = "TitleText";
    }

    private static void SetUpOptions()
    {
        if (popup.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

        for (var i = 0; i < AllOptions.Length; i++)
        {
            var info = AllOptions[i];

            var button = UnityEngine.Object.Instantiate(buttonPrefab, popup.transform);
            var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

            var transform = button.transform;
            transform.localPosition = pos;

            button.onState = info.DefaultValue;
            button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

            button.Text.text = Tr.Get(info.TitleKey);
            button.Text.fontSizeMin = button.Text.fontSizeMax = 1.8f;
            button.Text.font = UnityEngine.Object.Instantiate(titleText.font);
            button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

            button.name = info.TitleKey.Replace(" ", "") + "Toggle";
            button.gameObject.SetActive(true);

            var passiveButton = button.GetComponent<PassiveButton>();
            var colliderButton = button.GetComponent<BoxCollider2D>();

            colliderButton.size = new Vector2(2.2f, .7f);

            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnMouseOut = new UnityEvent();
            passiveButton.OnMouseOver = new UnityEvent();

            passiveButton.OnClick.AddListener((Action)(() =>
            {
                button.onState = info.OnClick();
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
            }));

            passiveButton.OnMouseOver.AddListener((Action)(() => button.Background.color = button.onState ? new Color32(34, 139, 34, byte.MaxValue) : new Color32(139, 34, 34, byte.MaxValue)));
            passiveButton.OnMouseOut.AddListener((Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

            foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                spr.size = new Vector2(2.2f, .7f);
            }
        }
    }

    private static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
    {
        for (var i = 0; i < Go.transform.childCount; i++)
        {
            yield return Go.transform.GetChild(i).gameObject;
        }
    }
}

internal class SelectionBehaviour
{
    internal string TitleKey;
    internal Func<bool> OnClick;
    internal bool DefaultValue;

    internal SelectionBehaviour(string titleKey, Func<bool> onClick, bool defaultValue)
    {
        TitleKey = titleKey;
        OnClick = onClick;
        DefaultValue = defaultValue;
    }
}