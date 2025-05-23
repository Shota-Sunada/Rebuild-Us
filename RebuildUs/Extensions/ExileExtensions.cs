using System;
using System.Linq;
using HarmonyLib;
using RebuildUs.Objects;
using RebuildUs.Patches;
using UnityEngine;

namespace RebuildUs.Extensions;

[HarmonyPatch]
public static class ExileExtensions
{
    public static void ExileControllerBegin(ref NetworkedPlayerInfo exiled)
    {
        // Medic shield
        if (Medic.exists)
        {
            foreach (var medic in Medic.players)
            {
                if (medic.player && AmongUsClient.Instance.AmHost && medic.futureShielded != null && !medic.player.Data.IsDead)
                { // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
                    using var writer = RPCProcedure.SendRPC(CustomRPC.MedicSetShielded);
                    writer.Write(medic.player.PlayerId);
                    writer.Write(medic.futureShielded.PlayerId);
                    RPCProcedure.medicSetShielded(medic.player.PlayerId, medic.futureShielded.PlayerId);
                }
                if (medic.usedShield) medic.meetingAfterShielding = true;  // Has to be after the setting of the shield
            }
        }

        // Shifter shift
        if (Shifter.shifter != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null)
        { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
            using var writer = RPCProcedure.SendRPC(CustomRPC.ShifterShift);
            writer.Write(Shifter.futureShift.PlayerId);
            RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
        }
        Shifter.futureShift = null;

        // Eraser erase
        if (Eraser.exists && AmongUsClient.Instance.AmHost && Eraser.futureErased != null)
        {  // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
            foreach (PlayerControl target in Eraser.futureErased)
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.ErasePlayerRoles);
                writer.Write(target.PlayerId);
                RPCProcedure.erasePlayerRoles(target.PlayerId);

                Eraser.alreadyErased.Add(target.PlayerId);
            }
        }
        Eraser.futureErased = [];

        // Trickster boxes
        if (Trickster.trickster != null && JackInTheBox.hasJackInTheBoxLimitReached())
        {
            JackInTheBox.convertToVents();
        }

        // Activate portals.
        Portal.meetingEndsUpdate();

        // Witch execute casted spells
        if (Witch.witch != null && Witch.futureSpelled != null && AmongUsClient.Instance.AmHost)
        {
            bool exiledIsWitch = exiled != null && exiled.PlayerId == Witch.witch.PlayerId;
            bool witchDiesWithExiledLover = exiled != null && Lovers.bothDie && exiled.Object.isLovers() && exiled.Object.getPartner() == Witch.witch;

            if ((witchDiesWithExiledLover || exiledIsWitch) && Witch.witchVoteSavesTargets) Witch.futureSpelled = [];
            foreach (PlayerControl target in Witch.futureSpelled)
            {
                if (target != null && !target.Data.IsDead && Helpers.checkMurderAttempt(Witch.witch, target, true) == MurderAttemptResult.PerformKill)
                {
                    if (exiled != null && Lawyer.lawyer != null && (target == Lawyer.lawyer || target == Lovers.getPartner(Lawyer.lawyer)) && Lawyer.target != null && Lawyer.isProsecutor && Lawyer.target.PlayerId == exiled.PlayerId)
                        continue;
                    if (target == Lawyer.target && Lawyer.lawyer != null)
                    {
                        using var writer2 = RPCProcedure.SendRPC(CustomRPC.LawyerPromotesToPursuer);
                        RPCProcedure.lawyerPromotesToPursuer();
                    }

                    using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedExilePlayer);
                    writer.Write(target.PlayerId);
                    RPCProcedure.uncheckedExilePlayer(target.PlayerId);

                    using var writer3 = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                    writer3.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer3.Write((byte)GhostInfoTypes.DeathReasonAndKiller);
                    writer3.Write(target.PlayerId);
                    writer3.Write((byte)CustomDeathReason.WitchExile);
                    writer3.Write(Witch.witch.PlayerId);

                    GameHistory.overrideDeathReasonAndKiller(target, CustomDeathReason.WitchExile, killer: Witch.witch);
                }
            }
        }
        Witch.futureSpelled = [];

        // SecurityGuard vents and cameras
        var allCameras = MapUtilities.CachedShipStatus.AllCameras.ToList();
        MapOptions.camerasToAdd.ForEach(camera =>
        {
            camera.gameObject.SetActive(true);
            camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            allCameras.Add(camera);
        });
        MapUtilities.CachedShipStatus.AllCameras = allCameras.ToArray();
        MapOptions.camerasToAdd = [];

        foreach (Vent vent in MapOptions.ventsToSeal)
        {
            PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>();
            vent.EnterVentAnim = vent.ExitVentAnim = null;
            Sprite newSprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
            SpriteRenderer rend = vent.myRend;
            if (Helpers.isFungle())
            {
                newSprite = SecurityGuard.getFungleVentSealedSprite();
                rend = vent.transform.GetChild(3).GetComponent<SpriteRenderer>();
                animator = vent.transform.GetChild(3).GetComponent<PowerTools.SpriteAnim>();
            }
            animator?.Stop();
            rend.sprite = newSprite;
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = SecurityGuard.getSubmergedCentralUpperSealedSprite();
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = SecurityGuard.getSubmergedCentralLowerSealedSprite();
            rend.color = Color.white;
            vent.name = "SealedVent_" + vent.name;
        }
        MapOptions.ventsToSeal = [];

        EventUtility.meetingEndsUpdate();
    }

    public static void WrapUpPostfix(PlayerControl exiled)
    {
        // Prosecutor win condition
        if (exiled != null && Lawyer.lawyer != null && Lawyer.target != null && Lawyer.isProsecutor && Lawyer.target.PlayerId == exiled.PlayerId && !Lawyer.lawyer.Data.IsDead)
            Lawyer.triggerProsecutorWin = true;

        // Mini exile lose condition
        else if (exiled != null && Mini.mini != null && Mini.mini.PlayerId == exiled.PlayerId && !Mini.isGrownUp() && !Mini.mini.Data.Role.IsImpostor && !RoleInfo.getRoleInfoForPlayer(Mini.mini).Any(x => x.roleType is RoleType.Neutral))
        {
            Mini.triggerMiniLose = true;
        }
        // Jester win condition
        else if (exiled != null && Jester.exists && exiled.isRole(RoleId.Jester))
        {
            Jester.triggerJesterWin = true;
            if (Jester.jesterWinEveryone)
            {
                var jester = Jester.players.FirstOrDefault(x => x.player.PlayerId == exiled.PlayerId);
                jester.isWin = true;
            }
        }


        // Reset custom button timers where necessary
        CustomButton.MeetingEndedUpdate();

        // Mini set adapted cooldown
        if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini && Mini.mini.Data.Role.IsImpostor)
        {
            var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            Mini.mini.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier);
        }

        // Seer spawn souls
        if (Seer.deadBodyPositions != null && Seer.exists && PlayerControl.LocalPlayer.isRole(RoleId.Seer) && (Seer.mode is 0 or 2))
        {
            foreach (Vector3 pos in Seer.deadBodyPositions)
            {
                GameObject soul = new();
                //soul.transform.position = pos;
                soul.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000 - 1f);
                soul.layer = 5;
                var rend = soul.AddComponent<SpriteRenderer>();
                soul.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                rend.sprite = Seer.getSoulSprite();

                if (Seer.limitSoulDuration)
                {
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Seer.soulDuration, new Action<float>((p) =>
                    {
                        if (rend != null)
                        {
                            var tmp = rend.color;
                            tmp.a = Mathf.Clamp01(1 - p);
                            rend.color = tmp;
                        }
                        if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                    })));
                }
            }
            Seer.deadBodyPositions = [];
        }

        // Tracker reset deadBodyPositions
        Tracker.deadBodyPositions = [];

        // Arsonist deactivate dead poolable players
        if (Arsonist.exists && PlayerControl.LocalPlayer.isRole(RoleId.Arsonist))
        {
            int visibleCounter = 0;
            Vector3 newBottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft;
            var BottomLeft = newBottomLeft + new Vector3(-0.25f, -0.25f, 0);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!MapOptions.playerIcons.ContainsKey(p.PlayerId)) continue;
                if (p.Data.IsDead || p.Data.Disconnected)
                {
                    MapOptions.playerIcons[p.PlayerId].gameObject.SetActive(false);
                }
                else
                {
                    MapOptions.playerIcons[p.PlayerId].transform.localPosition = newBottomLeft + Vector3.right * visibleCounter * 0.35f;
                    visibleCounter++;
                }
            }
        }

        // Force Bounty Hunter Bounty Update
        if (BountyHunter.bountyHunter != null && BountyHunter.bountyHunter == PlayerControl.LocalPlayer)
            BountyHunter.bountyUpdateTimer = 0f;

        // Medium spawn souls
        if (Medium.medium != null && PlayerControl.LocalPlayer == Medium.medium)
        {
            if (Medium.souls != null)
            {
                foreach (SpriteRenderer sr in Medium.souls) UnityEngine.Object.Destroy(sr.gameObject);
                Medium.souls = [];
            }

            if (Medium.futureDeadBodies != null)
            {
                foreach ((DeadPlayer db, Vector3 ps) in Medium.futureDeadBodies)
                {
                    GameObject s = new();
                    //s.transform.position = ps;
                    s.transform.position = new Vector3(ps.x, ps.y, ps.y / 1000 - 1f);
                    s.layer = 5;
                    var rend = s.AddComponent<SpriteRenderer>();
                    s.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                    rend.sprite = Medium.getSoulSprite();
                    Medium.souls.Add(rend);
                }
                Medium.deadBodies = Medium.futureDeadBodies;
                Medium.futureDeadBodies = [];
            }
        }

        // AntiTeleport set position
        AntiTeleport.setPosition();

        // Invert add meeting
        if (Invert.meetings > 0) Invert.meetings--;

        Chameleon.lastMoved.Clear();

        foreach (Trap trap in Trap.traps) trap.trigger_able = false;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2 + 2, new Action<float>((p) =>
        {
            if (p == 1f) foreach (Trap trap in Trap.traps) trap.trigger_able = true;
        })));

        if (!Yoyo.markStaysOverMeeting)
        {
            Silhouette.clearSilhouettes();
        }
    }

    public static void GetStringPostfix(ref string __result, StringNames id)
    {
        try
        {
            if (ExileController.Instance != null && ExileController.Instance.initData != null)
            {
                PlayerControl player = ExileController.Instance.initData.networkedPlayer.Object;
                if (player == null) return;
                // Exile role text
                if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                {
                    __result = player.Data.PlayerName + " was The " + string.Join(" ", [.. RoleInfo.getRoleInfoForPlayer(player).Select(x => x.name)]);
                }
                // Hide number of remaining impostors on Jester win
                if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS)
                {
                    if (Jester.exists && player.isRole(RoleId.Jester)) __result = "";
                }
                if (Tiebreaker.isTiebreak) __result += " (Tiebreaker)";
                Tiebreaker.isTiebreak = false;
            }
        }
        catch
        {
            // pass - Hopefully prevent leaving while exiling to soft lock game
        }
    }
}