using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using static RebuildUs.RebuildUs;
using static RebuildUs.MapOptions;
using RebuildUs.Objects;
using RebuildUs.Roles;
using System;

using RebuildUs.Utilities;
using UnityEngine;
using Innersloth.Assets;
using TMPro;

namespace RebuildUs.Patches;

[HarmonyPatch]
class MeetingHudPatch
{
    static bool[] selections;
    static SpriteRenderer[] renderers;
    private static NetworkedPlayerInfo target = null;
    private const float scale = 0.65f;
    private static TMPro.TextMeshPro meetingExtraButtonText;
    private static PassiveButton[] swapperButtonList;
    private static TMPro.TextMeshPro meetingExtraButtonLabel;
    private static PlayerVoteArea swapped1 = null;
    private static PlayerVoteArea swapped2 = null;
    public static bool animateSwap = false;

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    class MeetingCalculateVotesPatch
    {
        private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
        {
            Dictionary<byte, int> dictionary = [];
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                var playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != 255 && playerVoteArea.VotedFor != 254)
                {
                    PlayerControl player = Helpers.playerById(playerVoteArea.TargetPlayerId);
                    if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;

                    int additionalVotes = (Mayor.exists && Mayor.players.Any(x => x.player.PlayerId == playerVoteArea.TargetPlayerId)) ? Mayor.numVotes : 1; // Mayor vote
                    dictionary[playerVoteArea.VotedFor] = dictionary.TryGetValue(playerVoteArea.VotedFor, out int currentVotes) ? currentVotes + additionalVotes : additionalVotes;
                }
            }
            // Swapper swap votes
            if (Swapper.swapper != null && !Swapper.swapper.Data.IsDead)
            {
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;
                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                if (swapped1 != null && swapped2 != null)
                {
                    if (!dictionary.ContainsKey(swapped1.TargetPlayerId)) dictionary[swapped1.TargetPlayerId] = 0;
                    if (!dictionary.ContainsKey(swapped2.TargetPlayerId)) dictionary[swapped2.TargetPlayerId] = 0;
                    int tmp = dictionary[swapped1.TargetPlayerId];
                    dictionary[swapped1.TargetPlayerId] = dictionary[swapped2.TargetPlayerId];
                    dictionary[swapped2.TargetPlayerId] = tmp;

                    if (AmongUsClient.Instance.AmHost)
                    {
                        using var writer = RPCProcedure.SendRPC(CustomRPC.SwapperAnimate);
                        RPCProcedure.swapperAnimate();
                    }
                }
            }

            return dictionary;
        }


        static bool Prefix(MeetingHud __instance)
        {
            if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
            {
                // If skipping is disabled, replace skipps/no-votes with self vote
                if (target == null && blockSkippingInEmergencyMeetings && noVoteIsSelfVote)
                {
                    foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                    {
                        if (playerVoteArea.VotedFor == byte.MaxValue - 1) playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId; // TargetPlayerId
                    }
                }

                Dictionary<byte, int> self = CalculateVotes(__instance);
                bool tie;
                KeyValuePair<byte, int> max = self.MaxPair(out tie);
                NetworkedPlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

                // TieBreaker
                List<NetworkedPlayerInfo> potentialExiled = [];
                bool skipIsTie = false;
                if (self.Count > 0)
                {
                    Tiebreaker.isTiebreak = false;
                    int maxVoteValue = self.Values.Max();
                    PlayerVoteArea tb = null;
                    if (Tiebreaker.tiebreaker != null)
                        tb = __instance.playerStates.ToArray().FirstOrDefault(x => x.TargetPlayerId == Tiebreaker.tiebreaker.PlayerId);
                    bool isTiebreakerSkip = tb == null || tb.VotedFor == 253;
                    if (tb != null && tb.AmDead) isTiebreakerSkip = true;

                    foreach (KeyValuePair<byte, int> pair in self)
                    {
                        if (pair.Value != maxVoteValue || isTiebreakerSkip) continue;
                        if (pair.Key != 253)
                            potentialExiled.Add(GameData.Instance.AllPlayers.ToArray().FirstOrDefault(x => x.PlayerId == pair.Key));
                        else
                            skipIsTie = true;
                    }
                }

                MeetingHud.VoterState[] array = new MeetingHud.VoterState[__instance.playerStates.Length];
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    array[i] = new MeetingHud.VoterState
                    {
                        VoterId = playerVoteArea.TargetPlayerId,
                        VotedForId = playerVoteArea.VotedFor
                    };

                    if (Tiebreaker.tiebreaker == null || playerVoteArea.TargetPlayerId != Tiebreaker.tiebreaker.PlayerId) continue;

                    byte tiebreakerVote = playerVoteArea.VotedFor;
                    if (swapped1 != null && swapped2 != null)
                    {
                        if (tiebreakerVote == swapped1.TargetPlayerId) tiebreakerVote = swapped2.TargetPlayerId;
                        else if (tiebreakerVote == swapped2.TargetPlayerId) tiebreakerVote = swapped1.TargetPlayerId;
                    }

                    if (potentialExiled.FindAll(x => x != null && x.PlayerId == tiebreakerVote).Count > 0 && (potentialExiled.Count > 1 || skipIsTie))
                    {
                        exiled = potentialExiled.ToArray().FirstOrDefault(v => v.PlayerId == tiebreakerVote);
                        tie = false;

                        using var writer = RPCProcedure.SendRPC(CustomRPC.SetTiebreak);
                        RPCProcedure.setTiebreak();
                    }
                }

                // RPCVotingComplete
                __instance.RpcVotingComplete(array, exiled, tie);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
    class MeetingHudBloopAVoteIconPatch
    {
        public static bool Prefix(MeetingHud __instance, NetworkedPlayerInfo voterPlayer, int index, Transform parent)
        {
            var spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
            var showVoteColors = !GameManager.Instance.LogicOptions.GetAnonymousVotes() ||
                                    (PlayerControl.LocalPlayer.Data.IsDead && MapOptions.ghostsSeeVotes) ||
                                    (Mayor.exists && PlayerControl.LocalPlayer.isRole(RoleId.Mayor) && Mayor.canSeeVoteColors && TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data).Item1 >= Mayor.tasksNeededToSeeVoteColors);
            if (showVoteColors)
            {
                PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
            }
            else
            {
                PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
            }

            var transform = spriteRenderer.transform;
            transform.SetParent(parent);
            transform.localScale = Vector3.zero;
            var component = parent.GetComponent<PlayerVoteArea>();
            if (component != null)
            {
                spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
            }

            __instance.StartCoroutine(Effects.Bloop(index * 0.3f, transform));
            parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
    class MeetingHudPopulateVotesPatch
    {

        private static bool Prefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states)
        {
            // Swapper swap
            PlayerVoteArea swapped1 = null;
            PlayerVoteArea swapped2 = null;

            foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
            {
                if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
            }
            bool doSwap = animateSwap && swapped1 != null && swapped2 != null && Swapper.swapper != null && !Swapper.swapper.Data.IsDead;
            if (doSwap)
            {
                __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
                __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));
                Swapper.numSwaps--;
            }

            __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            int num = 0;
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                byte targetPlayerId = playerVoteArea.TargetPlayerId;
                // Swapper change playerVoteArea that gets the votes
                if (doSwap && playerVoteArea.TargetPlayerId == swapped1.TargetPlayerId) playerVoteArea = swapped2;
                else if (doSwap && playerVoteArea.TargetPlayerId == swapped2.TargetPlayerId) playerVoteArea = swapped1;

                playerVoteArea.ClearForResults();
                int num2 = 0;
                //bool mayorFirstVoteDisplayed = false;
                var votesApplied = new Dictionary<int, int>();
                for (int j = 0; j < states.Length; j++)
                {
                    MeetingHud.VoterState voterState = states[j];
                    PlayerControl voter = Helpers.playerById(voterState.VoterId);
                    if (voter == null) continue;

                    var playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                    if (playerById == null)
                    {
                        Debug.LogError(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId));
                    }
                    else if (i == 0 && voterState.SkippedVote && !playerById.IsDead)
                    {
                        __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                        num++;
                    }
                    else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead)
                    {
                        __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                        num2++;
                    }

                    if (!votesApplied.ContainsKey(voter.PlayerId))
                    {
                        votesApplied[voter.PlayerId] = 0;
                    }

                    votesApplied[voter.PlayerId]++;

                    // Major vote, redo this iteration to place a second vote
                    if (Mayor.exists && Mayor.players.Any(x => x.player.PlayerId == voterState.VoterId) && votesApplied[playerById.PlayerId] < Mayor.numVotes)
                    {
                        j--;
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    class MeetingHudVotingCompletedPatch
    {
        static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte[] states, [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
        {
            // Reset swapper values
            Swapper.playerId1 = Byte.MaxValue;
            Swapper.playerId2 = Byte.MaxValue;

            // Lovers, Lawyer & Pursuer save next to be exiled, because RPC of ending game comes before RPC of exiled
            Pursuer.notAckedExiled = false;
            if (exiled != null)
            {
                GameHistory.finalStatuses[exiled.PlayerId] = FinalStatus.Exiled;
                bool isLovers = exiled.Object.isLovers();
                if (isLovers)
                {
                    GameHistory.finalStatuses[exiled.Object.getPartner().PlayerId] = FinalStatus.Suicide;
                }

                Pursuer.notAckedExiled = (Pursuer.pursuer != null && Pursuer.pursuer.PlayerId == exiled.PlayerId) || (Lawyer.lawyer != null && Lawyer.target != null && Lawyer.target.PlayerId == exiled.PlayerId && Lawyer.target.isRole(RoleId.Jester) && !Lawyer.isProsecutor);
            }

            // Mini
            if (!Mini.isGrowingUpInMeeting) Mini.timeOfGrowthStart = Mini.timeOfGrowthStart.Add(DateTime.UtcNow.Subtract(Mini.timeOfMeetingStart)).AddSeconds(10);

            // Snitch
            if (Snitch.snitch != null && !Snitch.needsUpdate && Snitch.snitch.Data.IsDead && Snitch.text != null)
            {
                UnityEngine.Object.Destroy(Snitch.text);
            }
        }
    }


    static void swapperOnClick(int i, MeetingHud __instance)
    {
        if (Swapper.numSwaps <= 0) return;
        if (__instance.state == MeetingHud.VoteStates.Results) return;
        if (__instance.playerStates[i].AmDead) return;

        int selectedCount = selections.Where(b => b).Count();
        SpriteRenderer renderer = renderers[i];

        if (selectedCount == 0)
        {
            renderer.color = Color.green;
            selections[i] = true;
        }
        else if (selectedCount == 1)
        {
            if (selections[i])
            {
                renderer.color = Color.red;
                selections[i] = false;
            }
            else
            {
                selections[i] = true;
                renderer.color = Color.green;

                PlayerVoteArea firstPlayer = null;
                PlayerVoteArea secondPlayer = null;
                for (int A = 0; A < selections.Length; A++)
                {
                    if (selections[A])
                    {
                        if (firstPlayer != null)
                        {
                            secondPlayer = __instance.playerStates[A];
                            break;
                        }
                        else
                        {
                            firstPlayer = __instance.playerStates[A];
                        }
                    }
                }

                if (firstPlayer != null && secondPlayer != null)
                {
                    using var writer = RPCProcedure.SendRPC(CustomRPC.SwapperSwap);
                    writer.Write(firstPlayer.TargetPlayerId);
                    writer.Write(secondPlayer.TargetPlayerId);
                    RPCProcedure.swapperSwap(firstPlayer.TargetPlayerId, secondPlayer.TargetPlayerId);
                }
            }
        }
    }

    public static GameObject guesserUI;
    public static PassiveButton guesserUIExitButton;
    public static byte guesserCurrentTarget;
    static void guesserOnClick(int buttonTarget, MeetingHud __instance)
    {
        if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted)) return;
        __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));

        Transform PhoneUI = UnityEngine.Object.FindObjectsOfType<Transform>().FirstOrDefault(x => x.name == "PhoneUI");
        Transform container = UnityEngine.Object.Instantiate(PhoneUI, __instance.transform);
        container.transform.localPosition = new Vector3(0, 0, -5f);
        guesserUI = container.gameObject;

        int i = 0;
        var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
        var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
        var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
        var textTemplate = __instance.playerStates[0].NameText;

        guesserCurrentTarget = __instance.playerStates[buttonTarget].TargetPlayerId;

        Transform exitButtonParent = (new GameObject()).transform;
        exitButtonParent.SetParent(container);
        Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate.transform, exitButtonParent);
        Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
        exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
        exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -5);
        exitButtonParent.transform.localScale = new Vector3(0.217f, 0.9f, 1);
        guesserUIExitButton = exitButton.GetComponent<PassiveButton>();
        guesserUIExitButton.OnClick.RemoveAllListeners();
        guesserUIExitButton.OnClick.AddListener((System.Action)(() =>
        {
            __instance.playerStates.ToList().ForEach(x =>
            {
                x.gameObject.SetActive(true);
                if (PlayerControl.LocalPlayer.Data.IsDead && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
            });
            UnityEngine.Object.Destroy(container.gameObject);
        }));

        List<Transform> buttons = [];
        Transform selectedButton = null;

        foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos)
        {
            RoleId guesserRole = (Guesser.niceGuesser != null && PlayerControl.LocalPlayer.PlayerId == Guesser.niceGuesser.PlayerId) ? RoleId.NiceGuesser : RoleId.EvilGuesser;
            if (roleInfo.isModifier || roleInfo.roleId == guesserRole || (!HandleGuesser.evilGuesserCanGuessSpy && guesserRole == RoleId.EvilGuesser && roleInfo.roleId == RoleId.Spy)) continue; // Not guessable roles & modifier
            // remove all roles that cannot spawn due to the settings from the ui.
            RoleManagerSelectRolesPatch.RoleAssignmentData roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
            if (roleData.neutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.neutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
            else if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId].rate == 0) continue;
            else if (roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId].rate == 0) continue;
            else if (new List<RoleId>() { RoleId.Janitor, RoleId.Godfather, RoleId.Mafioso }.Contains(roleInfo.roleId) && (CustomOptionHolder.mafiaSpawnRate.getSelection() == 0 || GameOptionsManager.Instance.currentGameOptions.NumImpostors < 3)) continue;
            else if (roleInfo.roleId == RoleId.Sidekick && (!CustomOptionHolder.jackalCanCreateSidekick.getBool() || CustomOptionHolder.jackalSpawnRate.getSelection() == 0)) continue;
            if (roleInfo.roleId == RoleId.Pursuer && CustomOptionHolder.lawyerSpawnRate.getSelection() == 0) continue;
            if (roleInfo.roleId == RoleId.Spy && roleData.impostors.Count <= 1) continue;
            if (roleInfo.roleId == RoleId.Prosecutor && (CustomOptionHolder.lawyerIsProsecutorChance.getSelection() == 0 || CustomOptionHolder.lawyerSpawnRate.getSelection() == 0)) continue;
            if (roleInfo.roleId == RoleId.Lawyer && (CustomOptionHolder.lawyerIsProsecutorChance.getSelection() == 10 || CustomOptionHolder.lawyerSpawnRate.getSelection() == 0)) continue;
            if (Snitch.snitch != null && HandleGuesser.guesserCantGuessSnitch)
            {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
                int numberOfLeftTasks = playerTotal - playerCompleted;
                if (numberOfLeftTasks <= 0 && roleInfo.roleId == RoleId.Snitch) continue;
            }

            Transform buttonParent = (new GameObject()).transform;
            buttonParent.SetParent(container);
            Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
            Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
            TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
            button.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
            buttons.Add(button);
            int row = i / 5, col = i % 5;
            buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -5);
            buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
            label.text = Helpers.cs(roleInfo.color, roleInfo.name);
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
            label.transform.localScale *= 1.7f;
            int copiedIndex = i;

            button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            if (!PlayerControl.LocalPlayer.Data.IsDead && !Helpers.playerById(__instance.playerStates[buttonTarget].TargetPlayerId).Data.IsDead) button.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() =>
            {
                if (selectedButton != button)
                {
                    selectedButton = button;
                    buttons.ForEach(x => x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white);
                }
                else
                {
                    PlayerControl focusedTarget = Helpers.playerById(__instance.playerStates[buttonTarget].TargetPlayerId);
                    if (!(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted) || focusedTarget == null || HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) <= 0) return;

                    foreach (var medic in Medic.players)
                    {
                        if (!HandleGuesser.killsThroughShield && focusedTarget == medic.shielded)
                        { // Depending on the options, shooting the shielded player will not allow the guess, notifiy everyone about the kill attempt and close the window
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                            UnityEngine.Object.Destroy(container.gameObject);

                            using var murderAttemptWriter = RPCProcedure.SendRPC(CustomRPC.ShieldedMurderAttempt);
                            RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);

                            return;
                        }
                    }

                    var mainRoleInfo = RoleInfo.getRoleInfoForPlayer(focusedTarget, false).FirstOrDefault();
                    if (mainRoleInfo == null) return;

                    PlayerControl dyingTarget = (mainRoleInfo == roleInfo) ? focusedTarget : PlayerControl.LocalPlayer;

                    // Reset the GUI
                    __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                    UnityEngine.Object.Destroy(container.gameObject);
                    if (HandleGuesser.hasMultipleShotsPerMeeting && HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 1 && dyingTarget != PlayerControl.LocalPlayer)
                        __instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                    else
                        __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                    // Shoot player and send chat info if activated
                    using var writer = RPCProcedure.SendRPC(CustomRPC.GuesserShoot);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(dyingTarget.PlayerId);
                    writer.Write(focusedTarget.PlayerId);
                    writer.Write((byte)roleInfo.roleId);
                    RPCProcedure.guesserShoot(PlayerControl.LocalPlayer.PlayerId, dyingTarget.PlayerId, focusedTarget.PlayerId, (byte)roleInfo.roleId);
                }
            }));

            i++;
        }
        container.transform.localScale *= 0.75f;
    }

    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
    class PlayerVoteAreaSelectPatch
    {
        static bool Prefix(MeetingHud __instance)
        {
            return !(PlayerControl.LocalPlayer != null && HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && guesserUI != null);
        }
    }

    static void populateButtonsPostfix(MeetingHud __instance)
    {
        // Add Swapper Buttons
        if (PlayerControl.LocalPlayer.isRole(RoleId.Swapper) && Swapper.numSwaps > 0 && !Swapper.swapper.Data.IsDead)
        {
            selections = new bool[__instance.playerStates.Length];
            renderers = new SpriteRenderer[__instance.playerStates.Length];

            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.AmDead || (playerVoteArea.TargetPlayerId == Swapper.swapper.PlayerId && Swapper.canOnlySwapOthers)) continue;

                GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                GameObject checkbox = UnityEngine.Object.Instantiate(template);
                checkbox.transform.SetParent(playerVoteArea.transform);
                checkbox.transform.position = template.transform.position;
                checkbox.transform.localPosition = new Vector3(-0.95f, 0.03f, -20f);
                SpriteRenderer renderer = checkbox.GetComponent<SpriteRenderer>();
                renderer.sprite = Swapper.getCheckSprite();
                renderer.color = Color.red;

                PassiveButton button = checkbox.GetComponent<PassiveButton>();
                button.OnClick.RemoveAllListeners();
                int copiedIndex = i;
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => swapperOnClick(copiedIndex, __instance)));

                selections[i] = false;
                renderers[i] = renderer;
            }
        }

        bool isGuesser = HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId);

        // Add overlay for spelled players
        if (Witch.witch != null && Witch.futureSpelled != null)
        {
            foreach (PlayerVoteArea pva in __instance.playerStates)
            {
                if (Witch.futureSpelled.Any(x => x.PlayerId == pva.TargetPlayerId))
                {
                    SpriteRenderer rend = (new GameObject()).AddComponent<SpriteRenderer>();
                    rend.transform.SetParent(pva.transform);
                    rend.gameObject.layer = pva.Megaphone.gameObject.layer;
                    rend.transform.localPosition = new Vector3(-0.5f, -0.03f, -1f);
                    if (PlayerControl.LocalPlayer == Swapper.swapper && isGuesser) rend.transform.localPosition = new Vector3(-0.725f, -0.15f, -1f);
                    rend.sprite = Witch.getSpelledOverlaySprite();
                }
            }
        }

        // Add Guesser Buttons
        int remainingShots = HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId);
        var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);

        if (isGuesser && !PlayerControl.LocalPlayer.Data.IsDead && remainingShots > 0)
        {
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Eraser) && Eraser.alreadyErased.Contains(playerVoteArea.TargetPlayerId)) continue;
                if (PlayerControl.LocalPlayer != null && !Helpers.isEvil(PlayerControl.LocalPlayer)) continue;

                GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                targetBox.name = "ShootButton";
                targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                renderer.sprite = HandleGuesser.getTargetSprite();
                PassiveButton button = targetBox.GetComponent<PassiveButton>();
                button.OnClick.RemoveAllListeners();
                int copiedIndex = i;
                button.OnClick.AddListener((System.Action)(() => guesserOnClick(copiedIndex, __instance)));
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
    class MeetingServerStartPatch
    {
        static void Postfix(MeetingHud __instance)
        {
            populateButtonsPostfix(__instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
    class MeetingDeserializePatch
    {
        static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] bool initialState)
        {
            // Add swapper buttons
            if (initialState)
            {
                populateButtonsPostfix(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
    class StartMeetingPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo meetingTarget)
        {
            RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
            byte roomId = Byte.MinValue;
            if (roomTracker != null && roomTracker.LastRoom != null)
            {
                roomId = (byte)roomTracker.LastRoom?.RoomId;
            }
            if (Snitch.snitch != null && roomTracker != null)
            {
                using var roomWriter = RPCProcedure.SendRPC(CustomRPC.ShareRoom);
                roomWriter.Write(PlayerControl.LocalPlayer.PlayerId);
                roomWriter.Write(roomId);
            }

            // Resett Bait list
            Bait.active = [];
            // Save AntiTeleport position, if the player is able to move (i.e. not on a ladder or a gap thingy)
            if (PlayerControl.LocalPlayer.MyPhysics.enabled && (PlayerControl.LocalPlayer.moveable || PlayerControl.LocalPlayer.inVent
                || Hacker.hackerVitalsButton.isEffectActive || Hacker.hackerAdminTableButton.isEffectActive || HudManagerStartPatch.securityGuardCamButton.isEffectActive
                || Portal.isTeleporting && Portal.teleportedPlayers.Last().playerId == PlayerControl.LocalPlayer.PlayerId))
            {
                if (!PlayerControl.LocalPlayer.inMovingPlat)
                    AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
            }

            // Medium meeting start time
            Medium.meetingStartTime = DateTime.UtcNow;
            // Mini
            Mini.timeOfMeetingStart = DateTime.UtcNow;
            Mini.ageOnMeetingStart = Mathf.FloorToInt(Mini.growingProgress() * 18);
            // Reset vampire bitten
            Vampire.bitten = null;
            // Count meetings
            if (meetingTarget == null) meetingsCount++;
            // Save the meeting target
            target = meetingTarget;


            // Add Portal info into Portalmaker Chat:
            if (Portalmaker.portalmaker != null && (PlayerControl.LocalPlayer == Portalmaker.portalmaker || Helpers.shouldShowGhostInfo()) && !Portalmaker.portalmaker.Data.IsDead)
            {
                if (Portal.teleportedPlayers.Count > 0)
                {
                    string msg = "Portal Log:\n";
                    foreach (var entry in Portal.teleportedPlayers)
                    {
                        float timeBeforeMeeting = ((float)(DateTime.UtcNow - entry.time).TotalMilliseconds) / 1000;
                        msg += Portalmaker.logHasTime ? $"{(int)timeBeforeMeeting}s ago: " : "";
                        msg += $"{entry.name} used the teleporter\n";
                    }
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Portalmaker.portalmaker, $"{msg}");
                }
            }

            // Add trapped Info into Trapper chat
            if (Trapper.trapper != null && (PlayerControl.LocalPlayer == Trapper.trapper || Helpers.shouldShowGhostInfo()) && !Trapper.trapper.Data.IsDead)
            {
                if (Trap.traps.Any(x => x.revealed))
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Trapper.trapper, "Trap Logs:");
                foreach (Trap trap in Trap.traps)
                {
                    if (!trap.revealed) continue;
                    string message = $"Trap {trap.instanceId}: \n";
                    trap.trappedPlayer = trap.trappedPlayer.OrderBy(x => rnd.Next()).ToList();
                    foreach (byte playerId in trap.trappedPlayer)
                    {
                        PlayerControl p = Helpers.playerById(playerId);
                        if (Trapper.infoType == 0) message += RoleInfo.GetRolesString(p, false, false, true) + "\n";
                        else if (Trapper.infoType == 1)
                        {
                            if (Helpers.isNeutral(p) || p.Data.Role.IsImpostor) message += "Evil Role \n";
                            else message += "Good Role \n";
                        }
                        else message += p.Data.PlayerName + "\n";
                    }
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Trapper.trapper, $"{message}");
                }
            }

            // Add Snitch info
            string output = "";

            if (Snitch.snitch != null && Snitch.mode != Snitch.Mode.Map && (PlayerControl.LocalPlayer == Snitch.snitch || Helpers.shouldShowGhostInfo()) && !Snitch.snitch.Data.IsDead)
            {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
                int numberOfTasks = playerTotal - playerCompleted;
                if (numberOfTasks == 0)
                {
                    output = $"Bad alive roles in game: \n \n";
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.4f, new Action<float>((x) =>
                    {
                        if (x == 1f)
                        {
                            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                            {
                                if (Snitch.targets == Snitch.Targets.Killers && !Helpers.isKiller(p)) continue;
                                if (Snitch.targets == Snitch.Targets.EvilPlayers && !Helpers.isEvil(p)) continue;
                                if (!Snitch.playerRoomMap.ContainsKey(p.PlayerId)) continue;
                                if (p.Data.IsDead) continue;
                                var room = Snitch.playerRoomMap[p.PlayerId];
                                var roomName = "open fields";
                                if (room != byte.MinValue)
                                {
                                    roomName = DestroyableSingleton<TranslationController>.Instance.GetString((SystemTypes)room);
                                }
                                output += "- " + RoleInfo.GetRolesString(p, false, false, true) + ", was last seen " + roomName + "\n";
                            }
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Snitch.snitch, $"{output}");
                        }
                    })));
                }
            }

            if (PlayerControl.LocalPlayer.Data.IsDead && output != "") FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{output}");

            Trapper.playersOnMap = [];
            Snitch.playerRoomMap = [];

            // Remove revealed traps
            Trap.clearRevealedTraps();

            Bomber.clearBomb();

            // Reset zoomed out ghosts
            Helpers.toggleZoom(reset: true);

            // Close In-Game Settings Display if open
            HudManagerUpdate.CloseSettings();
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class MeetingHudUpdatePatch
    {
        static void Postfix(MeetingHud __instance)
        {
            // Deactivate skip Button if skipping on emergency meetings is disabled
            if (target == null && blockSkippingInEmergencyMeetings)
                __instance.SkipVoteButton.gameObject.SetActive(false);

            if (__instance.state >= MeetingHud.VoteStates.Discussion)
            {
                // Remove first kill shield
                MapOptions.firstKillPlayer = null;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
    public static void MeetingHudIntroPrefix()
    {
        EventUtility.meetingStartsUpdate();
    }

    [HarmonyPatch]
    public class ShowHost
    {
        private static TextMeshPro Text = null;
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        [HarmonyPostfix]

        public static void Setup(MeetingHud __instance)
        {
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return;

            __instance.ProceedButton.gameObject.transform.localPosition = new(-2.5f, 2.2f, 0);
            __instance.ProceedButton.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            __instance.ProceedButton.GetComponent<PassiveButton>().enabled = false;
            __instance.HostIcon.gameObject.SetActive(true);
            __instance.ProceedButton.gameObject.SetActive(true);
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        [HarmonyPostfix]

        public static void Postfix(MeetingHud __instance)
        {
            var host = GameData.Instance.GetHost();

            if (host != null)
            {
                PlayerMaterial.SetColors(host.DefaultOutfit.ColorId, __instance.HostIcon);
                if (Text == null) Text = __instance.ProceedButton.gameObject.GetComponentInChildren<TextMeshPro>();
                Text.text = $"host: {host.PlayerName}";
            }
        }
    }
}