using HarmonyLib;
using RebuildUs.Modules;

namespace RebuildUs.Patches;

public static class OptionsMenuBehaviourPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static void StartPostfix(OptionsMenuBehaviour __instance)
    {
        ClientOptions.OptionsMenuBehaviourStart(__instance);
    }
}