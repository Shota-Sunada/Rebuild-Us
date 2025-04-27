using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using RebuildUs.Localization;
using RebuildUs.Modules;
using RebuildUs.Roles;

namespace RebuildUs;

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
[BepInProcess("Among Us.exe")]
internal class Plugin : BasePlugin
{
    internal const string MOD_ID = "com.shota-sunada.rebuild-us";
    internal const string MOD_NAME = "Rebuild Us";
    internal const string MOD_VERSION = "1.0.0";
    internal const string MOD_DEVELOPER = "Shota Sunada";

    internal static Plugin Instance;
    internal Harmony Harmony { get; } = new(MOD_ID);
    internal Version Version { get; } = Version.Parse(MOD_VERSION);
    internal ManualLogSource Logger;

    internal ConfigEntry<bool> GhostsCanSeeRoles { get; private set; }
    internal ConfigEntry<bool> GhostsCanSeeModifiers { get; private set; }
    internal ConfigEntry<bool> GhostsCanSeeInformation { get; private set; }
    internal ConfigEntry<bool> GhostsCanSeeVotes { get; private set; }
    internal ConfigEntry<bool> ShowGameOverview { get; private set; }

    public override void Load()
    {
        Logger = Log;
        Instance = this;

        GhostsCanSeeRoles = Config.Bind("Custom", "GhostsCanSeeRoles", true);
        GhostsCanSeeModifiers = Config.Bind("Custom", "GhostsCanSeeModifiers", true);
        GhostsCanSeeInformation = Config.Bind("Custom", "GhostsCanSeeInformation", true);
        GhostsCanSeeVotes = Config.Bind("Custom", "GhostsCanSeeVotes", true);
        ShowGameOverview = Config.Bind("Custom", "ShowGameOverview", true);

        Harmony.PatchAll();

        Tr.Initialize();

        CustomOptionHolders.Initialize();

        RolesManager.RegisterRoles();

        SubmergedCompatibility.Initialize();

        Logger.LogMessage("\"Rebuild Us\" was completely loaded! Enjoy the modifications!");
    }
}