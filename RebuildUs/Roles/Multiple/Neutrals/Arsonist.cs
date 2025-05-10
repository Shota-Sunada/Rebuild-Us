using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;
using static RebuildUs.Patches.PlayerControlFixedUpdatePatch;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Arsonist : RoleBase<Arsonist>
{
    public static Color Color = new Color32(238, 112, 46, byte.MaxValue);
    public static List<PlayerControl> dousedPlayers = [];

    public static float cooldown { get { return CustomOptionHolder.arsonistCooldown.getFloat(); } }
    public static float duration { get { return CustomOptionHolder.arsonistDuration.getFloat(); } }
    public static bool canBeLovers { get { return CustomOptionHolder.arsonistCanBeLovers.getBool(); } }

    public static bool triggerArsonistWin = false;

    public PlayerControl currentTarget;
    public PlayerControl douseTarget;

    public static CustomButton arsonistButton;

    public Arsonist()
    {
        baseRoleId = roleId = RoleId.Arsonist;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        List<PlayerControl> untargetablePlayers;
        if (douseTarget != null)
        {
            untargetablePlayers = [];
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId != douseTarget.PlayerId)
                {
                    untargetablePlayers.Add(player);
                }
            }
        }
        else
        {
            untargetablePlayers = dousedPlayers;
        }
        currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        if (currentTarget != null) setPlayerOutline(currentTarget, Color);
    }
    public override void HudUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        arsonistButton = new CustomButton(
            () =>
            {
                if (dousedEveryoneAlive())
                {
                    using var winWriter = RPCProcedure.SendRPC(CustomRPC.ArsonistWin);
                    RPCProcedure.arsonistWin();
                    arsonistButton.hasEffect = false;
                }
                else if (currentTarget != null)
                {
                    douseTarget = currentTarget;
                    arsonistButton.hasEffect = true;
                }
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Arsonist) && !PlayerControl.LocalPlayer.isDead(); },
            () =>
            {
                if (dousedEveryoneAlive()) arsonistButton.actionButton.graphic.sprite = getIgniteSprite();

                if (arsonistButton.isEffectActive && douseTarget != currentTarget)
                {
                    douseTarget = null;
                    arsonistButton.Timer = 0f;
                    arsonistButton.isEffectActive = false;
                }

                return PlayerControl.LocalPlayer.CanMove && (dousedEveryoneAlive() || currentTarget != null);
            },
            () =>
            {
                arsonistButton.Timer = arsonistButton.MaxTimer;
                arsonistButton.isEffectActive = false;
                douseTarget = null;
            },
            getDouseSprite(),
            ButtonOffset.LowerRight,
            hm,
            hm.KillButton,
            KeyCode.F,
            true,
            duration,
            () =>
            {
                if (douseTarget != null) dousedPlayers.Add(douseTarget);

                arsonistButton.Timer = dousedEveryoneAlive() ? 0 : arsonistButton.MaxTimer;

                foreach (var p in dousedPlayers)
                {
                    if (MapOptions.playerIcons.ContainsKey(p.PlayerId))
                    {
                        MapOptions.playerIcons[p.PlayerId].setSemiTransparent(false);
                    }
                }

                // Ghost Info
                using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write((byte)GhostInfoTypes.ArsonistDouse);
                writer.Write(douseTarget.PlayerId);

                douseTarget = null;
            }
        );
    }
    public override void SetButtonCooldowns()
    {
        arsonistButton.MaxTimer = cooldown;
        arsonistButton.effectDuration = duration;
    }

    public static bool dousedEveryoneAlive()
    {
        return PlayerControl.AllPlayerControls.GetFastEnumerator().ToArray().All(x => { return x.isRole(RoleId.Arsonist) || x.isDead() || dousedPlayers.Any(y => y.PlayerId == x.PlayerId); });
    }

    public override void Clear()
    {
        players = [];
        dousedPlayers = [];
        triggerArsonistWin = false;
        foreach (PoolablePlayer p in MapOptions.playerIcons.Values)
        {
            if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
        }
    }

    private static Sprite douseSprite;
    public static Sprite getDouseSprite()
    {
        if (douseSprite) return douseSprite;
        douseSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.DouseButton.png", 115f);
        return douseSprite;
    }

    private static Sprite igniteSprite;
    public static Sprite getIgniteSprite()
    {
        if (igniteSprite) return igniteSprite;
        igniteSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.IgniteButton.png", 115f);
        return igniteSprite;
    }
}