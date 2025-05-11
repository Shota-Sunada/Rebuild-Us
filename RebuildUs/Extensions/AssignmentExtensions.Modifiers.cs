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
    private static void assignSpecialModifiers(RoleAssignmentData data)
    {
        // Madmate
    }

    public static void assignModifiers(RoleAssignmentData data)
    {
        var modifierMin = CustomOptionHolder.modifiersCountMin.getSelection();
        var modifierMax = CustomOptionHolder.modifiersCountMax.getSelection();
        if (modifierMin > modifierMax) modifierMin = modifierMax;
        int modifierCountSettings = rnd.Next(modifierMin, modifierMax + 1);
        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        int modifierCount = Mathf.Min(players.Count, modifierCountSettings);

        if (modifierCount == 0) return;

        for (int i = 0; i < modifierCount; i++)
        {

        }
    }
}