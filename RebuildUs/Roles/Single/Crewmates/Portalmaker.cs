using System;
using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public static class Portalmaker
{
    public static Color Color = new Color32(69, 69, 169, byte.MaxValue);

    public static PlayerControl portalmaker;

    public static float cooldown;
    public static float usePortalCooldown;
    public static bool logOnlyColorType;
    public static bool logHasTime;
    public static bool canPortalFromAnywhere;

    private static CustomButton portalmakerPlacePortalButton;
    private static CustomButton usePortalButton;
    private static CustomButton portalmakerMoveToPortalButton;
    public static TMPro.TMP_Text portalmakerButtonText1;
    public static TMPro.TMP_Text portalmakerButtonText2;

    private static Sprite placePortalButtonSprite;
    private static Sprite usePortalButtonSprite;
    private static Sprite usePortalSpecialButtonSprite1;
    private static Sprite usePortalSpecialButtonSprite2;
    private static Sprite logSprite;

    public static Sprite getPlacePortalButtonSprite()
    {
        if (placePortalButtonSprite) return placePortalButtonSprite;
        placePortalButtonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.PlacePortalButton.png", 115f);
        return placePortalButtonSprite;
    }

    public static Sprite getUsePortalButtonSprite()
    {
        if (usePortalButtonSprite) return usePortalButtonSprite;
        usePortalButtonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.UsePortalButton.png", 115f);
        return usePortalButtonSprite;
    }

    public static Sprite getUsePortalSpecialButtonSprite(bool first)
    {
        if (first)
        {
            if (usePortalSpecialButtonSprite1) return usePortalSpecialButtonSprite1;
            usePortalSpecialButtonSprite1 = Helpers.loadSpriteFromResources("RebuildUs.Resources.UsePortalSpecialButton1.png", 115f);
            return usePortalSpecialButtonSprite1;
        }
        else
        {
            if (usePortalSpecialButtonSprite2) return usePortalSpecialButtonSprite2;
            usePortalSpecialButtonSprite2 = Helpers.loadSpriteFromResources("RebuildUs.Resources.UsePortalSpecialButton2.png", 115f);
            return usePortalSpecialButtonSprite2;
        }
    }

    public static Sprite getLogSprite()
    {
        if (logSprite) return logSprite;
        logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton].Image;
        return logSprite;
    }

    public static void MakeButtons(HudManager hm)
    {
        portalmakerPlacePortalButton = new CustomButton(
            () =>
            {
                portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer;

                var pos = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlacePortal, Hazel.SendOption.Reliable);
                writer.WriteBytesAndSize(buff);
                writer.EndMessage();
                RPCProcedure.placePortal(buff);
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker) && !PlayerControl.LocalPlayer.isDead() && Portal.secondPortal == null; },
            () => { return PlayerControl.LocalPlayer.CanMove && Portal.secondPortal == null; },
            () => { portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer; },
            getPlacePortalButtonSprite(),
            ButtonOffset.LowerRight,
            hm,
            hm.UseButton,
            KeyCode.F
        );

        usePortalButton = new CustomButton(
            () =>
            {
                bool didTeleport = false;
                Vector3 exit = Portal.findExit(PlayerControl.LocalPlayer.transform.position);
                Vector3 entry = Portal.findEntry(PlayerControl.LocalPlayer.transform.position);

                bool portalMakerSoloTeleport = !Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position);
                if (portalMakerSoloTeleport)
                {
                    exit = Portal.firstPortal.portalGameObject.transform.position;
                    entry = PlayerControl.LocalPlayer.transform.position;
                }

                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(entry);

                if (!PlayerControl.LocalPlayer.Data.IsDead)
                {  // Ghosts can portal too, but non-blocking and only with a local animation
                    using var writer = RPCProcedure.SendRPC(CustomRPC.UsePortal);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(portalMakerSoloTeleport ? (byte)1 : (byte)0);
                }
                RPCProcedure.usePortal(PlayerControl.LocalPlayer.PlayerId, portalMakerSoloTeleport ? (byte)1 : (byte)0);
                usePortalButton.Timer = usePortalButton.MaxTimer;
                portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration, new Action<float>((p) =>
                { // Delayed action
                    PlayerControl.LocalPlayer.moveable = false;
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance)
                    {
                        if (SubmergedCompatibility.IsSubmerged)
                        {
                            SubmergedCompatibility.ChangeFloor(exit.y > -7);
                        }
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(exit);
                        didTeleport = true;
                    }
                    if (p == 1f)
                    {
                        PlayerControl.LocalPlayer.moveable = true;
                    }
                })));
            },
            () =>
            {
                if (PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker) && Portal.bothPlacedAndEnabled)
                {
                    portalmakerButtonText1.text = Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position) || !canPortalFromAnywhere ? "" : "1. " + Portal.firstPortal.room;
                }
                return Portal.bothPlacedAndEnabled;
            },
            () => { return PlayerControl.LocalPlayer.CanMove && (Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position) || canPortalFromAnywhere && PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker)) && !Portal.isTeleporting; },
            () => { usePortalButton.Timer = usePortalButton.MaxTimer; },
            getUsePortalButtonSprite(),
            new Vector3(0.9f, -0.06f, 0),
            hm,
            hm.UseButton,
            KeyCode.J,
            mirror: true
        );

        portalmakerMoveToPortalButton = new CustomButton(
            () =>
            {
                bool didTeleport = false;
                Vector3 exit = Portal.secondPortal.portalGameObject.transform.position;

                if (!PlayerControl.LocalPlayer.Data.IsDead)
                {  // Ghosts can portal too, but non-blocking and only with a local animation
                    using var writer = RPCProcedure.SendRPC(CustomRPC.UsePortal);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write((byte)2);
                }
                RPCProcedure.usePortal(PlayerControl.LocalPlayer.PlayerId, 2);
                usePortalButton.Timer = usePortalButton.MaxTimer;
                portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration, new Action<float>((p) =>
                { // Delayed action
                    PlayerControl.LocalPlayer.moveable = false;
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance)
                    {
                        if (SubmergedCompatibility.IsSubmerged)
                        {
                            SubmergedCompatibility.ChangeFloor(exit.y > -7);
                        }
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(exit);
                        didTeleport = true;
                    }
                    if (p == 1f)
                    {
                        PlayerControl.LocalPlayer.moveable = true;
                    }
                })));
            },
            () => { return canPortalFromAnywhere && Portal.bothPlacedAndEnabled && PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker); },
            () => { return PlayerControl.LocalPlayer.CanMove && !Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position) && !Portal.isTeleporting; },
            () => { portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer; },
            getUsePortalButtonSprite(),
            new Vector3(0.9f, 1f, 0),
            hm,
            hm.UseButton,
            KeyCode.G,
            mirror: true
        );

        portalmakerButtonText1 = GameObject.Instantiate(usePortalButton.actionButton.cooldownTimerText, usePortalButton.actionButton.cooldownTimerText.transform.parent);
        portalmakerButtonText1.text = "";
        portalmakerButtonText1.enableWordWrapping = false;
        portalmakerButtonText1.transform.localScale = Vector3.one * 0.5f;
        portalmakerButtonText1.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);

        portalmakerButtonText2 = GameObject.Instantiate(portalmakerMoveToPortalButton.actionButton.cooldownTimerText, portalmakerMoveToPortalButton.actionButton.cooldownTimerText.transform.parent);
        portalmakerButtonText2.text = "";
        portalmakerButtonText2.enableWordWrapping = false;
        portalmakerButtonText2.transform.localScale = Vector3.one * 0.5f;
        portalmakerButtonText2.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);
    }
    public static void SetButtonCooldowns()
    {
        portalmakerPlacePortalButton.MaxTimer = cooldown;
        usePortalButton.MaxTimer = usePortalCooldown;
        portalmakerMoveToPortalButton.MaxTimer = usePortalCooldown;
    }

    public static void clearAndReload()
    {
        portalmaker = null;
        cooldown = CustomOptionHolder.portalmakerCooldown.getFloat();
        usePortalCooldown = CustomOptionHolder.portalmakerUsePortalCooldown.getFloat();
        logOnlyColorType = CustomOptionHolder.portalmakerLogOnlyColorType.getBool();
        logHasTime = CustomOptionHolder.portalmakerLogHasTime.getBool();
        canPortalFromAnywhere = CustomOptionHolder.portalmakerCanPortalFromAnywhere.getBool();
    }
}