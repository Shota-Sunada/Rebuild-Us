using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class VitalsMinigamePatch
{
    private static List<TextMeshPro> hackerTexts = [];

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    public static void BeginPostfix(VitalsMinigame __instance)
    {
        if (Hacker.exists && PlayerControl.LocalPlayer.isRole(RoleId.Hacker))
        {
            hackerTexts = [];
            foreach (var panel in __instance.vitals)
            {
                var text = UnityEngine.Object.Instantiate(__instance.SabText, panel.transform);
                hackerTexts.Add(text);
                UnityEngine.Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                text.gameObject.SetActive(false);
                text.transform.localScale = Vector3.one * 0.75f;
                text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);
            }
        }

        //Fix Visor in Vitals
        foreach (var panel in __instance.vitals)
        {
            if (panel.PlayerIcon != null && panel.PlayerIcon.cosmetics.skin != null)
            {
                panel.PlayerIcon.cosmetics.skin.transform.position = new Vector3(0, 0, 0f);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    public static void UpdatePostfix(VitalsMinigame __instance)
    {
        // Hacker show time since death
        if (Hacker.exists && PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && Hacker.getRole().hackerTimer > 0)
        {
            for (int k = 0; k < __instance.vitals.Length; k++)
            {
                var vitalsPanel = __instance.vitals[k];
                var player = vitalsPanel.PlayerInfo;

                // Hacker update
                if (vitalsPanel.IsDead)
                {
                    var deadPlayer = GameHistory.deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                    if (deadPlayer != null && k < hackerTexts.Count && hackerTexts[k] != null)
                    {
                        float timeSinceDeath = (float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds;
                        hackerTexts[k].gameObject.SetActive(true);
                        hackerTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                    }
                }
            }
        }
        else
        {
            foreach (var text in hackerTexts)
            {
                if (text != null && text.gameObject != null)
                {
                    text.gameObject.SetActive(false);
                }
            }
        }
    }
}