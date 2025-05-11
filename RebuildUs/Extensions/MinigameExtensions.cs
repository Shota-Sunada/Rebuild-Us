using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RebuildUs.Extensions;

public static class MinigameExtensions
{
    public static int page = 0;
    public static float timer = 0f;

    public static List<GameObject> nightVisionOverlays = null;
    private static Sprite overlaySprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.NightVisionOverlay.png", 350f);
    public static bool nightVisionIsActive = false;
    private static bool isLightsOut;

    public static void nightVisionUpdate(SurveillanceMinigame SkeldCamsMinigame = null, PlanetSurveillanceMinigame SwitchCamsMinigame = null, FungleSurveillanceMinigame FungleCamMinigame = null)
    {
        GameObject closeButton = null;
        if (nightVisionOverlays == null)
        {
            List<MeshRenderer> viewPorts = [];
            Transform viewablesTransform = null;
            if (SkeldCamsMinigame != null)
            {
                closeButton = SkeldCamsMinigame.Viewables.transform.Find("CloseButton").gameObject;
                foreach (var rend in SkeldCamsMinigame.ViewPorts) viewPorts.Add(rend);
                viewablesTransform = SkeldCamsMinigame.Viewables.transform;
            }
            else if (SwitchCamsMinigame != null)
            {
                closeButton = SwitchCamsMinigame.Viewables.transform.Find("CloseButton").gameObject;
                viewPorts.Add(SwitchCamsMinigame.ViewPort);
                viewablesTransform = SwitchCamsMinigame.Viewables.transform;
            }
            else if (FungleCamMinigame != null)
            {
                closeButton = FungleCamMinigame.transform.Find("CloseButton").gameObject;
                viewPorts.Add(FungleCamMinigame.viewport);
                viewablesTransform = FungleCamMinigame.viewport.transform;
            }
            else return;

            nightVisionOverlays = [];

            foreach (var renderer in viewPorts)
            {
                GameObject overlayObject;
                float zPosition;
                if (FungleCamMinigame != null)
                {
                    overlayObject = GameObject.Instantiate(closeButton, renderer.transform);
                    overlayObject.layer = renderer.gameObject.layer;
                    zPosition = -0.5f;
                    overlayObject.transform.localPosition = new Vector3(0, 0, zPosition);
                }
                else
                {
                    overlayObject = GameObject.Instantiate(closeButton, viewablesTransform);
                    zPosition = overlayObject.transform.position.z;
                    overlayObject.layer = closeButton.layer;
                    overlayObject.transform.position = new Vector3(renderer.transform.position.x, renderer.transform.position.y, zPosition);
                }
                Vector3 localScale = (SkeldCamsMinigame != null) ? new Vector3(0.91f, 0.612f, 1f) : new Vector3(2.124f, 1.356f, 1f);
                localScale = (FungleCamMinigame != null) ? new Vector3(10f, 10f, 1f) : localScale;
                overlayObject.transform.localScale = localScale;
                var overlayRenderer = overlayObject.GetComponent<SpriteRenderer>();
                overlayRenderer.sprite = overlaySprite;
                overlayObject.SetActive(false);
                GameObject.Destroy(overlayObject.GetComponent<CircleCollider2D>());
                nightVisionOverlays.Add(overlayObject);
            }
        }

        isLightsOut = PlayerControl.LocalPlayer.myTasks.ToArray().Any(x => x.name.Contains("FixLightsTask")) || Trickster.lightsOutTimer > 0;
        bool ignoreNightVision = CustomOptionHolder.camsNoNightVisionIfImpVision.getBool() && Helpers.hasImpVision(GameData.Instance.GetPlayerById(PlayerControl.LocalPlayer.PlayerId)) || PlayerControl.LocalPlayer.Data.IsDead;
        bool nightVisionEnabled = CustomOptionHolder.camsNightVision.getBool();

        if (isLightsOut && !nightVisionIsActive && nightVisionEnabled && !ignoreNightVision)
        {  // only update when something changed!
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Ninja.ninja && Ninja.invisibleTimer > 0f)
                {
                    continue;
                }
                pc.setLook("", 11, "", "", "", "", false);
            }
            foreach (var overlayObject in nightVisionOverlays)
            {
                overlayObject.SetActive(true);
            }
            // Dead Bodies
            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
            {
                SpriteRenderer component = deadBody.bodyRenderers.FirstOrDefault();
                component.material.SetColor("_BackColor", Palette.ShadowColors[11]);
                component.material.SetColor("_BodyColor", Palette.PlayerColors[11]);
            }
            nightVisionIsActive = true;
        }
        else if (!isLightsOut && nightVisionIsActive)
        {
            resetNightVision();
        }
    }

    public static void resetNightVision()
    {
        foreach (var go in nightVisionOverlays)
        {
            go.Destroy();
        }
        nightVisionOverlays = null;

        if (nightVisionIsActive)
        {
            nightVisionIsActive = false;
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (Camouflager.camouflageTimer > 0)
                {
                    pc.setLook("", 6, "", "", "", "", false);
                }
                else if (pc == Morphing.morphing && Morphing.morphTimer > 0)
                {
                    PlayerControl target = Morphing.morphTarget;
                    Morphing.morphing.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId, false);
                }
                else if (pc == Ninja.ninja && Ninja.invisibleTimer > 0f)
                {
                    continue;
                }
                else
                {
                    Helpers.setDefaultLook(pc, false);
                }
                // Dead Bodies
                foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                {
                    var colorId = GameData.Instance.GetPlayerById(deadBody.ParentId).Object.Data.DefaultOutfit.ColorId;
                    SpriteRenderer component = deadBody.bodyRenderers.FirstOrDefault();
                    component.material.SetColor("_BackColor", Palette.ShadowColors[colorId]);
                    component.material.SetColor("_BodyColor", Palette.PlayerColors[colorId]);
                }
            }
        }
    }

    public static void enforceNightVision(PlayerControl player)
    {
        if (isLightsOut && nightVisionOverlays != null && nightVisionIsActive)
        {
            player.setLook("", 11, "", "", "", "", false);
        }
    }
}