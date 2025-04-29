using HarmonyLib;
using AmongUs.Data;
using InnerNet;
using RebuildUs.Utilities;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class ChatControllerAwakePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static void AwakePrefix()
    {
        if (!FastDestroyableSingleton<EOSManager>.Instance.isKWSMinor)
        {
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }
    }
}