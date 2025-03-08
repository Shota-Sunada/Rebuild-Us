using HarmonyLib;
using AmongUs.Data;
using InnerNet;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
internal static class ChatControllerAwakePatch
{
    internal static void Prefix()
    {
        if (!EOSManager.Instance.isKWSMinor)
        {
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }
    }
}