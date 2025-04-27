using HarmonyLib;
using RebuildUs.Roles.RoleBase;

namespace RebuildUs.Patches;

[HarmonyPatch]
internal static class HudManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    internal static void StartPostfix(HudManager __instance)
    {
        Buttons.CreateButtons(__instance);
    }
}