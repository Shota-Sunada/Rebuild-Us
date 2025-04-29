using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Roles.Crewmate;

[HarmonyPatch]
public class Sheriff : RoleBase<Sheriff>
{
    public static Color Color = RebuildPalette.SheriffYellow;

    public Sheriff()
    {
        baseRoleId = roleId = RoleId.Sheriff;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm) { }
    public override void SetButtonCooldowns() { }

    public override void Clear()
    {
        players = [];
    }
}