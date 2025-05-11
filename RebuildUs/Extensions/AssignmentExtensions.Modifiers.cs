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
    private static ModifierAssignmentData GetModifierAssignmentData()
    {
        var modifierMin = CustomOptionHolder.modifiersCountMin.getSelection();
        var modifierMax = CustomOptionHolder.modifiersCountMax.getSelection();

        if (modifierMin > modifierMax) modifierMin = modifierMax;

        int modCountSettings = rnd.Next(modifierMin, modifierMax + 1);

        int maxModifiers = Mathf.Min(PlayerControl.AllPlayerControls.Count, modCountSettings);

        Dictionary<ModifierId, (int rate, int count)> modifierSettings = [];

        return new()
        {
            players = [.. PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid())],
            modifierSettings = modifierSettings,
            maxModifiers = maxModifiers,
        };
    }

    private static void assignSpecialModifiers(ModifierAssignmentData data)
    {
        // Madmate
    }

    public static void assignEnsuredModifiers(ModifierAssignmentData data)
    {
        var ensuredModifiers = new List<ModifierId>(data.modifierSettings.Where(x => x.Value.rate == 10).Select(x => x.Key));

        while (data.players.Count > 0 && data.maxModifiers > 0)
        {
            if (ensuredModifiers.Count > 0)
            {
                var index = rnd.Next(0, ensuredModifiers.Count);
                var modId = ensuredModifiers[index];
                setModifierToRandomPlayer(modId, data.players);
                ensuredModifiers.RemoveAt(index);

                data.maxModifiers--;
            }
        }
    }

    public static void assignChanceModifiers(ModifierAssignmentData data)
    {
        var modifierTickets = new List<ModifierId>(data.modifierSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x));

        while (data.players.Count > 0 && data.maxModifiers > 0)
        {
            if (modifierTickets.Count > 0)
            {
                var index = rnd.Next(0, modifierTickets.Count);
                var modId = modifierTickets[index];
                setModifierToRandomPlayer(modId, data.players);
                modifierTickets.RemoveAll(x => x == modId);

                data.maxModifiers--;
            }
        }
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

    public class ModifierAssignmentData
    {
        public List<PlayerControl> players { get; set; }
        public Dictionary<ModifierId, (int rate, int count)> modifierSettings = [];
        public int maxModifiers { get; set; }
    }
}