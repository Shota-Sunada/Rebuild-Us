using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Seer : RoleBase<Seer>
{
    public static Color Color = new Color32(97, 178, 108, byte.MaxValue);
    public static List<Vector3> deadBodyPositions = [];

    public static int mode { get { return CustomOptionHolder.seerMode.getSelection(); } }
    public static bool limitSoulDuration { get { return CustomOptionHolder.seerLimitSoulDuration.getBool(); } }
    public static float soulDuration { get { return CustomOptionHolder.seerSoulDuration.getFloat(); } }

    public Seer()
    {
        baseRoleId = roleId = RoleId.Seer;
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

    private static Sprite soulSprite;
    public static Sprite getSoulSprite()
    {
        if (soulSprite) return soulSprite;
        soulSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.Soul.png", 500f);
        return soulSprite;
    }

    public override void Clear()
    {
        players = [];
        deadBodyPositions = [];
    }
}