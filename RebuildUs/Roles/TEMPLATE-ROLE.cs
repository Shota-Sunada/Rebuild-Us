// using HarmonyLib;
// using UnityEngine;

// namespace RebuildUs.Roles;

// [HarmonyPatch]
// public class Template : RoleBase<Template>
// {
//     public static Color Color = Palette.CrewmateBlue;

//     public Template()
//     {
//         baseRoleId = roleId = RoleId.NoRole;
//     }

//     public override void OnMeetingStart() { }
//     public override void OnMeetingEnd() { }
//     public override void FixedUpdate() { }
// public override void HudUpdate() { }
//     public override void OnKill(PlayerControl target) { }
//     public override void OnDeath(PlayerControl killer = null) { }
//     public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
//     public override void MakeButtons(HudManager hm) { }
//     public override void SetButtonCooldowns() { }

//     public override void Clear()
//     {
//         players = [];
//     }
// }