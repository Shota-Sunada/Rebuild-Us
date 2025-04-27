using HarmonyLib;
using RebuildUs.Extensions;
using RebuildUs.Roles.RoleBase;
using UnityEngine;

namespace RebuildUs.Roles.Crewmate;

[HarmonyPatch]
[RoleInfo(nameof(Sheriff), "SheriffIntro", "SheriffShort", "SheriffFull", RoleType.Crewmate, RoleId.Sheriff, 248, 205, 70)]
internal class Sheriff : ModRole
{
    // GLOBAL
    internal static float KillCooldown { get { return CustomOptionHolders.SheriffKillCooldown.GetFloat(); } }
    internal static int MaxShots { get { return Mathf.RoundToInt(CustomOptionHolders.SheriffMaxShots.GetFloat()); } }
    internal static bool CanKillNeutrals { get { return CustomOptionHolders.SheriffCanKillNeutrals.GetBool(); } }
    internal static bool KillTargetOnMisfire { get { return CustomOptionHolders.SheriffKillTargetOnMisfire.GetBool(); } }
    internal static bool CanKillMadmate { get { return CustomOptionHolders.SheriffCanKillMadmate.GetBool(); } }

    internal int remainShots = 0;
    internal PlayerControl currentTarget;

    public Sheriff()
    {
        remainShots = MaxShots;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate()
    {
        if (player == PlayerControl.LocalPlayer && remainShots > 0)
        {
            currentTarget = TargetExtensions.SetTarget();
            TargetExtensions.SetPlayerOutline(currentTarget, RolesManager.AllRoles[RoleId.Sheriff].Color);
        }
    }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void OnRoleReset() { }
    internal override void MakeButtons(HudManager hm) { }
    internal override void SetButtonCooldowns() { }
}