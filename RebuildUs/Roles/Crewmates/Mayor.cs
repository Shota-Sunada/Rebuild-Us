using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Mayor : RoleBase<Mayor>
{
    public static Color Color = new Color32(32, 77, 66, byte.MaxValue);

    public static int numVotes { get { return CustomOptionHolder.mayorNumVotes.getInt(); } }
    public static bool canSeeVoteColors { get { return CustomOptionHolder.mayorCanSeeVoteColors.getBool(); } }
    public static int tasksNeededToSeeVoteColors { get { return CustomOptionHolder.mayorTasksNeededToSeeVoteColors.getInt(); } }
    public static bool meetingButton { get { return CustomOptionHolder.mayorMeetingButton.getBool(); } }
    public static int maxRemoteMeetings { get { return CustomOptionHolder.mayorMaxRemoteMeetings.getInt(); } }

    public static Minigame emergency = null;
    public static Sprite emergencySprite = null;
    public static CustomButton mayorMeetingButton;

    public int remoteMeetingsLeft = 1;

    public Mayor()
    {
        baseRoleId = roleId = RoleId.Mayor;
        remoteMeetingsLeft = maxRemoteMeetings;
    }

    public static Sprite getMeetingSprite()
    {
        if (emergencySprite) return emergencySprite;
        emergencySprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.EmergencyButton.png", 550f);
        return emergencySprite;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate() { }
    public override void HudUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        mayorMeetingButton = new CustomButton(
            () =>
            {
                PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement
                remoteMeetingsLeft--;
                Helpers.handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                RPCProcedure.uncheckedCmdReportDeadBody(PlayerControl.LocalPlayer.PlayerId, byte.MaxValue);

                using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedCmdReportDeadBody);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(byte.MaxValue);
                mayorMeetingButton.Timer = 1f;
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Mayor) && !PlayerControl.LocalPlayer.Data.IsDead && meetingButton; },
            () =>
            {
                mayorMeetingButton.actionButton.OverrideText("Emergency (" + remoteMeetingsLeft + ")");
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                {
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles
                        || SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                    {
                        sabotageActive = true;
                    }
                }

                return !sabotageActive && PlayerControl.LocalPlayer.CanMove && (remoteMeetingsLeft > 0);
            },
            () => { mayorMeetingButton.Timer = mayorMeetingButton.MaxTimer; },
            getMeetingSprite(),
            ButtonOffset.LowerRight,
            hm,
            hm.UseButton,
            KeyCode.F,
            true,
            0f,
            () => { },
            false,
            "Meeting"
        );
    }
    public override void SetButtonCooldowns()
    {
        mayorMeetingButton.MaxTimer = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
    }

    public override void Clear()
    {
        players = [];
    }
}