using HarmonyLib;
using System;
using UnityEngine;
using static RebuildUs.RebuildUs;
using RebuildUs.Objects;
using System.Collections.Generic;
using System.Linq;

using RebuildUs.Utilities;
using RebuildUs.CustomGameModes;
using AmongUs.GameOptions;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
class HudManagerUpdatePatch
{
    private static Dictionary<byte, (string name, Color color)> TagColorDict = [];
    static void resetNameTagsAndColors()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var myData = PlayerControl.LocalPlayer.Data;
        var amImpostor = myData.Role.IsImpostor;
        var morphTimerNotUp = Morphing.morphTimer > 0f;
        var morphTargetNotNull = Morphing.morphTarget != null;

        var dict = TagColorDict;
        dict.Clear();

        foreach (var data in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            var player = data.Object;
            string text = data.PlayerName;
            Color color;
            if (player)
            {
                var playerName = text;
                if (morphTimerNotUp && morphTargetNotNull && Morphing.morphing == player) playerName = Morphing.morphTarget.Data.PlayerName;
                var nameText = player.cosmetics.nameText;

                nameText.text = Helpers.hidePlayerName(localPlayer, player) ? "" : playerName;
                nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                nameText.color = nameText.color.SetAlpha(Chameleon.visibility(player.PlayerId));
            }
            else
            {
                color = Color.white;
            }


            dict.Add(data.PlayerId, (text, color));
        }

        if (MeetingHud.Instance != null)
        {
            foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
            {
                var data = dict[playerVoteArea.TargetPlayerId];
                var text = playerVoteArea.NameText;
                text.text = data.name;
                text.color = data.color;
            }
        }
    }

    static void setPlayerNameColor(PlayerControl p, Color color)
    {
        p.cosmetics.nameText.color = color.SetAlpha(Chameleon.visibility(p.PlayerId));
        if (MeetingHud.Instance != null)
            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                    player.NameText.color = color;
    }

    static void setNameColors()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var localRole = RoleInfo.getRoleInfoForPlayer(localPlayer).FirstOrDefault();
        setPlayerNameColor(localPlayer, localRole.color);

        /*if (Jester.jester != null && Jester.jester == localPlayer)
            setPlayerNameColor(Jester.jester, Jester.color);
        else if (Mayor.mayor != null && Mayor.mayor == localPlayer)
            setPlayerNameColor(Mayor.mayor, Mayor.color);
        else if (Engineer.engineer != null && Engineer.engineer == localPlayer)
            setPlayerNameColor(Engineer.engineer, Engineer.color);
        else if (Sheriff.sheriff != null && Sheriff.sheriff == localPlayer) {
            setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
            if (Deputy.deputy != null && Deputy.knowsSheriff) {
                setPlayerNameColor(Deputy.deputy, Deputy.color);
            }
        } else
        if (Deputy.deputy != null && Deputy.deputy == localPlayer)
        {
            setPlayerNameColor(Deputy.deputy, Deputy.color);
            if (Sheriff.sheriff != null && Deputy.knowsSheriff)
            {
                setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
            }
        } else if (Portalmaker.portalmaker != null && Portalmaker.portalmaker == localPlayer)
            setPlayerNameColor(Portalmaker.portalmaker, Portalmaker.color);
        else if (Lighter.lighter != null && Lighter.lighter == localPlayer)
            setPlayerNameColor(Lighter.lighter, Lighter.color);
        else if (Detective.detective != null && Detective.detective == localPlayer)
            setPlayerNameColor(Detective.detective, Detective.color);
        else if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == localPlayer)
            setPlayerNameColor(TimeMaster.timeMaster, TimeMaster.color);
        else if (Medic.medic != null && Medic.medic == localPlayer)
            setPlayerNameColor(Medic.medic, Medic.color);
        else if (Shifter.shifter != null && Shifter.shifter == localPlayer)
            setPlayerNameColor(Shifter.shifter, Shifter.color);
        else if (Swapper.swapper != null && Swapper.swapper == localPlayer)
            setPlayerNameColor(Swapper.swapper, Swapper.color);
        else if (Seer.seer != null && Seer.seer == localPlayer)
            setPlayerNameColor(Seer.seer, Seer.color);
        else if (Hacker.hacker != null && Hacker.hacker == localPlayer)
            setPlayerNameColor(Hacker.hacker, Hacker.color);
        else if (Tracker.tracker != null && Tracker.tracker == localPlayer)
            setPlayerNameColor(Tracker.tracker, Tracker.color);
        else if (Snitch.snitch != null && Snitch.snitch == localPlayer)
            setPlayerNameColor(Snitch.snitch, Snitch.color);*/
        if (TeamJackal.Jackal.jackal != null && TeamJackal.Jackal.jackal == localPlayer)
        {
            // Jackal can see his sidekick
            setPlayerNameColor(TeamJackal.Jackal.jackal, TeamJackal.Color);
            if (TeamJackal.Sidekick.sidekick != null)
            {
                setPlayerNameColor(TeamJackal.Sidekick.sidekick, TeamJackal.Color);
            }
            if (TeamJackal.Jackal.fakeSidekick != null)
            {
                setPlayerNameColor(TeamJackal.Jackal.fakeSidekick, TeamJackal.Color);
            }
        }
        else if (PlayerControl.LocalPlayer.isRole(RoleId.Swapper))
        {
            setPlayerNameColor(PlayerControl.LocalPlayer, Swapper.swapper.Data.Role.IsImpostor ? Palette.ImpostorRed : Swapper.Color);
        }
        /*else if (Spy.spy != null && Spy.spy == localPlayer) {
            setPlayerNameColor(Spy.spy, Spy.color);
        } else if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == localPlayer) {
            setPlayerNameColor(SecurityGuard.securityGuard, SecurityGuard.color);
        } else if (Arsonist.arsonist != null && Arsonist.arsonist == localPlayer) {
            setPlayerNameColor(Arsonist.arsonist, Arsonist.color);
        } else if (Guesser.niceGuesser != null && Guesser.niceGuesser == localPlayer) {
            setPlayerNameColor(Guesser.niceGuesser, Guesser.color);
        } else if (Guesser.evilGuesser != null && Guesser.evilGuesser == localPlayer) {
            setPlayerNameColor(Guesser.evilGuesser, Palette.ImpostorRed);
        } else if (Vulture.vulture != null && Vulture.vulture == localPlayer) {
            setPlayerNameColor(Vulture.vulture, Vulture.color);
        } else if (Medium.medium != null && Medium.medium == localPlayer) {
            setPlayerNameColor(Medium.medium, Medium.color);
        } else if (Trapper.trapper != null && Trapper.trapper == localPlayer) {
            setPlayerNameColor(Trapper.trapper, Trapper.color);
        } else if (Lawyer.lawyer != null && Lawyer.lawyer == localPlayer) {
            setPlayerNameColor(Lawyer.lawyer, Lawyer.color);
        } else if (Pursuer.pursuer != null && Pursuer.pursuer == localPlayer) {
            setPlayerNameColor(Pursuer.pursuer, Pursuer.color);
        }*/

        // No else if here, as a Lover of team Jackal needs the colors
        if (TeamJackal.Sidekick.sidekick != null && TeamJackal.Sidekick.sidekick == localPlayer)
        {
            // Sidekick can see the jackal
            setPlayerNameColor(TeamJackal.Sidekick.sidekick, TeamJackal.Color);
            if (TeamJackal.Jackal.jackal != null)
            {
                setPlayerNameColor(TeamJackal.Jackal.jackal, TeamJackal.Color);
            }
        }

        // No else if here, as the Impostors need the Spy name to be colored
        if (Spy.spy != null && localPlayer.Data.Role.IsImpostor)
        {
            setPlayerNameColor(Spy.spy, Spy.color);
        }
        if (TeamJackal.Sidekick.sidekick != null && TeamJackal.Sidekick.wasTeamRed && localPlayer.Data.Role.IsImpostor)
        {
            setPlayerNameColor(TeamJackal.Sidekick.sidekick, Spy.color);
        }
        if (TeamJackal.Jackal.jackal != null && TeamJackal.Jackal.wasTeamRed && localPlayer.Data.Role.IsImpostor)
        {
            setPlayerNameColor(TeamJackal.Jackal.jackal, Spy.color);
        }

        // Crewmate roles with no changes: Mini
        // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter,  Witch and Mafioso
    }

    static void setNameTags()
    {
        // Mafia
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (Mafia.Godfather.godfather != null && player.isRole(RoleId.Godfather))
                {
                    player.cosmetics.nameText.text = player.Data.PlayerName + " (G)";
                }
                else if (Mafia.Mafioso.mafioso != null && player.isRole(RoleId.Mafioso))
                {
                    player.cosmetics.nameText.text = player.Data.PlayerName + " (M)";
                }
                else if (Mafia.Janitor.janitor != null && player.isRole(RoleId.Janitor))
                {
                    player.cosmetics.nameText.text = player.Data.PlayerName + " (J)";
                }
            }

            if (MeetingHud.Instance != null)
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    if (Mafia.Godfather.godfather != null && Mafia.Godfather.godfather.PlayerId == player.TargetPlayerId)
                    {
                        player.NameText.text = Mafia.Godfather.godfather.Data.PlayerName + " (G)";
                    }
                    else if (Mafia.Mafioso.mafioso != null && Mafia.Mafioso.mafioso.PlayerId == player.TargetPlayerId)
                    {
                        player.NameText.text = Mafia.Mafioso.mafioso.Data.PlayerName + " (M)";
                    }
                    else if (Mafia.Janitor.janitor != null && Mafia.Janitor.janitor.PlayerId == player.TargetPlayerId)
                    {
                        player.NameText.text = Mafia.Janitor.janitor.Data.PlayerName + " (J)";
                    }
                }
            }
        }

        // Lovers
        if (PlayerControl.LocalPlayer.isLovers() && PlayerControl.LocalPlayer.isAlive())
        {
            string suffix = Lovers.getIcon(PlayerControl.LocalPlayer);
            var lover1 = PlayerControl.LocalPlayer;
            var lover2 = PlayerControl.LocalPlayer.getPartner();

            lover1.cosmetics.nameText.text += suffix;
            if (!Helpers.hidePlayerName(lover2))
                lover2.cosmetics.nameText.text += suffix;

            if (Helpers.ShowMeetingText)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (lover1.PlayerId == player.TargetPlayerId || lover2.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
        }

        // Lawyer or Prosecutor
        if ((Lawyer.lawyer != null && Lawyer.target != null && Lawyer.lawyer == PlayerControl.LocalPlayer))
        {
            Color color = Lawyer.color;
            PlayerControl target = Lawyer.target;
            string suffix = Helpers.cs(color, " §");
            target.cosmetics.nameText.text += suffix;

            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.TargetPlayerId == target.PlayerId)
                        player.NameText.text += suffix;
        }

        // Former Thief
        if (Thief.formerThief != null && (Thief.formerThief == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead))
        {
            string suffix = Helpers.cs(Thief.color, " $");
            Thief.formerThief.cosmetics.nameText.text += suffix;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.TargetPlayerId == Thief.formerThief.PlayerId)
                        player.NameText.text += suffix;
        }

        // Display lighter / darker color for all alive players
        if (PlayerControl.LocalPlayer != null && MeetingHud.Instance != null && MapOptions.showLighterDarker)
        {
            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
            {
                var target = Helpers.playerById(player.TargetPlayerId);
                if (target != null) player.NameText.text += $" ({(Helpers.isLighterColor(target) ? "L" : "D")})";
            }
        }

        // Add medic shield info:
        if (Medic.exists)
        {
            foreach (var medic in Medic.players)
            {
                if (MeetingHud.Instance != null && medic.shielded != null && Medic.shieldVisible(medic.shielded))
                {
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == medic.shielded.PlayerId)
                        {
                            player.NameText.text = Helpers.cs(Medic.Color, "[") + player.NameText.text + Helpers.cs(Medic.Color, "]");
                            // player.HighlightedFX.color = Medic.color;
                            // player.HighlightedFX.enabled = true;
                        }
                }
            }
        }
    }

    static void updateShielded()
    {
    }

    static void timerUpdate()
    {
        var dt = Time.deltaTime;
        // Hacker.hackerTimer -= dt;
        Trickster.lightsOutTimer -= dt;
        // Tracker.corpsesTrackingTimer -= dt;
        Ninja.invisibleTimer -= dt;
    }

    public static void miniUpdate()
    {
        if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() || Mini.mini == Morphing.morphing && Morphing.morphTimer > 0f || Mini.mini == Ninja.ninja && Ninja.isInvisible || SurveillanceMinigamePatch.nightVisionIsActive) return;

        float growingProgress = Mini.growingProgress();
        float scale = growingProgress * 0.35f + 0.35f;
        string suffix = "";
        if (growingProgress != 1f)
            suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>";
        if (!Mini.isGrowingUpInMeeting && MeetingHud.Instance != null && Mini.ageOnMeetingStart != 0 && !(Mini.ageOnMeetingStart >= 18))
            suffix = " <color=#FAD934FF>(" + Mini.ageOnMeetingStart + ")</color>";

        Mini.mini.cosmetics.nameText.text += suffix;
        if (MeetingHud.Instance != null)
        {
            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                if (player.NameText != null && Mini.mini.PlayerId == player.TargetPlayerId)
                    player.NameText.text += suffix;
        }

        if (Morphing.morphing != null && Morphing.morphTarget == Mini.mini && Morphing.morphTimer > 0f)
            Morphing.morphing.cosmetics.nameText.text += suffix;
    }

    static void updateImpostorKillButton(HudManager __instance)
    {
        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
        if (MeetingHud.Instance)
        {
            __instance.KillButton.Hide();
            return;
        }
        bool enabled = true;
        if (Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer)
        {
            enabled = false;
        }
        else if (Mafia.Mafioso.mafioso != null && PlayerControl.LocalPlayer.isRole(RoleId.Mafioso) && Mafia.Godfather.godfather != null && !Mafia.Godfather.godfather.isDead())
        {
            enabled = false;
        }
        else if (Mafia.Janitor.janitor != null && PlayerControl.LocalPlayer.isRole(RoleId.Janitor))
        {
            enabled = false;
        }

        if (enabled) __instance.KillButton.Show();
        else __instance.KillButton.Hide();
    }

    static void updateReportButton(HudManager __instance)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
        if (MeetingHud.Instance) __instance.ReportButton.Hide();
        else if (!__instance.ReportButton.isActiveAndEnabled) __instance.ReportButton.Show();
    }

    static void updateVentButton(HudManager __instance)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
        if (MeetingHud.Instance) __instance.ImpostorVentButton.Hide();
        else if (PlayerControl.LocalPlayer.roleCanUseVents() && !__instance.ImpostorVentButton.isActiveAndEnabled)
        {
            __instance.ImpostorVentButton.Show();

        }
        if (Rewired.ReInput.players.GetPlayer(0).GetButtonDown(RewiredConsts.Action.UseVent) && !PlayerControl.LocalPlayer.Data.Role.IsImpostor && PlayerControl.LocalPlayer.roleCanUseVents())
        {
            __instance.ImpostorVentButton.DoClick();
        }

    }

    static void updateUseButton(HudManager __instance)
    {
        if (MeetingHud.Instance) __instance.UseButton.Hide();
    }

    static void updateSabotageButton(HudManager __instance)
    {
        if (MeetingHud.Instance) __instance.SabotageButton.Hide();
        if (PlayerControl.LocalPlayer.Data.IsDead && CustomOptionHolder.deadImpsBlockSabotage.getBool()) __instance.SabotageButton.Hide();
    }

    static void updateMapButton(HudManager __instance)
    {
        if (Trapper.trapper == null || !(PlayerControl.LocalPlayer.PlayerId == Trapper.trapper.PlayerId) || __instance == null || __instance.MapButton.HeldButtonSprite == null) return;
        __instance.MapButton.HeldButtonSprite.color = Trapper.playersOnMap.Any() ? Trapper.color : Color.white;
    }

    static void Postfix(HudManager __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        EventUtility.Update();

        CustomButton.HudUpdate();
        resetNameTagsAndColors();
        setNameColors();
        updateShielded();
        setNameTags();

        // Impostors
        updateImpostorKillButton(__instance);
        // Timer updates
        timerUpdate();
        // Mini
        miniUpdate();

        // Deputy Sabotage, Use and Vent Button Disabling
        updateReportButton(__instance);
        updateVentButton(__instance);
        // Meeting hide buttons if needed (used for the map usage, because closing the map would show buttons)
        updateSabotageButton(__instance);
        updateUseButton(__instance);
        updateMapButton(__instance);
        if (!MeetingHud.Instance) __instance.AbilityButton?.Update();

        // Fix dead player's pets being visible by just always updating whether the pet should be visible at all.
        foreach (PlayerControl target in PlayerControl.AllPlayerControls)
        {
            var pet = target.GetPet();
            if (pet != null)
            {
                pet.Visible = (PlayerControl.LocalPlayer.Data.IsDead && target.Data.IsDead || !target.Data.IsDead) && !target.inVent;
            }
        }
    }
}