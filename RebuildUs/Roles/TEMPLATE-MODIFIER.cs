// using HarmonyLib;
// using UnityEngine;

// namespace RebuildUs.Roles;

// [HarmonyPatch]
// public class Template : ModifierBase<Template>
// {
//     public static Color Color = Palette.CrewmateBlue;

//     public Template()
//     {
//         baseModifierId = modifierId = ModifierId.NoModifier;
//     }

//     public override void OnMeetingStart() { }
//     public override void OnMeetingEnd() { }
//     public override void FixedUpdate() { }
//     public override void OnKill(PlayerControl target) { }
//     public override void OnDeath(PlayerControl killer = null) { }
//     public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

//     public override void Clear()
//     {
//         players = [];
//     }
// }