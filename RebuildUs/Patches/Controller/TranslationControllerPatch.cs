using System.Linq;
using HarmonyLib;
using RebuildUs.Extensions;
using RebuildUs.Localization;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class TranslationControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), [typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)])]
    public static bool GetStringPrefix(TranslationController __instance, StringNames id, ref string __result)
    {
        if ((int)id < 6000)
        {
            return true;
        }
        string ourString = "";

        // For now only do this in custom options.
        int idInt = (int)id - 6000;
        var opt = CustomOption.options.FirstOrDefault(x => x.Key == idInt).Value;
        ourString = Tr.Get(opt?.titleKey);

        __result = ourString;

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), [typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)])]
    public static void Postfix(ref string __result, StringNames id)
    {
        ExileExtensions.GetStringPostfix(ref __result, id);
    }
}