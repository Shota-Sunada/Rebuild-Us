using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace RebuildUs.Extensions;

public static class EndGameExtensions
{
    public static GameOverReason gameOverReason = GameOverReason.CrewmatesByTask;

    public static void OnGameEndPrefix(ref EndGameResult endGameResult)
    {
        gameOverReason = endGameResult.GameOverReason;
        if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;

        // Reset zoomed out ghosts
        Helpers.toggleZoom(reset: true);
    }

    public static void OnGameEndPostfix()
    {
        AdditionalTempData.clear();

        foreach (var playerControl in PlayerControl.AllPlayerControls)
        {
            var roles = RoleInfo.getRoleInfoForPlayer(playerControl);
            var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(playerControl.Data);

            var finalStatus = GameHistory.finalStatuses[playerControl.PlayerId] =
                    playerControl.Data.Disconnected ? FinalStatus.Disconnected :
                    GameHistory.finalStatuses.ContainsKey(playerControl.PlayerId) ? GameHistory.finalStatuses[playerControl.PlayerId] :
                    playerControl.Data.IsDead ? FinalStatus.Dead :
                    gameOverReason == GameOverReason.ImpostorsBySabotage && !playerControl.Data.Role.IsImpostor ? FinalStatus.Sabotage :
                    FinalStatus.Alive;

            int? killCount = GameHistory.deadPlayers.FindAll(x => x.killerIfExisting != null && x.killerIfExisting.PlayerId == playerControl.PlayerId).Count;
            if (killCount == 0 && !(new List<RoleInfo>() { RoleInfo.sheriff, RoleInfo.jackal, RoleInfo.sidekick, RoleInfo.thief }.Contains(RoleInfo.getRoleInfoForPlayer(playerControl).FirstOrDefault()) || playerControl.Data.Role.IsImpostor))
            {
                killCount = null;
            }

            AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
            {
                PlayerName = playerControl.Data.PlayerName,
                Roles = roles,
                RoleNames = RoleInfo.GetRolesString(playerControl, true),
                TasksTotal = tasksTotal,
                TasksCompleted = tasksCompleted,
                Kills = killCount,
                IsAlive = !playerControl.Data.IsDead,
                Status = finalStatus
            });
        }

        // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be re-added)
        var notWinners = new List<PlayerControl>();
        if (TeamJackal.Sidekick.sidekick != null) notWinners.Add(TeamJackal.Sidekick.sidekick);
        if (TeamJackal.Jackal.jackal != null) notWinners.Add(TeamJackal.Jackal.jackal);
        if (Vulture.vulture != null) notWinners.Add(Vulture.vulture);
        if (Lawyer.lawyer != null) notWinners.Add(Lawyer.lawyer);
        if (Pursuer.pursuer != null) notWinners.Add(Pursuer.pursuer);
        if (Thief.thief != null) notWinners.Add(Thief.thief);

        notWinners.AddRange(TeamJackal.formerJackals);

        notWinners.AddRange(Jester.allPlayers);
        notWinners.AddRange(Arsonist.allPlayers);

        if (Lovers.separateTeam)
        {
            foreach (var couple in Lovers.couples)
            {
                notWinners.Add(couple.lover1);
                notWinners.Add(couple.lover2);
            }
        }

        var winnersToRemove = new List<CachedPlayerData>();
        foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator())
        {
            if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
        }
        foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);

        bool jesterWin = Jester.exists && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
        bool arsonistWin = Arsonist.exists && gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
        bool miniLose = Mini.mini != null && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
        bool loversWin = Lovers.anyAlive() && !(Lovers.separateTeam && gameOverReason == GameOverReason.CrewmatesByTask);
        bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin && ((TeamJackal.Jackal.jackal != null && !TeamJackal.Jackal.jackal.Data.IsDead) || (TeamJackal.Sidekick.sidekick != null && !TeamJackal.Sidekick.sidekick.Data.IsDead));
        bool vultureWin = Vulture.vulture != null && gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
        bool prosecutorWin = Lawyer.lawyer != null && gameOverReason == (GameOverReason)CustomGameOverReason.ProsecutorWin;

        bool isPursurerLose = jesterWin || arsonistWin || miniLose || vultureWin || teamJackalWin;

        // Mini lose
        if (miniLose)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Mini.mini.Data)
            {
                IsYou = false // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
            };
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.MiniLose;
        }

        // Jester win
        else if (jesterWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            foreach (var jester in Jester.players)
            {
                if (jester.isWin || Jester.jesterWinEveryone)
                {
                    var wpd = new CachedPlayerData(jester.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
            }
            AdditionalTempData.winCondition = WinCondition.JesterWin;
        }

        // Arsonist win
        else if (arsonistWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            foreach (var arsonist in Arsonist.players)
            {
                var wpd = new CachedPlayerData(arsonist.player.Data);
                EndGameResult.CachedWinners.Add(wpd);
            }
            AdditionalTempData.winCondition = WinCondition.ArsonistWin;
        }

        // Vulture win
        else if (vultureWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Vulture.vulture.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.VultureWin;
        }

        // Jester win
        else if (prosecutorWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Lawyer.lawyer.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.ProsecutorWin;
        }

        // Lovers win conditions
        else if (loversWin)
        {
            // Double win for lovers, crewmates also win
            if (GameManager.Instance.DidHumansWin(gameOverReason) && !Lovers.separateTeam && Lovers.anyNonKillingCouples())
            {
                AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                AdditionalTempData.additionalWinConditions.Add(WinCondition.LoversTeamWin);
            }
            // Lovers solo win
            else
            {
                AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();

                foreach (var couple in Lovers.couples)
                {
                    if (couple.existingAndAlive)
                    {
                        EndGameResult.CachedWinners.Add(new(couple.lover1.Data));
                        EndGameResult.CachedWinners.Add(new(couple.lover2.Data));
                    }
                }
            }
        }

        // Jackal win condition (should be implemented using a proper GameOverReason in the future)
        else if (teamJackalWin)
        {
            // Jackal wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.JackalWin;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(TeamJackal.Jackal.jackal.Data);
            wpd.IsImpostor = false;
            EndGameResult.CachedWinners.Add(wpd);
            // If there is a sidekick. The sidekick also wins
            if (TeamJackal.Sidekick.sidekick != null)
            {
                var wpdSidekick = new CachedPlayerData(TeamJackal.Sidekick.sidekick.Data);
                wpdSidekick.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpdSidekick);
            }
            foreach (var player in TeamJackal.formerJackals)
            {
                var wpdFormerJackal = new CachedPlayerData(player.Data);
                wpdFormerJackal.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpdFormerJackal);
            }
        }

        // Possible Additional winner: Lawyer
        if (Lawyer.lawyer != null && Lawyer.target != null && (!Lawyer.target.Data.IsDead || Lawyer.target.isRole(RoleId.Jester)) && !Pursuer.notAckedExiled && !Lawyer.isProsecutor)
        {
            CachedPlayerData winningClient = null;
            foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator())
            {
                if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                    winningClient = winner;
            }
            if (winningClient != null)
            { // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Lawyer.lawyer.Data.PlayerName))
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Lawyer.lawyer.Data));
                AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins together with the client
            }
        }

        // Possible Additional winner: Pursuer
        if (Pursuer.pursuer != null && !Pursuer.pursuer.Data.IsDead && !Pursuer.notAckedExiled && !isPursurerLose && !EndGameResult.CachedWinners.ToArray().Any(x => x.IsImpostor))
        {
            if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Pursuer.pursuer.Data.PlayerName))
                EndGameResult.CachedWinners.Add(new CachedPlayerData(Pursuer.pursuer.Data));
            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
        }

        // Reset Settings
        RPCProcedure.resetVariables();
        EventUtility.gameEndsUpdate();
    }

    public static void SetEverythingUpPostfix(EndGameManager __instance)
    {
        // Delete and readd PoolablePlayers always showing the name and role of the player
        foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
        {
            UnityEngine.Object.Destroy(pb.gameObject);
        }
        int num = Mathf.CeilToInt(7.5f);
        List<CachedPlayerData> list = [.. EndGameResult.CachedWinners.ToArray().ToList().OrderBy(delegate (CachedPlayerData b)
        {
            if (!b.IsYou)
            {
                return 0;
            }
            return -1;
        })];
        for (int i = 0; i < list.Count; i++)
        {
            CachedPlayerData CachedPlayerData2 = list[i];
            int num2 = (i % 2 == 0) ? -1 : 1;
            int num3 = (i + 1) / 2;
            float num4 = (float)num3 / (float)num;
            float num5 = Mathf.Lerp(1f, 0.75f, num4);
            float num6 = (float)((i == 0) ? -8 : -1);
            PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
            poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
            float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
            var vector = new Vector3(num7, num7, 1f);
            poolablePlayer.transform.localScale = vector;
            if (CachedPlayerData2.IsDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }
            poolablePlayer.UpdateFromPlayerOutfit(CachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, CachedPlayerData2.IsDead, true);

            poolablePlayer.cosmetics.nameText.color = Color.white;
            poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
            poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
            poolablePlayer.cosmetics.nameText.text = CachedPlayerData2.PlayerName;

            foreach (var data in AdditionalTempData.playerRoles)
            {
                if (data.PlayerName != CachedPlayerData2.PlayerName) continue;
                var roles =
                poolablePlayer.cosmetics.nameText.text += $"\n{string.Join("\n", data.Roles.Select(x => Helpers.cs(x.color, x.name)))}";
            }
        }

        // Additional code
        GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
        bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
        bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        TMPro.TMP_Text textRenderer = bonusText.GetComponent<TMPro.TMP_Text>();
        textRenderer.text = "";

        if (AdditionalTempData.winCondition == WinCondition.JesterWin)
        {
            textRenderer.text = "Jester Wins";
            textRenderer.color = Jester.Color;
        }
        else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
        {
            textRenderer.text = "Arsonist Wins";
            textRenderer.color = Arsonist.Color;
        }
        else if (AdditionalTempData.winCondition == WinCondition.VultureWin)
        {
            textRenderer.text = "Vulture Wins";
            textRenderer.color = Vulture.color;
        }
        else if (AdditionalTempData.winCondition == WinCondition.ProsecutorWin)
        {
            textRenderer.text = "Prosecutor Wins";
            textRenderer.color = Lawyer.color;
        }
        else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin)
        {
            textRenderer.text = "Lovers And Crewmates Win";
            textRenderer.color = Lovers.Color;
            __instance.BackgroundBar.material.SetColor("_Color", Lovers.Color);
        }
        else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin)
        {
            textRenderer.text = "Lovers Win";
            textRenderer.color = Lovers.Color;
            __instance.BackgroundBar.material.SetColor("_Color", Lovers.Color);
        }
        else if (AdditionalTempData.winCondition == WinCondition.JackalWin)
        {
            textRenderer.text = "Team Jackal Wins";
            textRenderer.color = TeamJackal.Color;
        }
        else if (AdditionalTempData.winCondition == WinCondition.MiniLose)
        {
            textRenderer.text = "Mini died";
            textRenderer.color = Mini.color;
        }
        else if (AdditionalTempData.winCondition == WinCondition.Default)
        {
            switch (gameOverReason)
            {
                case GameOverReason.ImpostorDisconnect:
                    textRenderer.text = "Last Crewmate Disconnected";
                    textRenderer.color = Color.red;
                    break;
                case GameOverReason.ImpostorsByKill:
                    textRenderer.text = "Impostors Win - By Kill";
                    textRenderer.color = Color.red;
                    break;
                case GameOverReason.ImpostorsBySabotage:
                    textRenderer.text = "Impostors Win - By Sabotage";
                    textRenderer.color = Color.red;
                    break;
                case GameOverReason.ImpostorsByVote:
                    textRenderer.text = "Impostors Win - By Vote, Guess or DC";
                    textRenderer.color = Color.red;
                    break;
                case GameOverReason.CrewmatesByTask:
                    textRenderer.text = "Crew Wins - Taskwin";
                    textRenderer.color = Color.white;
                    break;
                case GameOverReason.CrewmateDisconnect:
                    textRenderer.text = "Crew Wins - No Evil Killers Left";
                    textRenderer.color = Color.white;
                    break;
                case GameOverReason.CrewmatesByVote:
                    textRenderer.text = "Crew Wins - No Evil Killers Left";
                    textRenderer.color = Color.white;
                    break;
            }
        }

        foreach (WinCondition cond in AdditionalTempData.additionalWinConditions)
        {
            if (cond == WinCondition.AdditionalLawyerBonusWin)
            {
                textRenderer.text += $"\n{Helpers.cs(Lawyer.color, "The Lawyer wins with the client")}";
            }
            else if (cond == WinCondition.AdditionalAlivePursuerWin)
            {
                textRenderer.text += $"\n{Helpers.cs(Pursuer.color, "The Pursuer survived")}";
            }
            else if (cond == WinCondition.LoversTeamWin)
            {
                // textRenderer.text += Tr.Get("loversExtra");
            }
        }

        if (MapOptions.showRoleSummary)
        {
            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -214f);
            roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

            var roleSummaryText = new StringBuilder();
            roleSummaryText.AppendLine("Players and roles at the end of the game:");
            foreach (var data in AdditionalTempData.playerRoles)
            {
                //var roles = string.Join(" ", data.Roles.Select(x => Helpers.cs(x.color, x.name)));
                string roles = data.RoleNames;
                //if (data.IsGuesser) roles += " (Guesser)";
                var taskInfo = data.TasksTotal > 0 ? $" - <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>" : "";
                if (data.Kills != null) taskInfo += $" - <color=#FF0000FF>(Kills: {data.Kills})</color>";
                roleSummaryText.AppendLine($"{Helpers.cs(data.IsAlive ? Color.white : new Color(.7f, .7f, .7f), data.PlayerName)} - {roles}{taskInfo}");
            }
            TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
            roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
            roleSummaryTextMesh.color = Color.white;
            roleSummaryTextMesh.fontSizeMin = 1.5f;
            roleSummaryTextMesh.fontSizeMax = 1.5f;
            roleSummaryTextMesh.fontSize = 1.5f;

            var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
            roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            roleSummaryTextMesh.text = roleSummaryText.ToString();
            Helpers.previousEndGameSummary = $"<size=110%>{roleSummaryText.ToString()}</size>";
        }
        AdditionalTempData.clear();
    }

    public static bool CheckEndCriteriaPrefix(ShipStatus __instance)
    {
        if (!GameData.Instance) return false;
        if (DestroyableSingleton<TutorialManager>.InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
            return true;
        var statistics = new PlayerStatistics(__instance);
        if (CheckAndEndGameForMiniLose(__instance)) return false;
        if (CheckAndEndGameForJesterWin(__instance)) return false;
        if (CheckAndEndGameForArsonistWin(__instance)) return false;
        if (CheckAndEndGameForVultureWin(__instance)) return false;
        if (CheckAndEndGameForSabotageWin(__instance)) return false;
        if (CheckAndEndGameForTaskWin(__instance)) return false;
        if (CheckAndEndGameForProsecutorWin(__instance)) return false;
        if (CheckAndEndGameForLoversWin(__instance, statistics)) return false;
        if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
        if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
        if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;

        return false;
    }

    private static bool CheckAndEndGameForMiniLose(ShipStatus __instance)
    {
        if (Mini.triggerMiniLose)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MiniLose, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
    {
        if (Jester.triggerJesterWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
    {
        if (Arsonist.triggerArsonistWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForVultureWin(ShipStatus __instance)
    {
        if (Vulture.triggerVultureWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.VultureWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
    {
        if (MapUtilities.Systems == null) return false;
        var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp) ? MapUtilities.Systems[SystemTypes.LifeSupp] : null;
        if (systemType != null)
        {
            LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
            if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
            {
                EndGameForSabotage(__instance);
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }
        }
        var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor) ? MapUtilities.Systems[SystemTypes.Reactor] : null;
        systemType2 ??= MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory) ? MapUtilities.Systems[SystemTypes.Laboratory] : null;
        if (systemType2 != null)
        {
            ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
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
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForProsecutorWin(ShipStatus __instance)
    {
        if (Lawyer.triggerProsecutorWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ProsecutorWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForLoversWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.CouplesAlive == 1 && statistics.TotalAlive <= 3)
        {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
    {

        if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive &&
            statistics.TeamImpostorsAlive == 0 &&
            (statistics.TeamJackalLovers == 0 || statistics.TeamJackalLovers >= statistics.CouplesAlive * 2))
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamJackalWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive &&
            statistics.TeamJackalAlive == 0 &&
            (statistics.TeamImpostorLovers == 0 || statistics.TeamImpostorLovers >= statistics.CouplesAlive * 2))
        {
            //__instance.enabled = false;
            GameOverReason endReason;
            switch (GameData.LastDeathReason)
            {
                case DeathReason.Exile:
                    endReason = GameOverReason.ImpostorsByVote;
                    break;
                case DeathReason.Kill:
                    endReason = GameOverReason.ImpostorsByKill;
                    break;
                default:
                    endReason = GameOverReason.ImpostorsByVote;
                    break;
            }
            GameManager.Instance.RpcEndGame(endReason, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByVote, false);
            return true;
        }
        return false;
    }

    private static void EndGameForSabotage(ShipStatus __instance)
    {
        //__instance.enabled = false;
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsBySabotage, false);
        return;
    }

    private static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = [];
        public static List<PlayerRoleInfo> playerRoles = [];

        public static void clear()
        {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public List<RoleInfo> Roles { get; set; }
            public string RoleNames { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public int? Kills { get; set; }
            public bool IsAlive { get; set; }
            public FinalStatus Status { get; set; }
        }
    }

    internal class PlayerStatistics
    {
        public int TeamImpostorsAlive { get; set; }
        public int TeamJackalAlive { get; set; }
        public int TeamLoversAlive { get; set; }
        public int CouplesAlive { get; set; }
        public int TeamCrew { get; set; }
        public int NeutralAlive { get; set; }
        public int TotalAlive { get; set; }
        public int TeamImpostorLovers { get; set; }
        public int TeamJackalLovers { get; set; }

        public PlayerStatistics(ShipStatus __instance)
        {
            GetPlayerCounts();
        }

        private bool isLover(NetworkedPlayerInfo p)
        {
            foreach (var couple in Lovers.couples)
            {
                if (p.PlayerId == couple.lover1.PlayerId || p.PlayerId == couple.lover2.PlayerId) return true;
            }
            return false;
        }

        private void GetPlayerCounts()
        {
            int numJackalAlive = 0;
            int numImpostorsAlive = 0;
            int numTotalAlive = 0;
            int numNeutralAlive = 0;
            int numCrew = 0;

            int numLoversAlive = 0;
            int numCouplesAlive = 0;
            int impLovers = 0;
            int jackalLovers = 0;

            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                var playerInfo = GameData.Instance.AllPlayers[i];
                if (!playerInfo.Disconnected)
                {
                    if (playerInfo.Object.isCrewmate()) numCrew++;
                    if (!playerInfo.IsDead)
                    {
                        numTotalAlive++;

                        bool lover = isLover(playerInfo);
                        if (lover) numLoversAlive++;

                        if (playerInfo.Role.IsImpostor)
                        {
                            numImpostorsAlive++;
                            if (lover) impLovers++;
                        }
                        if (TeamJackal.Jackal.jackal != null && TeamJackal.Jackal.jackal.PlayerId == playerInfo.PlayerId)
                        {
                            numJackalAlive++;
                            if (lover) jackalLovers++;
                        }
                        if (TeamJackal.Sidekick.sidekick != null && TeamJackal.Sidekick.sidekick.PlayerId == playerInfo.PlayerId)
                        {
                            numJackalAlive++;
                            if (lover) jackalLovers++;
                        }

                        if (playerInfo.Object.isNeutral()) numNeutralAlive++;
                    }
                }
            }

            foreach (var couple in Lovers.couples)
            {
                if (couple.alive) numCouplesAlive++;
            }

            // In the special case of Mafia being enabled, but only the janitor's left alive,
            // count it as zero impostors alive bc they can't actually do anything.
            if (Mafia.Godfather.godfather?.isDead() == true && Mafia.Mafioso.mafioso?.isDead() == true && Mafia.Janitor.janitor?.isDead() == false)
            {
                numImpostorsAlive = 0;
            }

            TeamCrew = numCrew;
            TeamJackalAlive = numJackalAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TeamLoversAlive = numLoversAlive;
            NeutralAlive = numNeutralAlive;
            TotalAlive = numTotalAlive;
            CouplesAlive = numCouplesAlive;
            TeamImpostorLovers = impLovers;
            TeamJackalLovers = jackalLovers;
        }
    }
}