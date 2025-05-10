using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Engineer : RoleBase<Engineer>
{
    public static Color Color = new Color32(0, 40, 245, byte.MaxValue);

    private static CustomButton engineerRepairButton;
    private static Sprite buttonSprite;

    public static int numberOfFixes => CustomOptionHolder.engineerNumberOfFixes.getInt();
    public static bool highlightForImpostors => CustomOptionHolder.engineerHighlightForImpostors.getBool();
    public static bool highlightForTeamJackal => CustomOptionHolder.engineerHighlightForTeamJackal.getBool();

    public int remainingFixes = 1;

    public Engineer()
    {
        baseRoleId = roleId = RoleId.Engineer;
        remainingFixes = numberOfFixes;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate() { }
    public override void HudUpdate() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        engineerRepairButton = new CustomButton(
            () =>
            {
                engineerRepairButton.Timer = 0f;

                using var rpc1 = RPCProcedure.SendRPC(CustomRPC.EngineerUsedRepair);
                rpc1.Write(PlayerControl.LocalPlayer.PlayerId);
                RPCProcedure.engineerUsedRepair(PlayerControl.LocalPlayer.PlayerId);

                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                {
                    if (task.TaskType == TaskTypes.FixLights)
                    {
                        using var rpc2 = RPCProcedure.SendRPC(CustomRPC.EngineerFixLights);
                        RPCProcedure.engineerFixLights();
                    }
                    else if (task.TaskType == TaskTypes.RestoreOxy)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    }
                    else if (task.TaskType == TaskTypes.ResetReactor)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                    }
                    else if (task.TaskType == TaskTypes.ResetSeismic)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    }
                    else if (task.TaskType == TaskTypes.FixComms)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    }
                    else if (task.TaskType == TaskTypes.StopCharles)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                    }
                    else if (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                    {
                        using var rpc3 = RPCProcedure.SendRPC(CustomRPC.EngineerFixSubmergedOxygen);
                        RPCProcedure.engineerFixSubmergedOxygen();
                    }
                }
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Engineer) && remainingFixes > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                bool sabotageActive = false;
                foreach (var task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                {
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles
                        || SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                    {
                        sabotageActive = true;
                    }
                }

                return sabotageActive && remainingFixes > 0 && PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            getButtonSprite(),
            ButtonOffset.UpperRight,
            hm,
            hm.UseButton,
            KeyCode.F
        );
    }
    public override void SetButtonCooldowns()
    {
        engineerRepairButton.MaxTimer = 0f;
    }

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.RepairButton.png", 115f);
        return buttonSprite;
    }

    public override void Clear()
    {
        players = [];
    }
}