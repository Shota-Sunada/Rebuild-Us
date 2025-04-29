using HarmonyLib;
using RebuildUs.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RebuildUs.Patches;

[HarmonyPatch]
class GetStringPatch
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), [typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)])]
    public static bool Prefix(TranslationController __instance, StringNames id, ref string __result)
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
}