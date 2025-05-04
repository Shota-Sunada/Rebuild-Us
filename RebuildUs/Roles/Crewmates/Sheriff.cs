using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;
using TMPro;
using RebuildUs.Localization;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Sheriff : RoleBase<Sheriff>
{
    public static CustomButton sheriffKillButton;
    public static TMP_Text sheriffNumShotsText;

    public static Color Color = new Color32(248, 205, 70, byte.MaxValue);

    public static float cooldown { get { return CustomOptionHolder.sheriffCooldown.getFloat(); } }
    public static int maxShots { get { return Mathf.RoundToInt(CustomOptionHolder.sheriffNumShots.getFloat()); } }
    public static bool canKillNeutrals { get { return CustomOptionHolder.sheriffCanKillNeutrals.getBool(); } }
    public static bool misfireKillsTarget { get { return CustomOptionHolder.sheriffMisfireKillsTarget.getBool(); } }
    public static bool spyCanDieToSheriff { get { return CustomOptionHolder.spyCanDieToSheriff.getBool(); } }
    // public static bool madmateCanDieToSheriff { get { return CustomOptionHolder.madmateCanDieToSheriff.getBool(); } }

    public int numShots = 2;
    public PlayerControl currentTarget;

    public Sheriff()
    {
        baseRoleId = roleId = RoleId.Sheriff;
        numShots = maxShots;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        sheriffKillButton = new CustomButton(
            () =>
            {
                if (numShots <= 0)
                {
                    return;
                }

                var murderAttemptResult = Helpers.checkMurderAttempt(PlayerControl.LocalPlayer, currentTarget);
                if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                if (murderAttemptResult == MurderAttemptResult.PerformKill)
                {
                    bool misfire = false;
                    byte targetId = currentTarget.PlayerId;

                    misfire = (!currentTarget.Data.Role.IsImpostor || currentTarget == Mini.mini && !Mini.isGrownUp()) &&
                            (!spyCanDieToSheriff || Spy.spy != currentTarget) &&
                            // (madmateCanDieToSheriff && currentTarget.hasModifier(ModifierType.Madmate)) ||
                            (!canKillNeutrals || !currentTarget.isNeutral()) &&
                            (Jackal.jackal != currentTarget && Sidekick.sidekick != currentTarget);

                    // Armored sheriff shot doesn't kill if backfired
                    if (targetId == PlayerControl.LocalPlayer.PlayerId && Helpers.checkArmored(PlayerControl.LocalPlayer, true, true))
                    {
                        return;
                    }

                    using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedMurderPlayer);
                    writer.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                    writer.Write(targetId);
                    writer.Write(byte.MaxValue);
                    RPCProcedure.uncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, targetId, byte.MaxValue);
                }

                sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                currentTarget = null;
            },
            () => { return PlayerControl.LocalPlayer.isRole(RoleId.Sheriff) && numShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                if (sheriffNumShotsText != null)
                {
                    sheriffNumShotsText.text = numShots > 0 ? string.Format(Tr.Get("SheriffShots"), numShots) : "";
                }

                return currentTarget && PlayerControl.LocalPlayer.CanMove;
            },
            () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer; },
            hm.KillButton.graphic.sprite,
            ButtonOffset.UpperRight,
            hm,
            hm.KillButton,
            KeyCode.Q
        );

        sheriffNumShotsText = Object.Instantiate(sheriffKillButton.actionButton.cooldownTimerText, sheriffKillButton.actionButton.cooldownTimerText.transform.parent);
        sheriffNumShotsText.text = "";
        sheriffNumShotsText.enableWordWrapping = false;
        sheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
        sheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }

    public override void SetButtonCooldowns()
    {
        sheriffKillButton.MaxTimer = cooldown;
    }

    public override void Clear()
    {
        players = [];
    }
}