using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static RebuildUs.RebuildUs;

namespace RebuildUs.Extensions;

public static partial class AssignmentExtensions
{
    public static RoleAssignmentData getRoleAssignmentData()
    {
        // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
        List<PlayerControl> crewmates = [.. PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid())];
        crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
        List<PlayerControl> impostors = [.. PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid())];
        impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

        var crewmateMin = CustomOptionHolder.crewmateRolesCountMin.getSelection();
        var crewmateMax = CustomOptionHolder.crewmateRolesCountMax.getSelection();
        var neutralMin = CustomOptionHolder.neutralRolesCountMin.getSelection();
        var neutralMax = CustomOptionHolder.neutralRolesCountMax.getSelection();
        var impostorMin = CustomOptionHolder.impostorRolesCountMin.getSelection();
        var impostorMax = CustomOptionHolder.impostorRolesCountMax.getSelection();

        // Make sure min is less or equal to max
        if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
        if (neutralMin > neutralMax) neutralMin = neutralMax;
        if (impostorMin > impostorMax) impostorMin = impostorMax;

        // Automatically force everyone to get a role by setting crew Min / Max according to Neutral Settings
        if (CustomOptionHolder.crewmateRolesFill.getBool())
        {
            crewmateMax = crewmates.Count - neutralMin;
            crewmateMin = crewmates.Count - neutralMax;
        }

        // Get the maximum allowed count of each role type based on the minimum and maximum option
        int crewCountSettings = rnd.Next(crewmateMin, crewmateMax + 1);
        int neutralCountSettings = rnd.Next(neutralMin, neutralMax + 1);
        int impCountSettings = rnd.Next(impostorMin, impostorMax + 1);
        // If fill crewmates is enabled, make sure crew + neutral >= crewmates s.t. everyone has a role!
        while (crewCountSettings + neutralCountSettings < crewmates.Count && CustomOptionHolder.crewmateRolesFill.getBool())
        {
            crewCountSettings++;
        }

        // Potentially lower the actual maximum to the assignable players
        int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
        int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
        int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

        // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
        Dictionary<RoleId, (int rate, int count)> impSettings = [];
        Dictionary<RoleId, (int rate, int count)> neutralSettings = [];
        Dictionary<RoleId, (int rate, int count)> crewSettings = [];

        impSettings.Add(RoleId.Morphing, CustomOptionHolder.morphingSpawnRate.Data);
        impSettings.Add(RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.Data);
        impSettings.Add(RoleId.Vampire, CustomOptionHolder.vampireSpawnRate.Data);
        impSettings.Add(RoleId.Eraser, CustomOptionHolder.eraserSpawnRate.Data);
        impSettings.Add(RoleId.Trickster, CustomOptionHolder.tricksterSpawnRate.Data);
        impSettings.Add(RoleId.Cleaner, CustomOptionHolder.cleanerSpawnRate.Data);
        impSettings.Add(RoleId.Warlock, CustomOptionHolder.warlockSpawnRate.Data);
        impSettings.Add(RoleId.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.Data);
        impSettings.Add(RoleId.Witch, CustomOptionHolder.witchSpawnRate.Data);
        impSettings.Add(RoleId.Ninja, CustomOptionHolder.ninjaSpawnRate.Data);
        impSettings.Add(RoleId.Bomber, CustomOptionHolder.bomberSpawnRate.Data);
        impSettings.Add(RoleId.Yoyo, CustomOptionHolder.yoyoSpawnRate.Data);

        neutralSettings.Add(RoleId.Jester, CustomOptionHolder.jesterSpawnRate.Data);
        neutralSettings.Add(RoleId.Arsonist, CustomOptionHolder.arsonistSpawnRate.Data);
        neutralSettings.Add(RoleId.Jackal, CustomOptionHolder.jackalSpawnRate.Data);
        neutralSettings.Add(RoleId.Vulture, CustomOptionHolder.vultureSpawnRate.Data);
        neutralSettings.Add(RoleId.Thief, CustomOptionHolder.thiefSpawnRate.Data);

        if (rnd.Next(1, 101) <= CustomOptionHolder.lawyerIsProsecutorChance.getSelection() * 10) // Lawyer or Prosecutor
        {
            neutralSettings.Add(RoleId.Prosecutor, CustomOptionHolder.lawyerSpawnRate.Data);
        }
        else
        {
            neutralSettings.Add(RoleId.Lawyer, CustomOptionHolder.lawyerSpawnRate.Data);
        }

        crewSettings.Add(RoleId.Mayor, CustomOptionHolder.mayorSpawnRate.Data);
        crewSettings.Add(RoleId.Portalmaker, CustomOptionHolder.portalmakerSpawnRate.Data);
        crewSettings.Add(RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.Data);
        crewSettings.Add(RoleId.Lighter, CustomOptionHolder.lighterSpawnRate.Data);
        crewSettings.Add(RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.Data);
        crewSettings.Add(RoleId.TimeMaster, CustomOptionHolder.timeMasterSpawnRate.Data);
        crewSettings.Add(RoleId.Medic, CustomOptionHolder.medicSpawnRate.Data);
        crewSettings.Add(RoleId.Seer, CustomOptionHolder.seerSpawnRate.Data);
        crewSettings.Add(RoleId.Hacker, CustomOptionHolder.hackerSpawnRate.Data);
        crewSettings.Add(RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.Data);
        crewSettings.Add(RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.Data);
        crewSettings.Add(RoleId.Medium, CustomOptionHolder.mediumSpawnRate.Data);
        crewSettings.Add(RoleId.Trapper, CustomOptionHolder.trapperSpawnRate.Data);
        crewSettings.Add(RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.Data);
        if (impostors.Count > 1)
        {
            // Only add Spy if more than 1 impostor as the spy role is otherwise useless
            crewSettings.Add(RoleId.Spy, CustomOptionHolder.spySpawnRate.Data);
        }
        crewSettings.Add(RoleId.SecurityGuard, CustomOptionHolder.securityGuardSpawnRate.Data);

        return new RoleAssignmentData
        {
            crewmates = crewmates,
            impostors = impostors,
            crewSettings = crewSettings,
            neutralSettings = neutralSettings,
            impSettings = impSettings,
            maxCrewmateRoles = maxCrewmateRoles,
            maxNeutralRoles = maxNeutralRoles,
            maxImpostorRoles = maxImpostorRoles,
        };
    }

    private static void assignSpecialRoles(RoleAssignmentData data)
    {
        if (CustomOptionHolder.loversSpawnRate.Enabled)
        {
            for (int i = 0; i < CustomOptionHolder.loversNumCouples.getFloat(); i++)
            {
                var singleCrew = data.crewmates.FindAll(x => !x.isLovers());
                var singleImps = data.impostors.FindAll(x => !x.isLovers());

                bool isOnlyRole = !CustomOptionHolder.loversCanHaveAnotherRole.getBool();
                if (rnd.Next(1, 101) <= CustomOptionHolder.loversSpawnRate.getSelection() * 10)
                {
                    int lover1 = -1;
                    int lover2 = -1;
                    int lover1Index = -1;
                    int lover2Index = -1;
                    if (singleImps.Count > 0 && singleCrew.Count > 0 && (!isOnlyRole || (data.maxCrewmateRoles > 0 && data.maxImpostorRoles > 0)) && rnd.Next(1, 101) <= CustomOptionHolder.loversImpLoverRate.getSelection() * 10)
                    {
                        lover1Index = rnd.Next(0, singleImps.Count);
                        lover1 = singleImps[lover1Index].PlayerId;

                        lover2Index = rnd.Next(0, singleCrew.Count);
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.maxImpostorRoles--;
                            data.maxCrewmateRoles--;

                            data.impostors.RemoveAll(x => x.PlayerId == lover1);
                            data.crewmates.RemoveAll(x => x.PlayerId == lover2);
                        }
                    }

                    else if (singleCrew.Count >= 2 && (isOnlyRole || data.maxCrewmateRoles >= 2))
                    {
                        lover1Index = rnd.Next(0, singleCrew.Count);
                        while (lover2Index == lover1Index || lover2Index < 0) lover2Index = rnd.Next(0, singleCrew.Count);

                        lover1 = singleCrew[lover1Index].PlayerId;
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.maxCrewmateRoles -= 2;
                            data.crewmates.RemoveAll(x => x.PlayerId == lover1);
                            data.crewmates.RemoveAll(x => x.PlayerId == lover2);
                        }
                    }

                    if (lover1 >= 0 && lover2 >= 0)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLovers, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)lover1);
                        writer.Write((byte)lover2);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setLovers((byte)lover1, (byte)lover2);
                    }
                }
            }
        }

        // Assign Mafia
        if (data.impostors.Count >= 3 && data.maxImpostorRoles >= 3 && (rnd.Next(1, 101) <= CustomOptionHolder.mafiaSpawnRate.getSelection() * 10))
        {
            setRoleToRandomPlayer(RoleId.Godfather, data.impostors);
            setRoleToRandomPlayer(RoleId.Janitor, data.impostors);
            setRoleToRandomPlayer(RoleId.Mafioso, data.impostors);
            data.maxImpostorRoles -= 3;
        }
    }

    private static void selectFactionForFactionIndependentRoles(RoleAssignmentData data)
    {
        // Assign Guesser (chance to be impostor based on setting)
        bool isEvilGuesser = rnd.Next(1, 101) <= CustomOptionHolder.guesserIsImpGuesserRate.getSelection() * 10;
        if (CustomOptionHolder.guesserSpawnBothRate.getSelection() > 0)
        {
            if (rnd.Next(1, 101) <= CustomOptionHolder.guesserSpawnRate.getSelection() * 10)
            {
                if (isEvilGuesser)
                {
                    if (data.impostors.Count > 0 && data.maxImpostorRoles > 0)
                    {
                        byte evilGuesser = setRoleToRandomPlayer(RoleId.EvilGuesser, data.impostors);
                        data.impostors.ToList().RemoveAll(x => x.PlayerId == evilGuesser);
                        data.maxImpostorRoles--;
                        data.crewSettings.Add(RoleId.NiceGuesser, (CustomOptionHolder.guesserSpawnBothRate.getSelection(), 1));
                    }
                }
                else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
                {
                    byte niceGuesser = setRoleToRandomPlayer(RoleId.NiceGuesser, data.crewmates);
                    data.crewmates.ToList().RemoveAll(x => x.PlayerId == niceGuesser);
                    data.maxCrewmateRoles--;
                    data.impSettings.Add(RoleId.EvilGuesser, (CustomOptionHolder.guesserSpawnBothRate.getSelection(), 1));
                }
            }
        }
        else
        {
            if (isEvilGuesser) data.impSettings.Add(RoleId.EvilGuesser, (CustomOptionHolder.guesserSpawnRate.getSelection(), 1));
            else data.crewSettings.Add(RoleId.NiceGuesser, (CustomOptionHolder.guesserSpawnRate.getSelection(), 1));
        }

        // Assign Swapper (chance to be impostor based on setting)
        if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && rnd.Next(1, 101) <= CustomOptionHolder.swapperIsImpRate.getSelection() * 10)
        {
            data.impSettings.Add(RoleId.Swapper, (CustomOptionHolder.swapperSpawnRate.getSelection(), 1));
        }
        else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
        {
            data.crewSettings.Add(RoleId.Swapper, (CustomOptionHolder.swapperSpawnRate.getSelection(), 1));
        }

        // Assign Shifter (chance to be neutral based on setting)
        bool shifterIsNeutral = false;
        if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && rnd.Next(1, 101) <= CustomOptionHolder.shifterIsNeutralRate.getSelection() * 10)
        {
            data.neutralSettings.Add(RoleId.Shifter, (CustomOptionHolder.shifterSpawnRate.getSelection(), 1));
            shifterIsNeutral = true;
        }
        else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
        {
            data.crewSettings.Add(RoleId.Shifter, (CustomOptionHolder.shifterSpawnRate.getSelection(), 1));
            shifterIsNeutral = false;
        }

        using var writer = RPCProcedure.SendRPC(CustomRPC.SetShifterType);
        writer.Write(shifterIsNeutral);
        RPCProcedure.setShifterType(shifterIsNeutral);
    }

    private static void assignEnsuredRoles(RoleAssignmentData data)
    {
        // Get all roles where the chance to occur is set to 100%
        List<RoleId> ensuredCrewmateRoles = [.. data.crewSettings.Where(x => x.Value.rate == 10).Select(x => x.Key)];
        List<RoleId> ensuredNeutralRoles = [.. data.neutralSettings.Where(x => x.Value.rate == 10).Select(x => x.Key)];
        List<RoleId> ensuredImpostorRoles = [.. data.impSettings.Where(x => x.Value.rate == 10).Select(x => x.Key)];

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while (
            (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) ||
            (data.crewmates.Count > 0 && (
                (data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) ||
                (data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
            )))
        {

            Dictionary<RoleType, List<RoleId>> rolesToAssign = [];
            if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
            if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

            // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role)
            // then select one of the roles from the selected pool to a player
            // and remove the role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
            var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral ? data.crewmates : data.impostors;
            var index = rnd.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
            rolesToAssign[roleType].RemoveAt(index);

            if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId))
            {
                foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId])
                {
                    // Set chance for the blocked roles to 0 for chances less than 100%
                    if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = (0, 0);
                    if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = (0, 0);
                    if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = (0, 0);
                    // Remove blocked roles even if the chance was 100%
                    foreach (var ensuredRolesList in rolesToAssign.Values)
                    {
                        ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                    }
                }
            }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate: data.maxCrewmateRoles--; break;
                case RoleType.Neutral: data.maxNeutralRoles--; break;
                case RoleType.Impostor: data.maxImpostorRoles--; break;
            }
        }
    }

    private static void assignChanceRoles(RoleAssignmentData data)
    {
        // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
        List<RoleId> crewmateTickets = [.. data.crewSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x)];
        List<RoleId> neutralTickets = [.. data.neutralSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x)];
        List<RoleId> impostorTickets = [.. data.impSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x)];

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while (
            (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) ||
            (data.crewmates.Count > 0 && (
                (data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                (data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
            )))
        {

            Dictionary<RoleType, List<RoleId>> rolesToAssign = [];
            if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
            if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(RoleType.Neutral, neutralTickets);
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(RoleType.Impostor, impostorTickets);

            // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role)
            // then select one of the roles from the selected pool to a player
            // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
            var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral ? data.crewmates : data.impostors;
            var index = rnd.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            setRoleToRandomPlayer(roleId, players);
            rolesToAssign[roleType].RemoveAll(x => x == roleId);

            if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId))
            {
                foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId])
                {
                    // Remove tickets of blocked roles from all pools
                    crewmateTickets.RemoveAll(x => x == blockedRoleId);
                    neutralTickets.RemoveAll(x => x == blockedRoleId);
                    impostorTickets.RemoveAll(x => x == blockedRoleId);
                }
            }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate: data.maxCrewmateRoles--; break;
                case RoleType.Neutral: data.maxNeutralRoles--; break;
                case RoleType.Impostor: data.maxImpostorRoles--; break;
            }
        }
    }

    public static void assignRoleTargets(RoleAssignmentData data)
    {
        // Set Lawyer or Prosecutor Target
        if (Lawyer.lawyer != null)
        {
            var possibleTargets = new List<PlayerControl>();
            if (!Lawyer.isProsecutor)
            { // Lawyer
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && !p.isLovers() && (p.Data.Role.IsImpostor || p == TeamJackal.Jackal.jackal || (Lawyer.targetCanBeJester && p.isRole(RoleId.Jester))))
                        possibleTargets.Add(p);
                }
            }
            else
            { // Prosecutor
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && !p.isLovers() && p != Mini.mini && !p.Data.Role.IsImpostor && !Helpers.isNeutral(p) && p != Swapper.swapper)
                        possibleTargets.Add(p);
                }
            }

            if (possibleTargets.Count == 0)
            {
                using var w = RPCProcedure.SendRPC(CustomRPC.LawyerPromotesToPursuer);
                RPCProcedure.lawyerPromotesToPursuer();
            }
            else
            {
                var target = possibleTargets[RebuildUs.rnd.Next(0, possibleTargets.Count)];

                using var writer = RPCProcedure.SendRPC(CustomRPC.LawyerSetTarget);
                writer.Write(target.PlayerId);
                RPCProcedure.lawyerSetTarget(target.PlayerId);
            }
        }
    }

    private static byte setRoleToRandomPlayer(RoleId roleId, List<PlayerControl> playerList)
    {
        if (playerList.Count <= 0)
        {
            return byte.MaxValue;
        }

        var index = rnd.Next(0, playerList.Count);
        byte playerId = playerList[index].PlayerId;

        if (Helpers.RolesEnabled &&
            CustomOptionHolder.loversSpawnRate.Enabled &&
            Helpers.playerById(playerId)?.isLovers() == true &&
            blockLovers.Contains(roleId))
        {
            return byte.MaxValue;
        }

        playerList.RemoveAt(index);

        using var writer = RPCProcedure.SendRPC(CustomRPC.SetRole);
        writer.Write((byte)roleId);
        writer.Write(playerId);
        RPCProcedure.setRole((byte)roleId, playerId);

        return playerId;
    }

    public class RoleAssignmentData
    {
        public List<PlayerControl> crewmates { get; set; }
        public List<PlayerControl> impostors { get; set; }
        public Dictionary<RoleId, (int rate, int count)> impSettings = [];
        public Dictionary<RoleId, (int rate, int count)> neutralSettings = [];
        public Dictionary<RoleId, (int rate, int count)> crewSettings = [];
        public int maxCrewmateRoles { get; set; }
        public int maxNeutralRoles { get; set; }
        public int maxImpostorRoles { get; set; }
    }
}