using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using RebuildUs.Localization;

namespace RebuildUs;

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
[BepInProcess("Among Us.exe")]
public class Plugin : BasePlugin
{
    public const string MOD_ID = "com.shota-sunada.rebuild-us";
    public const string MOD_NAME = "Rebuild Us";
    public const string MOD_VERSION = "1.0.0";
    public const string MOD_DEVELOPER = "Shota Sunada";

    public static Plugin Instance;
    public Harmony Harmony { get; } = new(MOD_ID);
    public Version Version { get; } = Version.Parse(MOD_VERSION);
    public ManualLogSource Logger;

    public Random Random = new((int)DateTime.Now.Ticks);

    public ConfigEntry<bool> GhostsCanSeeRoles { get; private set; }
    public ConfigEntry<bool> GhostsCanSeeModifiers { get; private set; }
    public ConfigEntry<bool> GhostsCanSeeInformation { get; private set; }
    public ConfigEntry<bool> GhostsCanSeeVotes { get; private set; }
    public ConfigEntry<bool> ShowGameOverview { get; private set; }

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

        Logger.LogMessage("\"Rebuild Us\" was completely loaded! Enjoy the modifications!");
    }
}