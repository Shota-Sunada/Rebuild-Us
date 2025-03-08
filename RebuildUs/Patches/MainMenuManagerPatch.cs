using HarmonyLib;
using TMPro;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
internal static class MainMenuManagerStartPatch
{
    internal static void Postfix(MainMenuManager __instance)
    {
        ModManager.Instance.ShowModStamp();

        var ruLogo = new GameObject("RULogo");
        ruLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        ruLogo.transform.localPosition = new(-0.4f, 1f, 5f);

        var credits = new GameObject("RUModCredits");
        var text = credits.AddComponent<TextMeshPro>();
        text.SetText($"{RebuildUsPlugin.MOD_NAME} v{RebuildUsPlugin.MOD_VERSION}\n<size=50%>By {RebuildUsPlugin.MOD_DEVELOPER}</size>");
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize *= 0.05f;

        text.transform.SetParent(ruLogo.transform);
        text.transform.localPosition = Vector3.down * 1.25f;
    }
}