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
    private static List<RoleId> blockLovers = [];

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

        var roleData = getRoleAssignmentData();
        assignSpecialRoles(roleData); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
        selectFactionForFactionIndependentRoles(roleData);
        assignEnsuredRoles(roleData); // Assign roles that should always be in the game next
        assignChanceRoles(roleData); // Assign roles that may or may not be in the game last
        assignRoleTargets(roleData); // Assign targets for Lawyer & Prosecutor

        // modifiers
        var modData = GetModifierAssignmentData();
        assignSpecialModifiers(modData);
        assignEnsuredModifiers(modData);
        assignChanceModifiers(modData);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoleOptionsCollectionV09), nameof(RoleOptionsCollectionV09.GetNumPerGame))]
    public static void GetNumPerGamePostfix(ref int __result)
    {
        // Deactivate Vanilla Roles if the mod roles are active
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) __result = 0;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    public static void GetAdjustedNumImpostorsPostfix(ref int __result)
    {
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
        {
            // Ignore Vanilla impostor limits in RU Games.
            __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LegacyGameOptions), nameof(LegacyGameOptions.Validate))]
    public static void ValidatePostfix(LegacyGameOptions __instance)
    {
        __instance.NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static void SelectRolesPostfix()
    {
        using var writer = RPCProcedure.SendRPC(CustomRPC.ResetVariables);
        RPCProcedure.resetVariables();

        // Don't assign Roles in Hide N Seek
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        assignRoles();
    }
}