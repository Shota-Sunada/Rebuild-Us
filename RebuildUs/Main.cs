using System;
using AmongUs.Data;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace RebuildUs;

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
[BepInProcess("Among Us.exe")]
internal class RebuildUsPlugin : BasePlugin
{
    internal const string MOD_ID = "com.shota-sunada.rebuild-us";
    internal const string MOD_NAME = "Rebuild Us";
    internal const string MOD_VERSION = "1.0.0";

    internal static readonly Version Version = Version.Parse(MOD_VERSION);
    internal static RebuildUsPlugin Instance;
    internal Harmony Harmony { get; } = new(MOD_ID);
    internal static BepInEx.Logging.ManualLogSource Logger;

    public override void Load()
    {
        Logger = Log;
        Instance = this;

        Logger.LogInfo("Loading \"Rebuild Us\"...");

        Harmony.PatchAll();

        Logger.LogInfo("\"Rebuild Us\" was completely loaded! Enjoy the modifications!");
    }

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
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
}