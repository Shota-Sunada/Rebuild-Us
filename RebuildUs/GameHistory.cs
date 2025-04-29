using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RebuildUs.Extensions;

namespace RebuildUs;

public enum CustomDeathReason
{
    Kill,
    Exile,
    Disconnect,
}

public static class GameHistory
{
    public static List<(Vector3, bool)> LocalPlayerPositions = [];
    public static List<DeadPlayer> DeadPlayers = [];
    public static Dictionary<int, FinalStatus> FinalStatuses = [];

    public static void ClearGameHistory()
    {
        LocalPlayerPositions = [];
        DeadPlayers = [];
        FinalStatuses = [];
    }
}

public class DeadPlayer(PlayerControl player, DateTime timeOfDeath, CustomDeathReason reason, PlayerControl killer = null)
{
    public PlayerControl Player = player;
    public DateTime TimeOfDeath = timeOfDeath;
    public CustomDeathReason Reason = reason;
    public PlayerControl Killer = killer;
    public bool WasCleaned = false;
}