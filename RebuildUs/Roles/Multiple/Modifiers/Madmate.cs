using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Madmate : ModifierBase<Madmate>
{
    public static Color Color = Palette.ImpostorRed;

    public Madmate()
    {
        baseModifierId = modifierId = ModifierId.Madmate;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

    public override void Clear()
    {
        players = [];
    }
}