using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RebuildUs;

internal enum CustomDeathReason
{
    Kill,
    Exile,
    Disconnect,
}

internal static class GameHistory
{
    internal static List<(Vector3, bool)> LocalPlayerPositions = [];
    internal static List<DeadPlayer> DeadPlayers = [];

    internal static void ClearGameHistory()
    {
        LocalPlayerPositions = [];
        DeadPlayers = [];
    }

    internal static void OverrideDeathReasonAndKiller(PlayerControl player, CustomDeathReason reason, PlayerControl killer = null)
    {
        var target = DeadPlayers.FirstOrDefault(x => x.Player.PlayerId == player.PlayerId);
        if (target != null)
        {
            target.Reason = reason;

            if (killer != null)
            {
                target.Killer = killer;
            }
        }
        else if (player != null)
        {
            DeadPlayers.Add(new DeadPlayer(player, DateTime.UtcNow, reason, killer));
        }
    }
}

internal class DeadPlayer
{
    internal PlayerControl Player;
    internal DateTime TimeOfDeath;
    internal CustomDeathReason Reason;
    internal PlayerControl Killer = null;
    internal bool WasCleaned = false;

    internal DeadPlayer(PlayerControl player, DateTime timeOfDeath, CustomDeathReason reason, PlayerControl killer = null)
    {
        Player = player;
        TimeOfDeath = timeOfDeath;
        Reason = reason;
        Killer = killer;
    }
}