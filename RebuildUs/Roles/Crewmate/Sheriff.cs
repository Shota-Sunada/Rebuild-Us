using HarmonyLib;
using RebuildUs.Roles.RoleBase;
using UnityEngine;

namespace RebuildUs.Roles.Crewmate;

[HarmonyPatch]
[RoleInfo("Sheriff", "SheriffIntro", "SheriffShort", "SheriffFull", RoleType.Crewmate, RoleId.Sheriff, 248, 205, 70)]
internal class Sheriff : ModRoleBase<Sheriff>
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
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void OnRoleReset() { }
}