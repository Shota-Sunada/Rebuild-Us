using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
internal class MainMenuManagerStartPatch
{
    internal static void Postfix(MainMenuManager __instance)
    {
        ModManager.Instance.ShowModStamp();
    }
}