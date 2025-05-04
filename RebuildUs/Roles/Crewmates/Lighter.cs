using System.Linq;
using HarmonyLib;
using RebuildUs.Localization;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Lighter : RoleBase<Lighter>
{
    public static Color Color = new Color32(238, 229, 190, byte.MaxValue);

    public static float modeLightsOnVision { get { return CustomOptionHolder.lighterModeLightsOnVision.getFloat(); } }
    public static float modeLightsOffVision { get { return CustomOptionHolder.lighterModeLightsOffVision.getFloat(); } }
    public static float flashlightWidth { get { return CustomOptionHolder.lighterFlashlightWidth.getFloat(); } }

    public Lighter()
    {
        baseRoleId = roleId = RoleId.NoRole;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)    {    }
    public override void SetButtonCooldowns()    {    }

    public override void Clear()
    {
        players = [];
    }
}