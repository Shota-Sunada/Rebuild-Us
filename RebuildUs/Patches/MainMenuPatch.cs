using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using RebuildUs.Patches;
using UnityEngine.SceneManagement;
using RebuildUs.Utilities;
using AmongUs.Data;
using Assets.InnerNet;
using System.Linq;

namespace RebuildUs.Modules;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public class MainMenuPatch
{
    private static void Prefix(MainMenuManager __instance)
    {
    }

    public static void addSceneChangeCallbacks()
    {
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
        {
            if (!scene.name.Equals("MatchMaking", StringComparison.Ordinal)) return;
            MapOptions.gameMode = CustomGamemodes.Classic;
            // Add buttons For Guesser Mode, Hide N Seek in this scene.
            // find "HostLocalGameButton"

            // var template = GameObject.FindObjectOfType<HostLocalGameButton>();
            // var gameButton = template.transform.FindChild("CreateGameButton");
            // var gameButtonPassiveButton = gameButton.GetComponentInChildren<PassiveButton>();

            // var guesserButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
            // guesserButton.transform.localPosition += new Vector3(0f, -0.5f);
            // var guesserButtonText = guesserButton.GetComponentInChildren<TMPro.TextMeshPro>();
            // var guesserButtonPassiveButton = guesserButton.GetComponentInChildren<PassiveButton>();

            // guesserButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
            // guesserButtonPassiveButton.OnClick.AddListener((System.Action)(() =>
            // {
            //     TORMapOptions.gameMode = CustomGamemodes.Guesser;
            //     template.OnClick();
            // }));

            // var HideNSeekButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
            // HideNSeekButton.transform.localPosition += new Vector3(1.7f, -0.5f);
            // var HideNSeekButtonText = HideNSeekButton.GetComponentInChildren<TMPro.TextMeshPro>();
            // var HideNSeekButtonPassiveButton = HideNSeekButton.GetComponentInChildren<PassiveButton>();

            // HideNSeekButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
            // HideNSeekButtonPassiveButton.OnClick.AddListener((System.Action)(() =>
            // {
            //     TORMapOptions.gameMode = CustomGamemodes.HideNSeek;
            //     template.OnClick();
            // }));

            // var PropHuntButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
            // PropHuntButton.transform.localPosition += new Vector3(3.4f, -0.5f);
            // var PropHuntButtonText = PropHuntButton.GetComponentInChildren<TMPro.TextMeshPro>();
            // var PropHuntButtonPassiveButton = PropHuntButton.GetComponentInChildren<PassiveButton>();

            // PropHuntButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
            // PropHuntButtonPassiveButton.OnClick.AddListener((System.Action)(() =>
            // {
            //     TORMapOptions.gameMode = CustomGamemodes.PropHunt;
            //     template.OnClick();
            // }));

            // template.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            // {
            //     guesserButtonText.SetText("TOR Guesser");
            //     HideNSeekButtonText.SetText("TOR Hide N Seek");
            //     PropHuntButtonText.SetText("TOR Prop Hunt");
            // })));
        }));
    }
}