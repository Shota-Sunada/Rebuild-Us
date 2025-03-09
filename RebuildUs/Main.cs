using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using RebuildUs.Modules;
using RebuildUs.Roles;

namespace RebuildUs;

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
[BepInProcess("Among Us.exe")]
internal class RebuildUsPlugin : BasePlugin
{
    internal const string MOD_ID = "com.shota-sunada.rebuild-us";
    internal const string MOD_NAME = "Rebuild Us";
    internal const string MOD_VERSION = "1.0.0";
    internal const string MOD_DEVELOPER = "Shota Sunada";

    internal static RebuildUsPlugin Instance;
    internal Harmony Harmony { get; } = new(MOD_ID);
    internal ManualLogSource Logger;

    public override void Load()
    {
        Logger = Log;
        Instance = this;

        Harmony.PatchAll();

        CustomOptions.RegisterOptions();
        RolesManager.RegisterRoles();

        Logger.LogMessage("\"Rebuild Us\" was completely loaded! Enjoy the modifications!");
    }
}