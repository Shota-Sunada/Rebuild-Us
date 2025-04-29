using HarmonyLib;
using RebuildUs.Extensions;
using UnityEngine;

namespace RebuildUs.Roles.Crewmate;

[HarmonyPatch]
public class Sheriff : RoleBase<Sheriff>
{
    // GLOBAL
    public static float KillCooldown { get { return CustomOptionHolders.SheriffKillCooldown.GetFloat(); } }
    public static int MaxShots { get { return Mathf.RoundToInt(CustomOptionHolders.SheriffMaxShots.GetFloat()); } }
    public static bool CanKillNeutrals { get { return CustomOptionHolders.SheriffCanKillNeutrals.GetBool(); } }
    public static bool KillTargetOnMisfire { get { return CustomOptionHolders.SheriffKillTargetOnMisfire.GetBool(); } }
    public static bool CanKillMadmate { get { return CustomOptionHolders.SheriffCanKillMadmate.GetBool(); } }

    public int remainShots = 0;
    public PlayerControl currentTarget;

    public Sheriff() : base(RoleId.Sheriff)
    {
        remainShots = MaxShots;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        if (player == PlayerControl.LocalPlayer && remainShots > 0)
        {
            currentTarget = TargetExtensions.SetTarget();
            TargetExtensions.SetPlayerOutline(currentTarget, Colors.SheriffYellow);
        }
    }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void OnRoleReset() { }
    public override void MakeButtons(HudManager hm) { }
    public override void SetButtonCooldowns() { }
}