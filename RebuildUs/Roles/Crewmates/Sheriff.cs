using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;
using TMPro;

namespace RebuildUs.Roles.Crewmate;

[HarmonyPatch]
public class Sheriff : RoleBase<Sheriff>
{
    private static CustomButton sheriffKillButton;
    public static TMP_Text sheriffNumShotsText;

    public static Color color = RebuildPalette.SheriffYellow;

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