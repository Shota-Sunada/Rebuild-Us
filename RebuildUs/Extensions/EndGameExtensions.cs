using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RebuildUs.Roles;

namespace RebuildUs.Extensions;

internal enum CustomGameOverReason
{
}

internal enum WinCondition
{
    Default,

    EveryoneDied,
}

internal enum FinalStatus
{
    Alive,
    Sabotage,
    Exiled,
    Dead,
    Suicide,
    Misfire,
    Disconnected
}

internal static class EndGameExtensions
{
    internal static GameOverReason gameOverReason = GameOverReason.CrewmatesByTask;

    internal static void OnGameEndPrefix(ref EndGameResult endGameResult)
    {
        gameOverReason = endGameResult.GameOverReason;
        if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;
    }

    internal static void OnGameEndPostfix()
    {
        AdditionalTempData.Clear();

        foreach (var playerControl in PlayerControl.AllPlayerControls)
        {
            var roles = RolesManager.GetRoleInfoForPlayer(playerControl);
            var (tasksCompleted, tasksTotal) = TasksHandler.TaskInfo(playerControl.Data);
            int? killCount = GameHistory.DeadPlayers.FindAll(x => x.Killer != null && x.Killer.PlayerId == playerControl.PlayerId).Count;
            if (killCount == 0 && !(new List<RoleInfoAttribute>() { RolesManager.AllRoles[RoleId.Sheriff] }.Contains(RolesManager.GetRoleInfoForPlayer(playerControl).FirstOrDefault()) || playerControl.Data.Role.IsImpostor))
            {
                killCount = null;
            }
            var roleString = RoleInfo.GetRolesString(playerControl, true, true, false);
            AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo() { PlayerName = playerControl.Data.PlayerName, Roles = roles, RoleNames = roleString, TasksTotal = tasksTotal, TasksCompleted = tasksCompleted, IsGuesser = isGuesser, Kills = killCount, IsAlive = !playerControl.Data.IsDead });
        }
    }
}

static class AdditionalTempData
{
    internal static WinCondition winCondition = WinCondition.Default;
    internal static List<WinCondition> additionalWinConditions = [];
    internal static List<PlayerRoleInfo> playerRoles = [];
    internal static float timer = 0;

    internal static void Clear()
    {
        playerRoles.Clear();
        additionalWinConditions.Clear();
        winCondition = WinCondition.Default;
        timer = 0;
    }

    internal class PlayerRoleInfo
    {
        internal string PlayerName { get; set; }
        internal List<RoleInfoAttribute> Roles { get; set; }
        internal string RoleNames { get; set; }
        internal int TasksCompleted { get; set; }
        internal int TasksTotal { get; set; }
        internal int? Kills { get; set; }
        internal bool IsAlive { get; set; }
        internal FinalStatus Status { get; set; }
        internal int PlayerId { get; set; }
    }
}