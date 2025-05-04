using System.Linq;
using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class TimeMaster : RoleBase<TimeMaster>
{
    public static Color Color = new Color32(112, 142, 239, byte.MaxValue);
    private static CustomButton timeMasterShieldButton;

    public static float cooldown { get { return CustomOptionHolder.timeMasterCooldown.getFloat(); } }
    public static float rewindTime { get { return CustomOptionHolder.timeMasterRewindTime.getFloat(); } }
    public static float shieldDuration { get { return CustomOptionHolder.timeMasterShieldDuration.getFloat(); } }

    public static bool shieldActive = false;
    public static bool isRewinding = false;

    public TimeMaster()
    {
        baseRoleId = roleId = RoleId.TimeMaster;
        isRewinding = false;
        shieldActive = false;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        if (isRewinding)
        {
            if (GameHistory.localPlayerPositions.Count > 0)
            {
                // Set position
                var next = GameHistory.localPlayerPositions[0];
                if (next.Item2 == true)
                {
                    // Exit current vent if necessary
                    if (PlayerControl.LocalPlayer.inVent)
                    {
                        foreach (Vent vent in MapUtilities.CachedShipStatus.AllVents)
                        {
                            vent.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool couldUse);
                            if (canUse)
                            {
                                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(vent.Id);
                                vent.SetButtons(false);
                            }
                        }
                    }
                    // Set position
                    PlayerControl.LocalPlayer.transform.position = next.Item1;
                }
                else if (GameHistory.localPlayerPositions.Any(x => x.Item2 == true))
                {
                    PlayerControl.LocalPlayer.transform.position = next.Item1;
                }
                if (SubmergedCompatibility.IsSubmerged)
                {
                    SubmergedCompatibility.ChangeFloor(next.Item1.y > -7);
                }

                GameHistory.localPlayerPositions.RemoveAt(0);

                if (GameHistory.localPlayerPositions.Count > 1) GameHistory.localPlayerPositions.RemoveAt(0); // Skip every second position to rewind twice as fast, but never skip the last position
            }
            else
            {
                isRewinding = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        else
        {
            while (GameHistory.localPlayerPositions.Count >= Mathf.Round(rewindTime / Time.fixedDeltaTime)) GameHistory.localPlayerPositions.RemoveAt(GameHistory.localPlayerPositions.Count - 1);
            GameHistory.localPlayerPositions.Insert(0, new(PlayerControl.LocalPlayer.transform.position, PlayerControl.LocalPlayer.CanMove)); // CanMove = CanMove
        }
    }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        timeMasterShieldButton = new CustomButton(
            () =>
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.TimeMasterShield);
                RPCProcedure.timeMasterShield();
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.TimeMaster) && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                timeMasterShieldButton.isEffectActive = false;
                timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            getButtonSprite(),
            ButtonOffset.LowerRight,
            hm,
            hm.UseButton,
            KeyCode.F,
            true,
            shieldDuration,
            () =>
            {
                timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
            }
        );
    }
    public override void SetButtonCooldowns()
    {
        timeMasterShieldButton.MaxTimer = cooldown;
        timeMasterShieldButton.effectDuration = shieldDuration;
    }

    public static void resetTimeMasterButton()
    {
        timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
        timeMasterShieldButton.isEffectActive = false;
        timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    public override void Clear()
    {
        players = [];
    }

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.TimeShieldButton.png", 115f);
        return buttonSprite;
    }
}