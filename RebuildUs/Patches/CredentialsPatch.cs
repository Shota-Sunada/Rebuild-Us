using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class CredentialsPatch
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    internal static class PingTrackerPatch
    {
        static void Postfix(PingTracker __instance)
        {
            __instance.text.alignment = TextAlignmentOptions.Top;
            var position = __instance.GetComponent<AspectPosition>();
            position.Alignment = AspectPosition.EdgeAlignments.Top;

            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                __instance.text.text = $"{RebuildUs.MOD_NAME} v{RebuildUs.MOD_VERSION}\n{__instance.text.text}";
                position.DistanceFromEdge = MeetingHud.Instance ? new(1.25f, 0.15f, 0) : new(1.55f, 0.15f, 0);
            }
            else
            {
                __instance.text.text = $"{RebuildUs.MOD_NAME} v{RebuildUs.MOD_VERSION}\n<size=50%>By {RebuildUs.MOD_DEVELOPER}</size>\n{__instance.text.text}";
                position.DistanceFromEdge = new(0f, 0.1f, 0);
            }

            position.AdjustPosition();
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class LogoPatch
    {
        public static SpriteRenderer renderer;
        public static Sprite bannerSprite;
        public static Sprite horseBannerSprite;
        public static Sprite banner2Sprite;
        private static PingTracker instance;

        public static GameObject motdObject;
        public static TextMeshPro motdText;

        static void Postfix(PingTracker __instance)
        {
            ModManager.Instance.ShowModStamp();

            var ruLogo = new GameObject("RULogo");
            ruLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
            ruLogo.transform.localPosition = new(-0.4f, 1f, 5f);

            var credits = new GameObject("RUModCredits");
            var text = credits.AddComponent<TextMeshPro>();
            text.SetText($"{RebuildUs.MOD_NAME} v{RebuildUs.MOD_VERSION}\n<size=50%>By {RebuildUs.MOD_DEVELOPER}</size>");
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize *= 0.05f;

            text.transform.SetParent(ruLogo.transform);
            text.transform.localPosition = Vector3.down * 1.25f;
        }

        public static void loadSprites()
        {
            if (bannerSprite == null) bannerSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Banner.png", 300f);
            if (banner2Sprite == null) banner2Sprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Banner2.png", 300f);
            if (horseBannerSprite == null) horseBannerSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.bannerTheHorseRoles.png", 300f);
        }

        public static void updateSprite()
        {
            loadSprites();
            if (renderer != null)
            {
                float fadeDuration = 1f;
                instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                {
                    renderer.color = new Color(1, 1, 1, 1 - p);
                    if (p == 1)
                    {
                        renderer.sprite = MapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                        instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                        {
                            renderer.color = new Color(1, 1, 1, p);
                        })));
                    }
                })));
            }
        }
    }
}