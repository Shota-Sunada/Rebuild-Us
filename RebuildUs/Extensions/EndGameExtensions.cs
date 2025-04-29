using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RebuildUs.Localization;
using RebuildUs.Roles;
using RebuildUs.Utilities;
using UnityEngine;
using TMPro;

namespace RebuildUs.Extensions;

public enum CustomGameOverReason
{
}

public enum WinCondition
{
    Default,

    EveryoneDied,
}

public enum FinalStatus
{
    Alive,
    Sabotage,
    Exiled,
    Dead,
    Suicide,
    Misfire,
    Disconnected
}

public static class EndGameExtensions
{
    public static TMP_Text textRenderer;

    public static void OnGameEndPrefix(ref EndGameResult endGameResult)
    {
        AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
        if ((int)endGameResult.GameOverReason >= 10)
        {
            endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;
        }
    }

    public static void OnGameEndPostfix()
    {
        var gameOverReason = AdditionalTempData.gameOverReason;
        AdditionalTempData.Clear();

        var excludeRoles = new RoleId[] { };
        foreach (var p in GameData.Instance.AllPlayers)
        {
            //var p = pc.Data;
            var roles = RoleInfo.GetRoleInfoForPlayer(p.Object, excludeRoles);
            var (tasksCompleted, tasksTotal) = TasksHandler.TaskInfo(p);
            var finalStatus = GameHistory.FinalStatuses[p.PlayerId] =
                p.Disconnected == true ? FinalStatus.Disconnected :
                GameHistory.FinalStatuses.ContainsKey(p.PlayerId) ? GameHistory.FinalStatuses[p.PlayerId] :
                p.IsDead == true ? FinalStatus.Dead :
                gameOverReason == GameOverReason.ImpostorsBySabotage && !p.Role.IsImpostor ? FinalStatus.Sabotage :
                FinalStatus.Alive;
            var suffix = p.Object.ModifyNameText("");

            if (gameOverReason == GameOverReason.CrewmatesByTask && p.Object.IsCrewmate()) tasksCompleted = tasksTotal;

            AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
            {
                PlayerName = p.PlayerName,
                PlayerId = p.PlayerId,
                NameSuffix = suffix,
                Roles = roles,
                RoleString = RoleInfo.GetRolesString(p.Object, true, excludeRoles),
                TasksTotal = tasksTotal,
                TasksCompleted = tasksCompleted,
                Status = finalStatus,
            });
        }

        // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be added again)
        var notWinners = new List<PlayerControl>();

        var winnersToRemove = new List<CachedPlayerData>();
        foreach (var winner in EndGameResult.CachedWinners.GetFastEnumerator())
        {
            if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
        }
        foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);

        var saboWin = gameOverReason == GameOverReason.ImpostorsBySabotage;

        var everyoneDead = AdditionalTempData.playerRoles.All(x => x.Status != FinalStatus.Alive);

        if (everyoneDead)
        {
            EndGameResult.CachedWinners = new();
            AdditionalTempData.winCondition = WinCondition.EveryoneDied;
        }

        // Extra win conditions for non-impostor roles
        if (!saboWin)
        {
        }

        foreach (var winners in EndGameResult.CachedWinners.GetFastEnumerator())
        {
            winners.IsDead = winners.IsDead || AdditionalTempData.playerRoles.Any(x => x.PlayerName == winners.PlayerName && x.Status != FinalStatus.Alive);
        }

        // Reset Settings
        RPCProcedure.ResetVariables();
    }

    public static void SetEverythingUpPostfix(EndGameManager __instance)
    {
        // Delete and readd PoolablePlayers always showing the name and role of the player
        foreach (var pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
        {
            Object.Destroy(pb.gameObject);
        }
        int num = Mathf.CeilToInt(7.5f);
        var list = EndGameResult.CachedWinners.ToArray().ToList().OrderBy(b =>
        {
            if (!b.IsYou)
            {
                return 0;
            }
            return -1;
        }).ToList();
        for (int i = 0; i < list.Count; i++)
        {
            var winningPlayerData2 = list[i];
            var num2 = (i % 2 == 0) ? -1 : 1;
            var num3 = (i + 1) / 2;
            var num4 = num3 / (float)num;
            var num5 = Mathf.Lerp(1f, 0.75f, num4);
            var num6 = (float)((i == 0) ? -8 : -1);
            var poolablePlayer = Object.Instantiate(__instance.PlayerPrefab, __instance.transform);
            poolablePlayer.transform.localPosition = new Vector3(1f * num2 * num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + num3 * 0.01f) * 0.9f;
            var num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
            var vector = new Vector3(num7, num7, 1f);
            poolablePlayer.transform.localScale = vector;
            poolablePlayer.UpdateFromPlayerOutfit(winningPlayerData2.Outfit, PlayerMaterial.MaskType.None, winningPlayerData2.IsDead, true);
            if (winningPlayerData2.IsDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }

            poolablePlayer.cosmetics.nameText.color = Color.white;
            poolablePlayer.cosmetics.nameText.lineSpacing *= 0.7f;
            poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
            poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
            poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

            foreach (var data in AdditionalTempData.playerRoles)
            {
                if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                poolablePlayer.cosmetics.nameText.text += new StringBuilder(data.NameSuffix).Append("\n<size=80%>").Append(data.RoleString).Append("</size>").ToString();
            }
        }

        // Additional code
        var bonusTextObject = Object.Instantiate(__instance.WinText.gameObject);
        bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
        bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
        textRenderer.text = "";

        var bonusText = "";
        if (AdditionalTempData.winCondition == WinCondition.EveryoneDied)
        {
            bonusText = "everyoneDied";
            textRenderer.color = Palette.DisabledGrey;
            __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
        }
        else if (AdditionalTempData.gameOverReason is GameOverReason.CrewmatesByTask or GameOverReason.CrewmatesByVote)
        {
            bonusText = "crewWin";
            textRenderer.color = Palette.White;
        }
        else if (AdditionalTempData.gameOverReason is GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsBySabotage or GameOverReason.ImpostorsByVote)
        {
            bonusText = "impostorWin";
            textRenderer.color = Palette.ImpostorRed;
        }

        var extraText = "";
        foreach (var w in AdditionalTempData.additionalWinConditions)
        {
            switch (w)
            {
                default:
                    break;
            }
        }

        if (extraText.Length > 0)
        {
            textRenderer.text = string.Format(Tr.Get(bonusText + "Extra"), extraText);
        }
        else
        {
            textRenderer.text = Tr.Get(bonusText);
        }

        foreach (var cond in AdditionalTempData.additionalWinConditions)
        {
            switch (cond)
            {
                default:
                    break;
            }
        }

        if (MapOptions.ShowGameResult)
        {
            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            var gameResult = Object.Instantiate(__instance.WinText.gameObject);
            gameResult.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
            gameResult.transform.localScale = new Vector3(1f, 1f, 1f);

            var gameResultText = new StringBuilder();
            gameResultText.AppendLine(Tr.Get("roleSummaryText"));
            AdditionalTempData.playerRoles.Sort((x, y) =>
            {
                var roleX = x.Roles.FirstOrDefault();
                var roleY = y.Roles.FirstOrDefault();
                var idX = roleX == null ? RoleId.NoRole : roleX.roleId;
                var idY = roleY == null ? RoleId.NoRole : roleY.roleId;

                if (x.Status == y.Status)
                {
                    if (idX == idY)
                    {
                        return x.PlayerName.CompareTo(y.PlayerName);
                    }
                    return idX.CompareTo(idY);
                }
                return x.Status.CompareTo(y.Status);
            });

            foreach (var data in AdditionalTempData.playerRoles)
            {
                var taskInfo = data.TasksTotal > 0 ? new StringBuilder("<color=#FAD934FF>").Append(data.TasksCompleted).Append('/').Append(data.TasksTotal).Append("</color>").ToString() : "";
                var aliveDead = Tr.Get("roleSummary" + data.Status.ToString());
                var result = $"{data.PlayerName + data.NameSuffix}<pos=18.5%>{taskInfo}<pos=25%>{aliveDead}<pos=34%>{data.RoleString}";
                gameResultText.AppendLine(result);
            }

            var gameResultTextMesh = gameResult.GetComponent<TMP_Text>();
            gameResultTextMesh.alignment = TextAlignmentOptions.TopLeft;
            gameResultTextMesh.color = Color.white;
            gameResultTextMesh.outlineWidth *= 1.2f;
            gameResultTextMesh.fontSizeMin = 1.25f;
            gameResultTextMesh.fontSizeMax = 1.25f;
            gameResultTextMesh.fontSize = 1.25f;

            var gameResultTextMeshRectTransform = gameResultTextMesh.GetComponent<RectTransform>();
            gameResultTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            gameResultTextMesh.text = gameResultText.ToString();
        }
        AdditionalTempData.Clear();
    }

    public static bool CheckEndCriteriaPrefix(ShipStatus __instance)
    {
        if (!GameData.Instance) return false;
        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true; // InstanceExists | Don't check Custom Criteria when in Tutorial
        if (HudManager.Instance.IsIntroDisplayed) return false;

        var statistics = new PlayerStatistics(__instance);
        if (CheckAndEndGameForSabotageWin(__instance)) return false;
        if (CheckAndEndGameForTaskWin(__instance)) return false;
        if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
        if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
        return false;
    }

    private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
    {
        if (__instance.Systems == null) return false;
        var systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
        if (systemType != null)
        {
            var lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
            if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
            {
                EndGameForSabotage(__instance);
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }
        }
        var systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
        systemType2 ??= __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
        if (systemType2 != null)
        {
            var criticalSystem = systemType2.TryCast<ICriticalSabotage>();
            if (criticalSystem != null && criticalSystem.Countdown < 0f)
            {
                EndGameForSabotage(__instance);
                criticalSystem.ClearSabotage();
                return true;
            }
        }
        return false;
    }

    private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
    {
        if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            UncheckedEndGame(GameOverReason.CrewmatesByTask);
            return true;
        }

        return false;
    }

    private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive)
        {
            var endReason = GameData.LastDeathReason switch
            {
                DeathReason.Exile => GameOverReason.ImpostorsByVote,
                DeathReason.Kill => GameOverReason.ImpostorsByKill,
                _ => GameOverReason.ImpostorsByVote,
            };
            UncheckedEndGame(endReason);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamCrew > 0 && statistics.TeamImpostorsAlive == 0)
        {
            UncheckedEndGame(GameOverReason.CrewmatesByVote);
            return true;
        }
        return false;
    }

    private static void EndGameForSabotage(ShipStatus __instance)
    {
        UncheckedEndGame(GameOverReason.ImpostorsBySabotage);
        return;
    }

    private static void UncheckedEndGame(GameOverReason reason)
    {
        GameManager.Instance.RpcEndGame(reason, false);
    }

    private static void UncheckedEndGame(CustomGameOverReason reason)
    {
        UncheckedEndGame((GameOverReason)reason);
    }
}

internal class PlayerStatistics
{
    public int TeamImpostorsAlive { get; set; }
    public int TeamCrew { get; set; }
    public int NeutralAlive { get; set; }
    public int TotalAlive { get; set; }

    public PlayerStatistics(ShipStatus __instance)
    {
        GetPlayerCounts();
    }

    private void GetPlayerCounts()
    {
        var numImpostorsAlive = 0;
        var numTotalAlive = 0;
        var numNeutralAlive = 0;
        var numCrew = 0;

        for (int i = 0; i < GameData.Instance.PlayerCount; i++)
        {
            var playerInfo = GameData.Instance.AllPlayers[i];
            if (!playerInfo.Disconnected)
            {
                if (playerInfo.Object.IsCrewmate()) numCrew++;
                if (!playerInfo.IsDead)
                {
                    numTotalAlive++;

                    if (playerInfo.Role.IsImpostor)
                    {
                        numImpostorsAlive++;
                    }

                    if (playerInfo.Object.IsNeutral()) numNeutralAlive++;
                }
            }
        }

        TeamCrew = numCrew;
        TeamImpostorsAlive = numImpostorsAlive;
        NeutralAlive = numNeutralAlive;
        TotalAlive = numTotalAlive;
    }
}

static class AdditionalTempData
{
    public static WinCondition winCondition = WinCondition.Default;
    public static List<WinCondition> additionalWinConditions = [];
    public static List<PlayerRoleInfo> playerRoles = [];
    public static GameOverReason gameOverReason;

    public static void Clear()
    {
        playerRoles.Clear();
        additionalWinConditions.Clear();
        winCondition = WinCondition.Default;
    }

    public class PlayerRoleInfo
    {
        public string PlayerName { get; set; }
        public string NameSuffix { get; set; }
        public List<RoleInfo> Roles { get; set; }
        public string RoleString { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksTotal { get; set; }
        public FinalStatus Status { get; set; }
        public int PlayerId { get; set; }
    }
}