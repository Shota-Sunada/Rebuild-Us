using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;
using static RebuildUs.Patches.PlayerControlFixedUpdatePatch;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Medic : RoleBase<Medic>
{
    public static Color Color = new Color32(126, 251, 194, byte.MaxValue);
    public static Color ShieldedColor = new Color32(0, 221, 255, byte.MaxValue);

    public static int showShielded { get { return CustomOptionHolder.medicShowShielded.getSelection(); } }
    public static bool showAttemptToShielded { get { return CustomOptionHolder.medicShowAttemptToShielded.getBool(); } }
    public static int setOrShowShieldAfterMeeting { get { return CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection(); } }
    public static bool showAttemptToMedic { get { return CustomOptionHolder.medicShowAttemptToMedic.getBool(); } }
    public static bool setShieldAfterMeeting { get { return CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 2; } }
    public static bool showShieldAfterMeeting { get { return CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 1; } }

    public Medic()
    {
        baseRoleId = roleId = RoleId.NoRole;
        usedShield = false;
        currentTarget = null;
        shielded = null;
        futureShielded = null;
        meetingAfterShielding = false;
    }

    public PlayerControl currentTarget;
    public PlayerControl shielded;
    public PlayerControl futureShielded;
    public bool meetingAfterShielding = false;
    public bool usedShield = false;

    private static CustomButton medicShieldButton;

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        if (player == PlayerControl.LocalPlayer)
        {
            currentTarget = setTarget();
            if (!usedShield) setPlayerOutline(currentTarget, ShieldedColor);
        }
    }
    public override void HudUpdate()
    {
        if (shielded != null && (shielded.isDead() || player.isDead()))
        {
            shielded = null;
        }
    }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        medicShieldButton = new CustomButton(
            () =>
            {
                medicShieldButton.Timer = 0f;

                using var writer = RPCProcedure.SendRPC(setShieldAfterMeeting ? CustomRPC.SetFutureShielded : CustomRPC.MedicSetShielded); ;
                writer.Write(currentTarget.PlayerId);

                if (setShieldAfterMeeting)
                {
                    RPCProcedure.setFutureShielded(PlayerControl.LocalPlayer.PlayerId, currentTarget.PlayerId);
                }
                else
                {
                    RPCProcedure.medicSetShielded(PlayerControl.LocalPlayer.PlayerId, currentTarget.PlayerId);
                }
                meetingAfterShielding = false;
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Medic) && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return !usedShield && currentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { },
            getButtonSprite(),
            ButtonOffset.LowerRight,
            hm,
            hm.UseButton,
            KeyCode.F
        );
    }
    public override void SetButtonCooldowns()
    {
        medicShieldButton.MaxTimer = 0f;

    }

    public override void Clear()
    {
        players = [];
    }

    public static bool shieldVisible(PlayerControl target)
    {
        bool hasVisibleShield = false;

        bool isMorphedMorphing = target == Morphing.morphing && Morphing.morphTarget != null && Morphing.morphTimer > 0f;
        if (local.shielded != null && ((target == local.shielded && !isMorphedMorphing) || (isMorphedMorphing && Morphing.morphTarget == local.shielded)))
        {
            hasVisibleShield = showShielded == 0 || Helpers.shouldShowGhostInfo() // Everyone or Ghost info
                || (showShielded == 1 && (PlayerControl.LocalPlayer == local.shielded || PlayerControl.LocalPlayer.isRole(RoleId.Medic))) // Shielded + Medic
                || (showShielded == 2 && PlayerControl.LocalPlayer.isRole(RoleId.Medic)); // Medic only
            // Make shield invisible till after the next meeting if the option is set (the medic can already see the shield)
            hasVisibleShield = hasVisibleShield && (local.meetingAfterShielding || !showShieldAfterMeeting || PlayerControl.LocalPlayer.isRole(RoleId.Medic) || Helpers.shouldShowGhostInfo());
        }
        return hasVisibleShield;
    }

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.ShieldButton.png", 115f);
        return buttonSprite;
    }
}