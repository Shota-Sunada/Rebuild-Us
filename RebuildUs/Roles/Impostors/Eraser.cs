using System.Collections.Generic;
using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;
using static RebuildUs.Patches.PlayerControlFixedUpdatePatch;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Eraser : RoleBase<Eraser>
{
    public static Color Color = Palette.ImpostorRed;

    public static List<byte> alreadyErased = [];

    public static List<PlayerControl> futureErased = [];
    public PlayerControl currentTarget;

    public static float cooldown { get { return CustomOptionHolder.eraserCooldown.getFloat(); } }
    public static float cooldownIncrease { get { return CustomOptionHolder.eraserCooldownIncrease.getFloat(); } }
    public static bool canEraseAnyone { get { return CustomOptionHolder.eraserCanEraseAnyone.getBool(); } }

    private static CustomButton eraserButton;

    public Eraser()
    {
        baseRoleId = roleId = RoleId.Eraser;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        List<PlayerControl> untargetablePlayers = [];
        if (Spy.spy != null) untargetablePlayers.Add(Spy.spy);
        if (Sidekick.wasTeamRed) untargetablePlayers.Add(Sidekick.sidekick);
        if (Jackal.wasTeamRed) untargetablePlayers.Add(Jackal.jackal);
        currentTarget = setTarget(onlyCrewmates: !canEraseAnyone, untargetablePlayers: canEraseAnyone ? [] : untargetablePlayers);
        setPlayerOutline(currentTarget, Color);
    }
    public override void HudUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        eraserButton = new CustomButton(
            () =>
            {
                eraserButton.MaxTimer += 10;
                eraserButton.Timer = eraserButton.MaxTimer;

                using var writer = RPCProcedure.SendRPC(CustomRPC.SetFutureErased);
                writer.Write(currentTarget.PlayerId);
                RPCProcedure.setFutureErased(currentTarget.PlayerId);
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Eraser) && !PlayerControl.LocalPlayer.isDead(); },
            () => { return PlayerControl.LocalPlayer.CanMove && currentTarget != null; },
            () => { eraserButton.Timer = eraserButton.MaxTimer; },
            getButtonSprite(),
            ButtonOffset.UpperLeft,
            hm,
            hm.KillButton,
            KeyCode.F
        );
    }
    public override void SetButtonCooldowns()
    {
        eraserButton.MaxTimer = cooldown;
    }

    public override void Clear()
    {
        players = [];
        futureErased = [];
        alreadyErased = [];
    }

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.EraserButton.png", 115f);
        return buttonSprite;
    }
}