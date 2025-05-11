using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class VentButtonPatch
{
    private static Sprite defaultVentSprite = null;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.SetTarget))]
    private static void SetTargetPostfix(VentButton __instance)
    {
        // Trickster render special vent button
        if (Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer)
        {
            if (defaultVentSprite == null) defaultVentSprite = __instance.graphic.sprite;
            bool isSpecialVent = __instance.currentTarget != null && __instance.currentTarget.gameObject != null && __instance.currentTarget.gameObject.name.StartsWith("JackInTheBoxVent_");
            __instance.graphic.sprite = isSpecialVent ? Trickster.getTricksterVentButtonSprite() : defaultVentSprite;
            __instance.buttonLabelText.enabled = !isSpecialVent;
        }
    }
}