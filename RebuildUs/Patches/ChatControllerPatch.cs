using HarmonyLib;
using AmongUs.Data;
using InnerNet;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class ChatControllerAwakePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    internal static void AwakePrefix()
    {
        if (!EOSManager.Instance.isKWSMinor)
        {
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }
    }
}