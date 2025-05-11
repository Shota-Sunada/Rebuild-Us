using System.Linq;
using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class ConsolePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static bool CanUsePrefix(ref float __result, Console __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;
        if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
        {
            return !__instance.TaskTypes.Any(x => x is TaskTypes.FixLights or TaskTypes.FixComms);
        }

        if (__instance.AllowImpostor)
        {
            return true;
        }

        if (!Helpers.hasFakeTasks(pc.Object))
        {
            return true;
        }

        __result = float.MaxValue;

        return false;
    }
}