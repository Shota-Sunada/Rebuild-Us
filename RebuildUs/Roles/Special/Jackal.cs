using System.Collections.Generic;
using System.Linq;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

public static class TeamJackal
{
    public static Color32 Color = new(0, 180, 235, 255);
    public static List<PlayerControl> formerJackals = [];

    public static CustomButton jackalKillButton;
    public static CustomButton sidekickKillButton;
    private static CustomButton jackalSidekickButton;
    public static CustomButton jackalAndSidekickSabotageLightsButton;

    public static void MakeButtons(HudManager hm)
    {
        // Jackal Sidekick Button
        jackalSidekickButton = new CustomButton(
            () =>
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.JackalCreatesSidekick);
                writer.Write(Jackal.currentTarget.PlayerId);
                RPCProcedure.jackalCreatesSidekick(Jackal.currentTarget.PlayerId);
            },
            () => { return Jackal.canCreateSidekick && Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return Jackal.canCreateSidekick && Jackal.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
            () => { jackalSidekickButton.Timer = jackalSidekickButton.MaxTimer; },
            Jackal.getSidekickButtonSprite(),
            ButtonOffset.LowerCenter,
            hm,
            hm.UseButton,
            KeyCode.F
        );

        // Jackal Kill
        jackalKillButton = new CustomButton(
            () =>
            {
                if (Helpers.checkMurderAttemptAndKill(Jackal.jackal, Jackal.currentTarget) == MurderAttemptResult.SuppressKill) return;

                jackalKillButton.Timer = jackalKillButton.MaxTimer;
                Jackal.currentTarget = null;
            },
            () => { return Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return Jackal.currentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { jackalKillButton.Timer = jackalKillButton.MaxTimer; },
            hm.KillButton.graphic.sprite,
            ButtonOffset.UpperRight,
            hm,
            hm.KillButton,
            KeyCode.Q
        );

        // Sidekick Kill
        sidekickKillButton = new CustomButton(
            () =>
            {
                if (Helpers.checkMurderAttemptAndKill(Sidekick.sidekick, Sidekick.currentTarget) == MurderAttemptResult.SuppressKill) return;
                sidekickKillButton.Timer = sidekickKillButton.MaxTimer;
                Sidekick.currentTarget = null;
            },
            () => { return Sidekick.canKill && Sidekick.sidekick != null && Sidekick.sidekick == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return Sidekick.currentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { sidekickKillButton.Timer = sidekickKillButton.MaxTimer; },
            hm.KillButton.graphic.sprite,
            ButtonOffset.UpperRight,
            hm,
            hm.KillButton,
            KeyCode.Q
        );

        jackalAndSidekickSabotageLightsButton = new CustomButton(
            () =>
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)SystemTypes.Electrical);
            },
            () =>
            {
                return (Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer && Jackal.canSabotageLights ||
                        Sidekick.sidekick != null && Sidekick.sidekick == PlayerControl.LocalPlayer && Sidekick.canSabotageLights) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () =>
            {
                if (Helpers.sabotageTimer() > jackalAndSidekickSabotageLightsButton.Timer || Helpers.sabotageActive())
                    jackalAndSidekickSabotageLightsButton.Timer = Helpers.sabotageTimer() + 5f;  // this will give imps time to do another sabotage.
                return Helpers.canUseSabotage();
            },
            () =>
            {
                jackalAndSidekickSabotageLightsButton.Timer = Helpers.sabotageTimer() + 5f;
            },
            Trickster.getLightsOutButtonSprite(),
            ButtonOffset.UpperCenter,
            hm,
            hm.SabotageButton,
            KeyCode.G
        );
    }

    public static void SetButtonCooldowns()
    {
        jackalKillButton.MaxTimer = Jackal.cooldown;
        sidekickKillButton.MaxTimer = Sidekick.cooldown;
        jackalSidekickButton.MaxTimer = Jackal.createSidekickCooldown;
    }

    public static class Jackal
    {
        public static PlayerControl jackal;
        public static PlayerControl fakeSidekick;
        public static PlayerControl currentTarget;

        public static float cooldown = 30f;
        public static float createSidekickCooldown = 30f;
        public static bool canUseVents = true;
        public static bool canCreateSidekick = true;
        public static bool jackalPromotedFromSidekickCanCreateSidekick = true;
        public static bool canCreateSidekickFromImpostor = true;
        public static bool hasImpostorVision = false;
        public static bool canSabotageLights = false;
        public static bool wasTeamRed;
        public static bool wasImpostor;
        public static bool wasSpy;

        public static Sprite buttonSprite;
        public static Sprite getSidekickButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.SidekickButton.png", 115f);
            return buttonSprite;
        }

        public static void removeCurrentJackal()
        {
            if (!formerJackals.Any(x => x.PlayerId == jackal.PlayerId)) formerJackals.Add(jackal);
            jackal = null;
            currentTarget = null;
            fakeSidekick = null;
        }

        public static void ClearAndReload()
        {
            jackal = null;
            currentTarget = null;
            fakeSidekick = null;
            formerJackals.Clear();
            wasTeamRed = wasImpostor = wasSpy = false;

            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
            canUseVents = CustomOptionHolder.jackalCanUseVents.getBool();
            canCreateSidekick = CustomOptionHolder.jackalCanCreateSidekick.getBool();
            jackalPromotedFromSidekickCanCreateSidekick = CustomOptionHolder.jackalPromotedFromSidekickCanCreateSidekick.getBool();
            canCreateSidekickFromImpostor = CustomOptionHolder.jackalCanCreateSidekickFromImpostor.getBool();
            hasImpostorVision = CustomOptionHolder.teamJackalHaveImpostorVision.getBool();
            canSabotageLights = CustomOptionHolder.jackalCanSabotageLights.getBool();
        }
    }

    public static class Sidekick
    {
        public static PlayerControl sidekick;
        public static PlayerControl currentTarget;

        public static bool wasTeamRed;
        public static bool wasImpostor;
        public static bool wasSpy;

        public static float cooldown => CustomOptionHolder.jackalKillCooldown.getFloat();
        public static bool canUseVents => CustomOptionHolder.sidekickCanUseVents.getBool();
        public static bool canKill => CustomOptionHolder.sidekickCanKill.getBool();
        public static bool promotesToJackal => CustomOptionHolder.sidekickPromotesToJackal.getBool();
        public static bool hasImpostorVision => CustomOptionHolder.teamJackalHaveImpostorVision.getBool();
        public static bool canSabotageLights => CustomOptionHolder.sidekickCanSabotageLights.getBool();

        public static void ClearAndReload()
        {
            sidekick = null;
            currentTarget = null;
            wasTeamRed = wasImpostor = wasSpy = false;
        }
    }
}