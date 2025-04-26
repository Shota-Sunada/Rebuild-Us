using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class OptionsMenuBehaviourPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    internal static void StartPostfix(OptionsMenuBehaviour __instance)
    {
        ClientOptions.Initialize(__instance);
    }
}