using HarmonyLib;
using RebuildUs.Modules;
using RebuildUs.Utilities;
using TMPro;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class MainMenuManagerStartPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static void Postfix(MainMenuManager __instance)
    {
        FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();

        var ruLogo = new GameObject("RULogo");
        ruLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        ruLogo.transform.localPosition = new(-0.4f, 1f, 5f);

        var credits = new GameObject("RUModCredits");
        var text = credits.AddComponent<TextMeshPro>();
        text.SetText($"{Plugin.MOD_NAME} v{Plugin.MOD_VERSION}\n<size=50%>By {Plugin.MOD_DEVELOPER}</size>");
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize *= 0.05f;

        text.transform.SetParent(ruLogo.transform);
        text.transform.localPosition = Vector3.down * 1.25f;

        ClientOptions.SetupTitleText();
    }
}