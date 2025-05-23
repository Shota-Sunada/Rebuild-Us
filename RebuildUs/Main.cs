global using Il2CppInterop.Runtime;
global using Il2CppInterop.Runtime.Attributes;
global using Il2CppInterop.Runtime.InteropTypes;
global using Il2CppInterop.Runtime.InteropTypes.Arrays;
global using Il2CppInterop.Runtime.Injection;

global using RebuildUs.Enums;
global using RebuildUs.Utilities;
global using RebuildUs.Roles;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using RebuildUs.Modules;
using AmongUs.Data;
using RebuildUs.Modules.CustomHats;
using RebuildUs.Localization;
using BepInEx.Logging;
using AmongUs.Data.Player;

namespace RebuildUs;

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
[BepInProcess("Among Us.exe")]
public class RebuildUs : BasePlugin
{
    public const string MOD_ID = "com.shota-sunada.rebuild-us";
    public const string MOD_NAME = "Rebuild Us";
    public const string MOD_VERSION = "1.0.0";
    public const string MOD_DEVELOPER = "Shota Sunada";

    public static RebuildUs Instance;
    public Harmony Harmony { get; } = new(MOD_ID);
    public Version Version { get; } = Version.Parse(MOD_VERSION);
    public ManualLogSource Logger;

    public static int optionsPage = 2;

    public static ConfigEntry<bool> DebugMode { get; private set; }
    public static ConfigEntry<bool> GhostsSeeInformation { get; set; }
    public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
    public static ConfigEntry<bool> GhostsSeeModifier { get; set; }
    public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
    public static ConfigEntry<bool> ShowRoleSummary { get; set; }
    public static ConfigEntry<bool> ShowLighterDarker { get; set; }
    public static ConfigEntry<bool> EnableHorseMode { get; set; }
    public static ConfigEntry<bool> ShowVentsOnMap { get; set; }
    public static ConfigEntry<bool> ShowChatNotifications { get; set; }
    public static ConfigEntry<string> Ip { get; set; }
    public static ConfigEntry<ushort> Port { get; set; }
    public static ConfigEntry<string> ShowPopUpVersion { get; set; }

    public static IRegionInfo[] defaultRegions;

    // This is part of the Mini.RegionInstaller, Licensed under GPLv3
    // file="RegionInstallPlugin.cs" company="miniduikboot">
    public static void UpdateRegions()
    {
        ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
        var regions = new IRegionInfo[] {
                new StaticHttpRegionInfo("Custom", StringNames.NoTranslation, Ip.Value, new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new("Custom", Ip.Value, Port.Value, false) })).CastFast<IRegionInfo>()
            };

        IRegionInfo currentRegion = serverManager.CurrentRegion;
        Instance.Logger.LogInfo($"Adding {regions.Length} regions");
        foreach (IRegionInfo region in regions)
        {
            if (region == null)
                Instance.Logger.LogError("Could not add region");
            else
            {
                if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase))
                    currentRegion = region;
                serverManager.AddOrUpdateRegion(region);
            }
        }

        // AU remembers the previous region that was set, so we need to restore it
        if (currentRegion != null)
        {
            Instance.Logger.LogDebug("Resetting previous region");
            serverManager.SetRegion(currentRegion);
        }
    }

    public override void Load()
    {
        Logger = Log;
        Instance = this;

        DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
        GhostsSeeInformation = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
        GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
        GhostsSeeModifier = Config.Bind("Custom", "Ghosts See Modifier", true);
        GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
        ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
        ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", true);
        EnableHorseMode = Config.Bind("Custom", "Enable Horse Mode", false);
        ShowPopUpVersion = Config.Bind("Custom", "Show PopUp", "0");
        ShowVentsOnMap = Config.Bind("Custom", "Show vent positions on minimap", false);
        ShowChatNotifications = Config.Bind("Custom", "Show Chat Notifications", true);

        Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
        Port = Config.Bind("Custom", "Custom Server Port", (ushort)22023);
        defaultRegions = ServerManager.DefaultRegions;
        // Removes vanilla Servers
        ServerManager.DefaultRegions = new Il2CppReferenceArray<IRegionInfo>(new IRegionInfo[0]);
        UpdateRegions();

        DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
        Harmony.PatchAll();

        CustomOptionHolder.Load();
        CustomColors.Load();
        CustomHatManager.LoadHats();

        AddComponent<ModUpdater>();

        EventUtility.Load();
        SubmergedCompatibility.Initialize();
        MainMenuPatch.addSceneChangeCallbacks();
        _ = RoleInfo.loadReadme();
        AddToKillDistanceSetting.addKillDistance();

        Tr.Initialize();

        Logger.LogMessage("\"Rebuild Us\" was completely loaded! Enjoy the modifications!");
    }

    public static System.Random rnd = new((int)DateTime.Now.Ticks);

    public static void clearAndReloadRoles()
    {
        // Jester.clearAndReload();
        // Mayor.clearAndReload();
        // Portalmaker.clearAndReload();
        // Engineer.clearAndReload();
        // Lighter.clearAndReload();
        // Godfather.clearAndReload();
        // Mafioso.clearAndReload();
        // Janitor.clearAndReload();
        // Detective.clearAndReload();
        // TimeMaster.clearAndReload();
        // Medic.clearAndReload();
        Shifter.clearAndReload();
        Swapper.clearAndReload();
        // Lovers.clearAndReload();
        // Seer.clearAndReload();
        Morphing.clearAndReload();
        Camouflager.clearAndReload();
        // Hacker.clearAndReload();
        // Tracker.clearAndReload();
        Vampire.clearAndReload();
        Snitch.clearAndReload();
        // Jackal.clearAndReload();
        // Sidekick.clearAndReload();
        // Eraser.clearAndReload();
        Spy.clearAndReload();
        Trickster.clearAndReload();
        Cleaner.clearAndReload();
        Warlock.clearAndReload();
        SecurityGuard.clearAndReload();
        // Arsonist.clearAndReload();
        BountyHunter.clearAndReload();
        Vulture.clearAndReload();
        Medium.clearAndReload();
        Lawyer.clearAndReload();
        Pursuer.clearAndReload();
        Witch.clearAndReload();
        Ninja.clearAndReload();
        Thief.clearAndReload();
        Trapper.clearAndReload();
        Bomber.clearAndReload();
        Yoyo.clearAndReload();

        // Modifier
        Bait.clearAndReload();
        Bloody.clearAndReload();
        AntiTeleport.clearAndReload();
        Tiebreaker.clearAndReload();
        Sunglasses.clearAndReload();
        Mini.clearAndReload();
        Vip.clearAndReload();
        Invert.clearAndReload();
        Chameleon.clearAndReload();
        Armored.clearAndReload();

        // Gamemodes
        HandleGuesser.clearAndReload();
    }

    // Deactivate bans, since I always leave my local testing game and ban myself
    [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    public static class IsBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static class ChatControllerAwakePatch
    {
        private static void Prefix()
        {
            if (!EOSManager.Instance.isKWSMinor)
            {
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
        }
    }

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly System.Random random = new((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = [];

        public static void Postfix(KeyboardJoystick __instance)
        {
            // Spawn dummies
            if (DebugMode.Value && Input.GetKeyDown(KeyCode.F))
            {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                var i = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl, new InnerNet.ClientData(new(0)));
                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor((byte)random.Next(Palette.PlayerColors.Length));
                playerControl.Data.RpcSetTasks(Array.Empty<byte>());
            }

            // Terminate round
            if (Input.GetKeyDown(KeyCode.L))
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.ForceEnd);
                RPCProcedure.forceEnd();
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
        }
    }
}