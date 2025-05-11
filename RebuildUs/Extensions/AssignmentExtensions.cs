using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using AmongUs.GameOptions;

using RebuildUs.Utilities;
using static RebuildUs.RebuildUs;
using RebuildUs.CustomGameModes;
using RebuildUs.Modules;

namespace RebuildUs.Extensions;

public static partial class AssignmentExtensions
{

    [HarmonyPatch(typeof(RoleOptionsCollectionV08), nameof(RoleOptionsCollectionV08.GetNumPerGame))]
    class RoleOptionsDataGetNumPerGamePatch
    {
        public static void Postfix(ref int __result)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    class GameOptionsDataGetAdjustedNumImpostorsPatch
    {
        public static void Postfix(ref int __result)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
            {  // Ignore Vanilla impostor limits in RU Games.
                __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
            }
        }
    }

    [HarmonyPatch(typeof(LegacyGameOptions), nameof(LegacyGameOptions.Validate))]
    class GameOptionsDataValidatePatch
    {
        public static void Postfix(LegacyGameOptions __instance)
        {
            __instance.NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {
        private static List<RoleId> blockLovers = [];

        private static bool isEvilGuesser;
        public static void Postfix()
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.ResetVariables);
            RPCProcedure.resetVariables();

            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return; // Don't assign Roles in Hide N Seek
            assignRoles();
        }

        private static void assignRoles()
        {
            blockLovers = [RoleId.Bait];

            if (!Lovers.hasTasks)
            {
                blockLovers.Add(RoleId.Snitch);
            }

            if (!CustomOptionHolder.arsonistCanBeLovers.getBool())
            {
                blockLovers.Add(RoleId.Arsonist);
            }

            var data = getRoleAssignmentData();
            assignSpecialRoles(data); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
            selectFactionForFactionIndependentRoles(data);
            assignEnsuredRoles(data); // Assign roles that should always be in the game next
            assignChanceRoles(data); // Assign roles that may or may not be in the game last
            assignRoleTargets(data); // Assign targets for Lawyer & Prosecutor
            assignSpecialModifiers(data);
            assignModifiers(data); // Assign modifier
        }

        private static byte setModifierToRandomPlayer(ModifierId modifierId, List<PlayerControl> playerList)
        {
            if (playerList.Count <= 0)
            {
                return byte.MaxValue;
            }

            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;
            playerList.RemoveAt(index);

            using var writer = RPCProcedure.SendRPC(CustomRPC.AddModifier);
            writer.Write((byte)modifierId);
            writer.Write(playerId);
            RPCProcedure.addModifier((byte)modifierId, playerId);

            return playerId;
        }
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