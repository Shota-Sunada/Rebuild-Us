using HarmonyLib;
using RebuildUs;

namespace RebuildUs.Modules.CustomHats.Patches;

[HarmonyPatch(typeof(CosmeticsCache))]
internal static class CosmeticsCachePatches
{
    [HarmonyPatch(nameof(CosmeticsCache.GetHat))]
    [HarmonyPrefix]
    private static bool GetHatPrefix(string id, ref HatViewData __result)
    {
        RebuildUsPlugin.Instance.Logger.LogMessage($"trying to load hat {id} from cosmetics cache");
        return !CustomHatManager.ViewDataCache.TryGetValue(id, out __result);
    }
}