using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Detective : RoleBase<Detective>
{
    public static Color Color = new Color32(45, 106, 165, byte.MaxValue);

    public static bool anonymousFootprints { get { return CustomOptionHolder.detectiveAnonymousFootprints.getBool(); } }
    public static float footprintInterval { get { return CustomOptionHolder.detectiveFootprintInterval.getFloat(); } }
    public static float footprintDuration { get { return CustomOptionHolder.detectiveFootprintDuration.getFloat(); } }
    public static float reportNameDuration { get { return CustomOptionHolder.detectiveReportNameDuration.getFloat(); } }
    public static float reportColorDuration { get { return CustomOptionHolder.detectiveReportColorDuration.getFloat(); } }

    public float timer = 6.2f;

    public Detective()
    {
        baseRoleId = roleId = RoleId.Detective;
        timer = 6.2f;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Detective))
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f)
            {
                timer = footprintInterval;
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent)
                    {
                        FootprintHolder.Instance.MakeFootprint(player);
                    }
                }
            }
        }
    }
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