using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Jester : RoleBase<Jester>
{
    public static Color Color = new Color32(236, 98, 165, byte.MaxValue);

    public static bool triggerJesterWin = false;
    public bool isWin = false;

    public static bool canCallEmergency => CustomOptionHolder.jesterCanCallEmergency.getBool();
    public static bool canSabotage => CustomOptionHolder.jesterCanSabotage.getBool();
    public static bool hasImpostorVision => CustomOptionHolder.jesterHasImpostorVision.getBool();
    public static bool jesterWinEveryone => CustomOptionHolder.jesterWinEveryone.getBool();

    public Jester()
    {
        baseRoleId = roleId = RoleId.Jester;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate() { }
    public override void HudUpdate() { }
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