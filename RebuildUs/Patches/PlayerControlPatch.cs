using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using static RebuildUs.RebuildUs;
using static RebuildUs.GameHistory;
using RebuildUs.Objects;

using RebuildUs.Utilities;
using RebuildUs.Roles;
using UnityEngine;
using RebuildUs.CustomGameModes;
using static UnityEngine.GraphicsBuffer;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using Sentry.Internal.Extensions;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class PlayerControlFixedUpdatePatch
{
    // Helpers

    public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
    {
        PlayerControl result = null;
        float num = AmongUs.GameOptions.LegacyGameOptions.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead) return result;

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
            {
                PlayerControl @object = playerInfo.Object;
                if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object))
                {
                    // if that player is not targetable: skip check
                    continue;
                }

                if (@object && (!@object.inVent || targetPlayersInVents))
                {
                    Vector2 vector = @object.GetTruePosition() - truePosition;
                    float magnitude = vector.magnitude;
                    if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                    {
                        result = @object;
                        num = magnitude;
                    }
                }
            }
        }
        return result;
    }

    public static void setPlayerOutline(PlayerControl target, Color color)
    {
        if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) return;

        color = color.SetAlpha(Chameleon.visibility(target.PlayerId));

        target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
        target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
    }

    // Update functions

    static void setBasePlayerOutlines()
    {
        foreach (PlayerControl target in PlayerControl.AllPlayerControls)
        {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) continue;

            bool isMorphedMorphling = target == Morphing.morphing && Morphing.morphTarget != null && Morphing.morphTimer > 0f;
            bool hasVisibleShield = false;
            Color color = Medic.ShieldedColor;
            if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && Medic.shieldVisible(target))
                hasVisibleShield = true;

            if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && MapOptions.firstKillPlayer != null && MapOptions.shieldFirstKill && ((target == MapOptions.firstKillPlayer && !isMorphedMorphling) || (isMorphedMorphling && Morphing.morphTarget == MapOptions.firstKillPlayer)))
            {
                hasVisibleShield = true;
                color = Color.blue;
            }

            if (PlayerControl.LocalPlayer.Data.IsDead && Armored.armored != null && target == Armored.armored && !Armored.isBrokenArmor && !hasVisibleShield)
            {
                hasVisibleShield = true;
                color = Color.yellow;
            }

            if (hasVisibleShield)
            {
                target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
                target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
            }
            else
            {
                target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
            }
        }
    }

    static void setPetVisibility()
    {
        bool localalive = PlayerControl.LocalPlayer.Data.IsDead;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            bool playeralive = !player.Data.IsDead;
            player.cosmetics.SetPetVisible((localalive && playeralive) || !localalive);
        }
    }

    public static void bendTimeUpdate()
    {
    }

    static void medicSetTarget()
    {
    }

    static void shifterSetTarget()
    {
        if (Shifter.shifter == null || Shifter.shifter != PlayerControl.LocalPlayer) return;
        Shifter.currentTarget = setTarget();
        if (Shifter.futureShift == null) setPlayerOutline(Shifter.currentTarget, Color.yellow);
    }


    static void morphlingSetTarget()
    {
        if (Morphing.morphing == null || Morphing.morphing != PlayerControl.LocalPlayer) return;
        Morphing.currentTarget = setTarget();
        setPlayerOutline(Morphing.currentTarget, Morphing.color);
    }

    static void trackerSetTarget()
    {
        if (Tracker.tracker == null || Tracker.tracker != PlayerControl.LocalPlayer) return;
        Tracker.currentTarget = setTarget();
        if (!Tracker.usedTracker) setPlayerOutline(Tracker.currentTarget, Tracker.color);
    }

    static void vampireSetTarget()
    {
        if (Vampire.vampire == null || Vampire.vampire != PlayerControl.LocalPlayer) return;

        PlayerControl target = null;
        if (Spy.spy != null || Sidekick.wasSpy || Jackal.wasSpy)
        {
            if (Spy.impostorsCanKillAnyone)
            {
                target = setTarget(false, true);
            }
            else
            {
                target = setTarget(true, true, [Spy.spy, Sidekick.wasTeamRed ? Sidekick.sidekick : null, Jackal.wasTeamRed ? Jackal.jackal : null]);
            }
        }
        else
        {
            target = setTarget(true, true, [Sidekick.wasImpostor ? Sidekick.sidekick : null, Jackal.wasImpostor ? Jackal.jackal : null]);
        }

        bool targetNearGarlic = false;
        if (target != null)
        {
            foreach (Garlic garlic in Garlic.garlics)
            {
                if (Vector2.Distance(garlic.garlic.transform.position, target.transform.position) <= 1.91f)
                {
                    targetNearGarlic = true;
                }
            }
        }
        Vampire.targetNearGarlic = targetNearGarlic;
        Vampire.currentTarget = target;
        setPlayerOutline(Vampire.currentTarget, Vampire.color);
    }

    static void jackalSetTarget()
    {
        if (Jackal.jackal == null || Jackal.jackal != PlayerControl.LocalPlayer) return;
        var untargetablePlayers = new List<PlayerControl>();
        if (Jackal.canCreateSidekickFromImpostor)
        {
            // Only exclude sidekick from beeing targeted if the jackal can create sidekicks from impostors
            if (Sidekick.sidekick != null) untargetablePlayers.Add(Sidekick.sidekick);
        }
        if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Jackal from targeting the Mini unless it has grown up
        Jackal.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        setPlayerOutline(Jackal.currentTarget, Palette.ImpostorRed);
    }

    static void sidekickSetTarget()
    {
        if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
        var untargetablePlayers = new List<PlayerControl>();
        if (Jackal.jackal != null) untargetablePlayers.Add(Jackal.jackal);
        if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Sidekick from targeting the Mini unless it has grown up
        Sidekick.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        if (Sidekick.canKill) setPlayerOutline(Sidekick.currentTarget, Palette.ImpostorRed);
    }

    static void sidekickCheckPromotion()
    {
        // If LocalPlayer is Sidekick, the Jackal is disconnected and Sidekick promotion is enabled, then trigger promotion
        if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
        if (Sidekick.sidekick.Data.IsDead == true || !Sidekick.promotesToJackal) return;
        if (Jackal.jackal == null || Jackal.jackal?.Data?.Disconnected == true)
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.SidekickPromotes);
            RPCProcedure.sidekickPromotes();
        }
    }

    static void eraserSetTarget()
    {
        if (Eraser.eraser == null || Eraser.eraser != PlayerControl.LocalPlayer) return;

        List<PlayerControl> untargetables = [];
        if (Spy.spy != null) untargetables.Add(Spy.spy);
        if (Sidekick.wasTeamRed) untargetables.Add(Sidekick.sidekick);
        if (Jackal.wasTeamRed) untargetables.Add(Jackal.jackal);
        Eraser.currentTarget = setTarget(onlyCrewmates: !Eraser.canEraseAnyone, untargetablePlayers: Eraser.canEraseAnyone ? [] : untargetables);
        setPlayerOutline(Eraser.currentTarget, Eraser.color);
    }

    static void impostorSetTarget()
    {
        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || !PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead)
        { // !isImpostor || !canMove || isDead
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            return;
        }

        PlayerControl target = null;
        if (Spy.spy != null || Sidekick.wasSpy || Jackal.wasSpy)
        {
            if (Spy.impostorsCanKillAnyone)
            {
                target = setTarget(false, true);
            }
            else
            {
                target = setTarget(true, true, [Spy.spy, Sidekick.wasTeamRed ? Sidekick.sidekick : null, Jackal.wasTeamRed ? Jackal.jackal : null]);
            }
        }
        else
        {
            target = setTarget(true, true, [Sidekick.wasImpostor ? Sidekick.sidekick : null, Jackal.wasImpostor ? Jackal.jackal : null]);
        }

        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
    }

    static void warlockSetTarget()
    {
        if (Warlock.warlock == null || Warlock.warlock != PlayerControl.LocalPlayer) return;
        if (Warlock.curseVictim != null && (Warlock.curseVictim.Data.Disconnected || Warlock.curseVictim.Data.IsDead))
        {
            // If the cursed victim is disconnected or dead reset the curse so a new curse can be applied
            Warlock.resetCurse();
        }
        if (Warlock.curseVictim == null)
        {
            Warlock.currentTarget = setTarget();
            setPlayerOutline(Warlock.currentTarget, Warlock.color);
        }
        else
        {
            Warlock.curseVictimTarget = setTarget(targetingPlayer: Warlock.curseVictim);
            setPlayerOutline(Warlock.curseVictimTarget, Warlock.color);
        }
    }

    static void ninjaUpdate()
    {
        if (Ninja.isInvisble && Ninja.invisibleTimer <= 0 && Ninja.ninja == PlayerControl.LocalPlayer)
        {
            using var invisibleWriter = RPCProcedure.SendRPC(CustomRPC.SetInvisible);
            invisibleWriter.Write(Ninja.ninja.PlayerId);
            invisibleWriter.Write(byte.MaxValue);
            RPCProcedure.setInvisible(Ninja.ninja.PlayerId, byte.MaxValue);
        }
        if (Ninja.arrow?.arrow != null)
        {
            if (Ninja.ninja == null || Ninja.ninja != PlayerControl.LocalPlayer || !Ninja.knowsTargetLocation)
            {
                Ninja.arrow.arrow.SetActive(false);
                return;
            }
            if (Ninja.ninjaMarked != null && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                bool trackedOnMap = !Ninja.ninjaMarked.Data.IsDead;
                Vector3 position = Ninja.ninjaMarked.transform.position;
                if (!trackedOnMap)
                { // Check for dead body
                    DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == Ninja.ninjaMarked.PlayerId);
                    if (body != null)
                    {
                        trackedOnMap = true;
                        position = body.transform.position;
                    }
                }
                Ninja.arrow.Update(position);
                Ninja.arrow.arrow.SetActive(trackedOnMap);
            }
            else
            {
                Ninja.arrow.arrow.SetActive(false);
            }
        }
    }

    static void trackerUpdate()
    {
        // Handle player tracking
        if (Tracker.arrow?.arrow != null)
        {
            if (Tracker.tracker == null || PlayerControl.LocalPlayer != Tracker.tracker)
            {
                Tracker.arrow.arrow.SetActive(false);
                if (Tracker.DangerMeterParent) Tracker.DangerMeterParent.SetActive(false);
                return;
            }

            if (Tracker.tracked != null && !Tracker.tracker.Data.IsDead)
            {
                Tracker.timeUntilUpdate -= Time.fixedDeltaTime;

                if (Tracker.timeUntilUpdate <= 0f)
                {
                    bool trackedOnMap = !Tracker.tracked.Data.IsDead;
                    Vector3 position = Tracker.tracked.transform.position;
                    if (!trackedOnMap)
                    { // Check for dead body
                        DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == Tracker.tracked.PlayerId);
                        if (body != null)
                        {
                            trackedOnMap = true;
                            position = body.transform.position;
                        }
                    }

                    if (Tracker.trackingMode == 1 || Tracker.trackingMode == 2) Arrow.UpdateProximity(position);
                    if (Tracker.trackingMode == 0 || Tracker.trackingMode == 2)
                    {
                        Tracker.arrow.Update(position);
                        Tracker.arrow.arrow.SetActive(trackedOnMap);
                    }
                    Tracker.timeUntilUpdate = Tracker.updateIntervall;
                }
                else
                {
                    if (Tracker.trackingMode == 0 || Tracker.trackingMode == 2) Tracker.arrow.Update();
                }
            }
            else if (Tracker.tracker.Data.IsDead)
            {
                Tracker.DangerMeterParent?.SetActive(false);
                Tracker.Meter?.gameObject.SetActive(false);
            }
        }

        // Handle corpses tracking
        if (Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && Tracker.corpsesTrackingTimer >= 0f && !Tracker.tracker.Data.IsDead)
        {
            bool arrowsCountChanged = Tracker.localArrows.Count != Tracker.deadBodyPositions.Count();
            int index = 0;

            if (arrowsCountChanged)
            {
                foreach (Arrow arrow in Tracker.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Tracker.localArrows = [];
            }
            foreach (Vector3 position in Tracker.deadBodyPositions)
            {
                if (arrowsCountChanged)
                {
                    Tracker.localArrows.Add(new Arrow(Tracker.color));
                    Tracker.localArrows[index].arrow.SetActive(true);
                }
                if (Tracker.localArrows[index] != null) Tracker.localArrows[index].Update(position);
                index++;
            }
        }
        else if (Tracker.localArrows.Count > 0)
        {
            foreach (Arrow arrow in Tracker.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
            Tracker.localArrows = [];
        }
    }

    public static void playerSizeUpdate(PlayerControl p)
    {
        // Set default player size
        CircleCollider2D collider = p.Collider.CastFast<CircleCollider2D>();

        p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        collider.radius = Mini.defaultColliderRadius;
        collider.offset = Mini.defaultColliderOffset * Vector2.down;

        // Set adapted player size to Mini and Morphling
        if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() || Mini.mini == Morphing.morphing && Morphing.morphTimer > 0) return;

        float growingProgress = Mini.growingProgress();
        float scale = growingProgress * 0.35f + 0.35f;
        float correctedColliderRadius = Mini.defaultColliderRadius * 0.7f / scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

        if (p == Mini.mini)
        {
            p.transform.localScale = new Vector3(scale, scale, 1f);
            collider.radius = correctedColliderRadius;
        }
        if (Morphing.morphing != null && p == Morphing.morphing && Morphing.morphTarget == Mini.mini && Morphing.morphTimer > 0f)
        {
            p.transform.localScale = new Vector3(scale, scale, 1f);
            collider.radius = correctedColliderRadius;
        }
    }

    public static void updatePlayerInfo()
    {
        Vector3 colorBlindTextMeetingInitialLocalPos = new(0.3384f, -0.16666f, -0.01f);
        Vector3 colorBlindTextMeetingInitialLocalScale = new(0.9f, 1f, 1f);
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {

            // Colorblind Text in Meeting
            PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
            if (playerVoteArea != null && playerVoteArea.ColorBlindName.gameObject.active)
            {
                playerVoteArea.ColorBlindName.transform.localPosition = colorBlindTextMeetingInitialLocalPos + new Vector3(0f, 0.4f, 0f);
                playerVoteArea.ColorBlindName.transform.localScale = colorBlindTextMeetingInitialLocalScale * 0.8f;
            }

            // Colorblind Text During the round
            if (p.cosmetics.colorBlindText != null && p.cosmetics.showColorBlindText && p.cosmetics.colorBlindText.gameObject.active)
            {
                p.cosmetics.colorBlindText.transform.localPosition = new Vector3(0, -1f, 0f);
            }

            p.cosmetics.nameText.transform.parent.SetLocalZ(-0.0001f);  // This moves both the name AND the colorblindtext behind objects (if the player is behind the object), like the rock on polus

            if ((Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target) || p == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead)
            {
                Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo == null)
                {
                    playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                    playerInfo.transform.localPosition += Vector3.up * 0.225f;
                    playerInfo.fontSize *= 0.75f;
                    playerInfo.gameObject.name = "Info";
                    playerInfo.color = playerInfo.color.SetAlpha(1f);
                }

                Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (meetingInfo == null && playerVoteArea != null)
                {
                    meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                    meetingInfo.transform.localPosition += Vector3.down * 0.2f;
                    meetingInfo.fontSize *= 0.60f;
                    meetingInfo.gameObject.name = "Info";
                }

                // Set player name higher to align in middle
                if (meetingInfo != null && playerVoteArea != null)
                {
                    var playerName = playerVoteArea.NameText;
                    playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                }

                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                string roleNames = RoleInfo.GetRolesString(p, true, false);
                string roleText = RoleInfo.GetRolesString(p, true, MapOptions.ghostsSeeModifier);
                string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";

                string playerInfoText = "";
                string meetingInfoText = "";
                if (p == PlayerControl.LocalPlayer)
                {
                    if (p.Data.IsDead) roleNames = roleText;
                    playerInfoText = $"{roleNames}";
                    if (p == Swapper.swapper) playerInfoText = $"{roleNames}" + Helpers.cs(Swapper.color, $" ({Swapper.charges})");
                    if (HudManager.Instance.TaskPanel != null)
                    {
                        TMPro.TextMeshPro tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                        tabText.SetText($"Tasks {taskInfo}");
                    }
                    meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                }
                else if (MapOptions.ghostsSeeRoles && MapOptions.ghostsSeeInformation)
                {
                    playerInfoText = $"{roleText} {taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (MapOptions.ghostsSeeInformation)
                {
                    playerInfoText = $"{taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (MapOptions.ghostsSeeRoles || (Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target))
                {
                    playerInfoText = $"{roleText}";
                    meetingInfoText = playerInfoText;
                }

                playerInfo.text = playerInfoText;
                playerInfo.gameObject.SetActive(p.Visible);
                if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
            }
        }
    }

    public static void securityGuardSetTarget()
    {
        if (SecurityGuard.securityGuard == null || SecurityGuard.securityGuard != PlayerControl.LocalPlayer || MapUtilities.CachedShipStatus == null || MapUtilities.CachedShipStatus.AllVents == null) return;

        Vent target = null;
        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        float closestDistance = float.MaxValue;
        for (int i = 0; i < MapUtilities.CachedShipStatus.AllVents.Length; i++)
        {
            Vent vent = MapUtilities.CachedShipStatus.AllVents[i];
            if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") || vent.gameObject.name.StartsWith("SealedVent_") || vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 9) continue; // cannot seal submergeds exit only vent!
            float distance = Vector2.Distance(vent.transform.position, truePosition);
            if (distance <= vent.UsableDistance && distance < closestDistance)
            {
                closestDistance = distance;
                target = vent;
            }
        }
        SecurityGuard.ventTarget = target;
    }

    public static void securityGuardUpdate()
    {
        if (SecurityGuard.securityGuard == null || PlayerControl.LocalPlayer != SecurityGuard.securityGuard || SecurityGuard.securityGuard.Data.IsDead) return;
        var (playerCompleted, _) = TasksHandler.taskInfo(SecurityGuard.securityGuard.Data);
        if (playerCompleted == SecurityGuard.rechargedTasks)
        {
            SecurityGuard.rechargedTasks += SecurityGuard.rechargeTasksNumber;
            if (SecurityGuard.maxCharges > SecurityGuard.charges) SecurityGuard.charges++;
        }
    }

    public static void arsonistSetTarget()
    {
    }

    static void snitchUpdate()
    {
        if (Snitch.snitch == null) return;
        if (!Snitch.needsUpdate) return;

        bool snitchIsDead = Snitch.snitch.Data.IsDead;
        var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);

        if (playerTotal == 0) return;
        PlayerControl local = PlayerControl.LocalPlayer;

        int numberOfTasks = playerTotal - playerCompleted;

        if (Snitch.isRevealed && ((Snitch.targets == Snitch.Targets.EvilPlayers && Helpers.isEvil(local)) || (Snitch.targets == Snitch.Targets.Killers && Helpers.isKiller(local))))
        {
            if (Snitch.text == null)
            {
                Snitch.text = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                Snitch.text.enableWordWrapping = false;
                Snitch.text.transform.localScale = Vector3.one * 0.75f;
                Snitch.text.transform.localPosition += new Vector3(0f, 1.8f, -69f);
                Snitch.text.gameObject.SetActive(true);
            }
            else
            {
                Snitch.text.text = $"Snitch is alive: " + playerCompleted + "/" + playerTotal;
                if (snitchIsDead) Snitch.text.text = $"Snitch is dead!";
            }
        }
        else if (Snitch.text != null)
            Snitch.text.Destroy();

        if (snitchIsDead)
        {
            if (MeetingHud.Instance == null) Snitch.needsUpdate = false;
            return;
        }
        if (numberOfTasks <= Snitch.taskCountForReveal) Snitch.isRevealed = true;
    }

    static void bountyHunterUpdate()
    {
        if (BountyHunter.bountyHunter == null || PlayerControl.LocalPlayer != BountyHunter.bountyHunter) return;

        if (BountyHunter.bountyHunter.Data.IsDead)
        {
            if (BountyHunter.arrow != null || BountyHunter.arrow.arrow != null) UnityEngine.Object.Destroy(BountyHunter.arrow.arrow);
            BountyHunter.arrow = null;
            if (BountyHunter.cooldownText != null && BountyHunter.cooldownText.gameObject != null) UnityEngine.Object.Destroy(BountyHunter.cooldownText.gameObject);
            BountyHunter.cooldownText = null;
            BountyHunter.bounty = null;
            foreach (PoolablePlayer p in MapOptions.playerIcons.Values)
            {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
            return;
        }

        BountyHunter.arrowUpdateTimer -= Time.fixedDeltaTime;
        BountyHunter.bountyUpdateTimer -= Time.fixedDeltaTime;

        if (BountyHunter.bounty == null || BountyHunter.bountyUpdateTimer <= 0f)
        {
            // Set new bounty
            BountyHunter.bounty = null;
            BountyHunter.arrowUpdateTimer = 0f; // Force arrow to update
            BountyHunter.bountyUpdateTimer = BountyHunter.bountyDuration;
            var possibleTargets = new List<PlayerControl>();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor && p != Spy.spy && (p != Sidekick.sidekick || !Sidekick.wasTeamRed) && (p != Jackal.jackal || !Jackal.wasTeamRed) && (p != Mini.mini || Mini.isGrownUp()) && (Lovers.getPartner(BountyHunter.bountyHunter) == null || p != Lovers.getPartner(BountyHunter.bountyHunter))) possibleTargets.Add(p);
            }
            BountyHunter.bounty = possibleTargets[RebuildUs.rnd.Next(0, possibleTargets.Count)];
            if (BountyHunter.bounty == null) return;

            // Ghost Info
            using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write((byte)GhostInfoTypes.BountyTarget);
            writer.Write(BountyHunter.bounty.PlayerId);

            // Show poolable player
            if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
            {
                foreach (PoolablePlayer pp in MapOptions.playerIcons.Values) pp.gameObject.SetActive(false);
                if (MapOptions.playerIcons.ContainsKey(BountyHunter.bounty.PlayerId) && MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject != null)
                    MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject.SetActive(true);
            }
        }

        // Hide in meeting
        if (MeetingHud.Instance && MapOptions.playerIcons.ContainsKey(BountyHunter.bounty.PlayerId) && MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject != null)
            MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject.SetActive(false);

        // Update Cooldown Text
        if (BountyHunter.cooldownText != null)
        {
            BountyHunter.cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(BountyHunter.bountyUpdateTimer, 0, BountyHunter.bountyDuration)).ToString();
            BountyHunter.cooldownText.gameObject.SetActive(!MeetingHud.Instance);  // Show if not in meeting
        }

        // Update Arrow
        if (BountyHunter.showArrow && BountyHunter.bounty != null)
        {
            if (BountyHunter.arrow == null) BountyHunter.arrow = new Arrow(Color.red);
            if (BountyHunter.arrowUpdateTimer <= 0f)
            {
                BountyHunter.arrow.Update(BountyHunter.bounty.transform.position);
                BountyHunter.arrowUpdateTimer = BountyHunter.arrowUpdateIntervall;
            }
            BountyHunter.arrow.Update();
        }
    }

    static void vultureUpdate()
    {
        if (Vulture.vulture == null || PlayerControl.LocalPlayer != Vulture.vulture || Vulture.localArrows == null || !Vulture.showArrows) return;
        if (Vulture.vulture.Data.IsDead)
        {
            foreach (Arrow arrow in Vulture.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
            Vulture.localArrows = [];
            return;
        }

        DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        bool arrowUpdate = Vulture.localArrows.Count != deadBodies.Count();
        int index = 0;

        if (arrowUpdate)
        {
            foreach (Arrow arrow in Vulture.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
            Vulture.localArrows = [];
        }

        foreach (DeadBody db in deadBodies)
        {
            if (arrowUpdate)
            {
                Vulture.localArrows.Add(new Arrow(Color.blue));
                Vulture.localArrows[index].arrow.SetActive(true);
            }
            if (Vulture.localArrows[index] != null) Vulture.localArrows[index].Update(db.transform.position);
            index++;
        }
    }

    public static void mediumSetTarget()
    {
        if (Medium.medium == null || Medium.medium != PlayerControl.LocalPlayer || Medium.medium.Data.IsDead || Medium.deadBodies == null || MapUtilities.CachedShipStatus?.AllVents == null) return;

        DeadPlayer target = null;
        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        float closestDistance = float.MaxValue;
        float usableDistance = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault().UsableDistance;
        foreach ((DeadPlayer dp, Vector3 ps) in Medium.deadBodies)
        {
            float distance = Vector2.Distance(ps, truePosition);
            if (distance <= usableDistance && distance < closestDistance)
            {
                closestDistance = distance;
                target = dp;
            }
        }
        Medium.target = target;
    }

    static bool mushroomSaboWasActive = false;
    static void morphlingAndCamouflagerUpdate()
    {
        bool mushRoomSaboIsActive = Helpers.MushroomSabotageActive();
        if (!mushroomSaboWasActive) mushroomSaboWasActive = mushRoomSaboIsActive;

        float oldCamouflageTimer = Camouflager.camouflageTimer;
        float oldMorphTimer = Morphing.morphTimer;
        Camouflager.camouflageTimer = Mathf.Max(0f, Camouflager.camouflageTimer - Time.fixedDeltaTime);
        Morphing.morphTimer = Mathf.Max(0f, Morphing.morphTimer - Time.fixedDeltaTime);

        if (mushRoomSaboIsActive) return;

        // Camouflage reset and set Morphling look if necessary
        if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f)
        {
            Camouflager.resetCamouflage();
            if (Morphing.morphTimer > 0f && Morphing.morphing != null && Morphing.morphTarget != null)
            {
                PlayerControl target = Morphing.morphTarget;
                Morphing.morphing.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
            }
        }

        // If the MushRoomSabotage ends while Morph is still active set the Morphlings look to the target's look
        if (mushroomSaboWasActive)
        {
            if (Morphing.morphTimer > 0f && Morphing.morphing != null && Morphing.morphTarget != null)
            {
                PlayerControl target = Morphing.morphTarget;
                Morphing.morphing.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
            }
            if (Camouflager.camouflageTimer > 0)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    player.setLook("", 6, "", "", "", "");
            }
        }

        // Morphling reset (only if camouflage is inactive)
        if (Camouflager.camouflageTimer <= 0f && oldMorphTimer > 0f && Morphing.morphTimer <= 0f && Morphing.morphing != null)
            Morphing.resetMorph();
        mushroomSaboWasActive = false;
    }

    public static void lawyerUpdate()
    {
        if (Lawyer.lawyer == null || Lawyer.lawyer != PlayerControl.LocalPlayer) return;

        // Promote to Pursuer
        if (Lawyer.target != null && Lawyer.target.Data.Disconnected && !Lawyer.lawyer.Data.IsDead)
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.LawyerPromotesToPursuer);
            RPCProcedure.lawyerPromotesToPursuer();

            return;
        }
    }

    public static void hackerUpdate()
    {
    }

    // For swapper swap charges
    public static void swapperUpdate()
    {
        if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || PlayerControl.LocalPlayer.Data.IsDead) return;
        var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
        if (playerCompleted == Swapper.rechargedTasks)
        {
            Swapper.rechargedTasks += Swapper.rechargeTasksNumber;
            Swapper.charges++;
        }
    }

    static void pursuerSetTarget()
    {
        if (Pursuer.pursuer == null || Pursuer.pursuer != PlayerControl.LocalPlayer) return;
        Pursuer.target = setTarget();
        setPlayerOutline(Pursuer.target, Pursuer.color);
    }

    static void witchSetTarget()
    {
        if (Witch.witch == null || Witch.witch != PlayerControl.LocalPlayer) return;
        List<PlayerControl> untargetables;
        if (Witch.spellCastingTarget != null)
            untargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != Witch.spellCastingTarget.PlayerId).ToList(); // Don't switch the target from the the one you're currently casting a spell on
        else
        {
            untargetables = []; // Also target players that have already been spelled, to hide spells that were blanks/blocked by shields
            if (Spy.spy != null && !Witch.canSpellAnyone) untargetables.Add(Spy.spy);
            if (Sidekick.wasTeamRed && !Witch.canSpellAnyone) untargetables.Add(Sidekick.sidekick);
            if (Jackal.wasTeamRed && !Witch.canSpellAnyone) untargetables.Add(Jackal.jackal);
        }
        Witch.currentTarget = setTarget(onlyCrewmates: !Witch.canSpellAnyone, untargetablePlayers: untargetables);
        setPlayerOutline(Witch.currentTarget, Witch.color);
    }

    static void ninjaSetTarget()
    {
        if (Ninja.ninja == null || Ninja.ninja != PlayerControl.LocalPlayer) return;
        List<PlayerControl> untargetables = [];
        if (Spy.spy != null && !Spy.impostorsCanKillAnyone) untargetables.Add(Spy.spy);
        if (Mini.mini != null && !Mini.isGrownUp()) untargetables.Add(Mini.mini);
        if (Sidekick.wasTeamRed && !Spy.impostorsCanKillAnyone) untargetables.Add(Sidekick.sidekick);
        if (Jackal.wasTeamRed && !Spy.impostorsCanKillAnyone) untargetables.Add(Jackal.jackal);
        Ninja.currentTarget = setTarget(onlyCrewmates: Spy.spy == null || !Spy.impostorsCanKillAnyone, untargetablePlayers: untargetables);
        setPlayerOutline(Ninja.currentTarget, Ninja.color);
    }

    static void thiefSetTarget()
    {
        if (Thief.thief == null || Thief.thief != PlayerControl.LocalPlayer) return;
        List<PlayerControl> untargetables = [];
        if (Mini.mini != null && !Mini.isGrownUp()) untargetables.Add(Mini.mini);
        Thief.currentTarget = setTarget(onlyCrewmates: false, untargetablePlayers: untargetables);
        setPlayerOutline(Thief.currentTarget, Thief.color);
    }




    static void baitUpdate()
    {
        if (!Bait.active.Any()) return;

        // Bait report
        foreach (KeyValuePair<DeadPlayer, float> entry in new Dictionary<DeadPlayer, float>(Bait.active))
        {
            Bait.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
            if (entry.Value <= 0)
            {
                Bait.active.Remove(entry.Key);
                if (entry.Key.killerIfExisting != null && entry.Key.killerIfExisting.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    Helpers.handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                    RPCProcedure.uncheckedCmdReportDeadBody(entry.Key.killerIfExisting.PlayerId, entry.Key.player.PlayerId);

                    using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedCmdReportDeadBody);
                    writer.Write(entry.Key.killerIfExisting.PlayerId);
                    writer.Write(entry.Key.player.PlayerId);
                }
            }
        }
    }

    static void bloodyUpdate()
    {
        if (!Bloody.active.Any()) return;
        foreach (KeyValuePair<byte, float> entry in new Dictionary<byte, float>(Bloody.active))
        {
            PlayerControl player = Helpers.playerById(entry.Key);
            PlayerControl bloodyPlayer = Helpers.playerById(Bloody.bloodyKillerMap[player.PlayerId]);

            Bloody.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
            if (entry.Value <= 0 || player.Data.IsDead)
            {
                Bloody.active.Remove(entry.Key);
                continue;  // Skip the creation of the next blood drop, if the killer is dead or the time is up
            }
            new Bloodytrail(player, bloodyPlayer);
        }
    }

    // Mini set adapted button cooldown for Vampire, Sheriff, Jackal, Sidekick, Warlock, Cleaner
    public static void miniCooldownUpdate()
    {
        if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini)
        {
            var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            Sheriff.sheriffKillButton.MaxTimer = Sheriff.cooldown * multiplier;
            HudManagerStartPatch.vampireKillButton.MaxTimer = Vampire.cooldown * multiplier;
            HudManagerStartPatch.jackalKillButton.MaxTimer = Jackal.cooldown * multiplier;
            HudManagerStartPatch.sidekickKillButton.MaxTimer = Sidekick.cooldown * multiplier;
            HudManagerStartPatch.warlockCurseButton.MaxTimer = Warlock.cooldown * multiplier;
            HudManagerStartPatch.cleanerCleanButton.MaxTimer = Cleaner.cooldown * multiplier;
            HudManagerStartPatch.witchSpellButton.MaxTimer = (Witch.cooldown + Witch.currentCooldownAddition) * multiplier;
            HudManagerStartPatch.ninjaButton.MaxTimer = Ninja.cooldown * multiplier;
            HudManagerStartPatch.thiefKillButton.MaxTimer = Thief.cooldown * multiplier;
        }
    }

    public static void trapperUpdate()
    {
        if (Trapper.trapper == null || PlayerControl.LocalPlayer != Trapper.trapper || Trapper.trapper.Data.IsDead) return;
        var (playerCompleted, _) = TasksHandler.taskInfo(Trapper.trapper.Data);
        if (playerCompleted == Trapper.rechargedTasks)
        {
            Trapper.rechargedTasks += Trapper.rechargeTasksNumber;
            if (Trapper.maxCharges > Trapper.charges) Trapper.charges++;
        }
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        // Mini and Morphling shrink
        playerSizeUpdate(__instance);

        // set position of colorblind text
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            //pc.cosmetics.colorBlindText.gameObject.transform.localPosition = new Vector3(0, 0, -0.0001f);
        }

        if (PlayerControl.LocalPlayer == __instance)
        {
            // Update player outlines
            setBasePlayerOutlines();

            // Update Role Description
            Helpers.refreshRoleDescription(__instance);

            // Update Player Info
            updatePlayerInfo();

            //Update pet visibility
            setPetVisibility();

            // Time Master
            bendTimeUpdate();
            // Morphling
            morphlingSetTarget();
            // Medic
            medicSetTarget();
            // Shifter
            shifterSetTarget();
            // Tracker
            trackerSetTarget();
            // Vampire
            vampireSetTarget();
            Garlic.UpdateAll();
            Trap.Update();
            // Eraser
            eraserSetTarget();
            // Tracker
            trackerUpdate();
            // Jackal
            jackalSetTarget();
            // Sidekick
            sidekickSetTarget();
            // Impostor
            impostorSetTarget();
            // Warlock
            warlockSetTarget();
            // Check for sidekick promotion on Jackal disconnect
            sidekickCheckPromotion();
            // SecurityGuard
            securityGuardSetTarget();
            securityGuardUpdate();
            // Arsonist
            arsonistSetTarget();
            // Snitch
            snitchUpdate();
            // BountyHunter
            bountyHunterUpdate();
            // Vulture
            vultureUpdate();
            // Medium
            mediumSetTarget();
            // Morphling and Camouflager
            morphlingAndCamouflagerUpdate();
            // Lawyer
            lawyerUpdate();
            // Pursuer
            pursuerSetTarget();
            // Witch
            witchSetTarget();
            // Ninja
            ninjaSetTarget();
            NinjaTrace.UpdateAll();
            ninjaUpdate();
            // Thief
            thiefSetTarget();
            // yoyo
            Silhouette.UpdateAll();

            hackerUpdate();
            swapperUpdate();
            // Hacker
            hackerUpdate();
            // Trapper
            trapperUpdate();

            // -- MODIFIER--
            // Bait
            baitUpdate();
            // Bloody
            bloodyUpdate();
            // mini (for the cooldowns)
            miniCooldownUpdate();
            // Chameleon (invis stuff, timers)
            Chameleon.update();
            Bomb.update();
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
class PlayerPhysicsWalkPlayerToPatch
{
    private static Vector2 offset = Vector2.zero;
    public static void Prefix(PlayerPhysics __instance)
    {
        bool correctOffset = Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && (__instance.myPlayer == Mini.mini || (Morphing.morphing != null && __instance.myPlayer == Morphing.morphing && Morphing.morphTarget == Mini.mini && Morphing.morphTimer > 0f));
        correctOffset = correctOffset && !(Mini.mini == Morphing.morphing && Morphing.morphTimer > 0f);
        if (correctOffset)
        {
            float currentScaling = (Mini.growingProgress() + 1) * 0.5f;
            __instance.myPlayer.Collider.offset = currentScaling * Mini.defaultColliderOffset * Vector2.down;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
class PlayerControlCmdReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        Helpers.handleVampireBiteOnBodyReport();
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
class BodyReportPatch
{
    static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        // Medic or Detective report
        bool isMedicReport = Medic.exists && PlayerControl.LocalPlayer.isRole(RoleId.Medic) && __instance.isRole(RoleId.Medic);
        bool isDetectiveReport = Detective.exists && PlayerControl.LocalPlayer.isRole(RoleId.Detective) && Detective.allPlayers.Contains(__instance);
        if (isMedicReport || isDetectiveReport)
        {
            DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();

            if (deadPlayer != null && deadPlayer.killerIfExisting != null)
            {
                float timeSinceDeath = (float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds;
                string msg = "";

                if (isMedicReport)
                {
                    msg = $"Body Report: Killed {Math.Round(timeSinceDeath / 1000)}s ago!";
                }
                else if (isDetectiveReport)
                {
                    if (timeSinceDeath < Detective.reportNameDuration * 1000)
                    {
                        msg = $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.Data.PlayerName}!";
                    }
                    else if (timeSinceDeath < Detective.reportColorDuration * 1000)
                    {
                        var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting) ? "lighter" : "darker";
                        msg = $"Body Report: The killer appears to be a {typeOfColor} color!";
                    }
                    else
                    {
                        msg = $"Body Report: The corpse is too old to gain information from!";
                    }
                }

                if (!string.IsNullOrWhiteSpace(msg))
                {
                    if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                    {
                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);

                        // Ghost Info
                        using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write((byte)GhostInfoTypes.DetectiveOrMedicInfo);
                        writer.Write(msg);
                    }
                    if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class MurderPlayerPatch
{
    public static bool resetToCrewmate = false;
    public static bool resetToDead = false;

    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Allow everyone to murder players
        resetToCrewmate = !__instance.Data.Role.IsImpostor;
        resetToDead = __instance.Data.IsDead;
        __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
        __instance.Data.IsDead = false;
    }

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Collect dead player info
        DeadPlayer deadPlayer = new(target, DateTime.UtcNow, CustomDeathReason.Kill, __instance);
        GameHistory.deadPlayers.Add(deadPlayer);

        // Reset killer to crewmate if resetToCrewmate
        if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
        if (resetToDead) __instance.Data.IsDead = true;

        // Remove fake tasks when player dies
        if (target.hasFakeTasks() || target == Lawyer.lawyer || target == Pursuer.pursuer || target == Thief.thief)
            target.clearAllTasks();

        // First kill (set before lover suicide)
        if (MapOptions.firstKillName == "") MapOptions.firstKillName = target.Data.PlayerName;

        // Sidekick promotion trigger on murder
        if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && target == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer)
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.SidekickPromotes);
            RPCProcedure.sidekickPromotes();
        }

        // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
        if (target == Lawyer.target && AmongUsClient.Instance.AmHost && Lawyer.lawyer != null)
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.LawyerPromotesToPursuer);
            RPCProcedure.lawyerPromotesToPursuer();
        }

        // Seer show flash and add dead player position
        if (Seer.exists)
        {
            foreach (var seer in Seer.players)
            {
                if ((PlayerControl.LocalPlayer.isRole(RoleId.Seer) || Helpers.shouldShowGhostInfo()) && !seer.player.isDead() && !target.isRole(RoleId.Seer) && Seer.mode <= 1)
                {
                    Helpers.showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f), message: "Seer Info: Someone Died");
                }
            }
            Seer.deadBodyPositions?.Add(target.transform.position);
        }

        // Tracker store body positions
        if (Tracker.deadBodyPositions != null) Tracker.deadBodyPositions.Add(target.transform.position);

        // Medium add body
        if (Medium.deadBodies != null)
        {
            Medium.futureDeadBodies.Add(new Tuple<DeadPlayer, Vector3>(deadPlayer, target.transform.position));
        }

        // Set bountyHunter cooldown
        if (BountyHunter.bountyHunter != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter && __instance == BountyHunter.bountyHunter)
        {
            if (target == BountyHunter.bounty)
            {
                BountyHunter.bountyHunter.SetKillTimer(BountyHunter.bountyKillCooldown);
                BountyHunter.bountyUpdateTimer = 0f; // Force bounty update
            }
            else
                BountyHunter.bountyHunter.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + BountyHunter.punishmentTime);
        }

        // Mini Set Impostor Mini kill timer (Due to mini being a modifier, all "SetKillTimers" must have happened before this!)
        if (Mini.mini != null && __instance == Mini.mini && __instance == PlayerControl.LocalPlayer)
        {
            float multiplier = 1f;
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            Mini.mini.SetKillTimer(__instance.killTimer * multiplier);
        }

        // Cleaner Button Sync
        if (Cleaner.cleaner != null && PlayerControl.LocalPlayer == Cleaner.cleaner && __instance == Cleaner.cleaner && HudManagerStartPatch.cleanerCleanButton != null)
            HudManagerStartPatch.cleanerCleanButton.Timer = Cleaner.cleaner.killTimer;

        // Witch Button Sync
        if (Witch.triggerBothCooldowns && Witch.witch != null && PlayerControl.LocalPlayer == Witch.witch && __instance == Witch.witch && HudManagerStartPatch.witchSpellButton != null)
            HudManagerStartPatch.witchSpellButton.Timer = HudManagerStartPatch.witchSpellButton.MaxTimer;

        // Warlock Button Sync
        if (Warlock.warlock != null && PlayerControl.LocalPlayer == Warlock.warlock && __instance == Warlock.warlock && HudManagerStartPatch.warlockCurseButton != null)
        {
            if (Warlock.warlock.killTimer > HudManagerStartPatch.warlockCurseButton.Timer)
            {
                HudManagerStartPatch.warlockCurseButton.Timer = Warlock.warlock.killTimer;
            }
        }
        // Ninja Button Sync
        if (Ninja.ninja != null && PlayerControl.LocalPlayer == Ninja.ninja && __instance == Ninja.ninja && HudManagerStartPatch.ninjaButton != null)
            HudManagerStartPatch.ninjaButton.Timer = HudManagerStartPatch.ninjaButton.MaxTimer;

        // Bait
        if (Bait.bait.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
        {
            float reportDelay = (float)rnd.Next((int)Bait.reportDelayMin, (int)Bait.reportDelayMax + 1);
            Bait.active.Add(deadPlayer, reportDelay);

            if (Bait.showKillFlash && __instance == PlayerControl.LocalPlayer) Helpers.showFlash(new Color(204f / 255f, 102f / 255f, 0f / 255f));
        }

        // Add Bloody Modifier
        if (Bloody.bloody.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.Bloody);
            writer.Write(__instance.PlayerId);
            writer.Write(target.PlayerId);
            RPCProcedure.bloody(__instance.PlayerId, target.PlayerId);
        }

        // VIP Modifier
        if (Vip.vip.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
        {
            Color color = Color.yellow;
            if (Vip.showColor)
            {
                color = Color.white;
                if (target.Data.Role.IsImpostor) color = Color.red;
                else if (RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault().isNeutral) color = Color.blue;
            }
            Helpers.showFlash(color, 1.5f);
        }

        // Snitch
        if (Snitch.snitch != null && PlayerControl.LocalPlayer.PlayerId == Snitch.snitch.PlayerId && MapBehaviourPatch.herePoints.Keys.Any(x => x == target.PlayerId))
        {
            foreach (var a in MapBehaviourPatch.herePoints.Where(x => x.Key == target.PlayerId))
            {
                UnityEngine.Object.Destroy(a.Value);
                MapBehaviourPatch.herePoints.Remove(a.Key);
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
class PlayerControlSetCoolDownPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
        if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown <= 0f) return false;
        float multiplier = 1f;
        float addition = 0f;
        if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
        if (BountyHunter.bountyHunter != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter) addition = BountyHunter.punishmentTime;

        __instance.killTimer = Mathf.Clamp(time, 0f, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier + addition);
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier + addition);
        return false;
    }
}

[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
class KillAnimationCoPerformKillPatch
{
    public static bool hideNextAnimation = false;
    public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source, [HarmonyArgument(1)] ref PlayerControl target)
    {
        if (hideNextAnimation)
            source = target;
        hideNextAnimation = false;
    }
}

[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
class KillAnimationSetMovementPatch
{
    private static int? colorId = null;
    public static void Prefix(PlayerControl source, bool canMove)
    {
        Color color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
        if (Morphing.morphing != null && source.Data.PlayerId == Morphing.morphing.PlayerId)
        {
            var index = Palette.PlayerColors.IndexOf(color);
            if (index != -1) colorId = index;
        }
    }

    public static void Postfix(PlayerControl source, bool canMove)
    {
        if (colorId.HasValue) source.RawSetColor(colorId.Value);
        colorId = null;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class ExilePlayerPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        // Collect dead player info
        DeadPlayer deadPlayer = new(__instance, DateTime.UtcNow, CustomDeathReason.Exile, null);
        GameHistory.deadPlayers.Add(deadPlayer);


        // Remove fake tasks when player dies
        if (__instance.hasFakeTasks() || __instance == Lawyer.lawyer || __instance == Pursuer.pursuer || __instance == Thief.thief)
            __instance.clearAllTasks();

        // Sidekick promotion trigger on exile
        if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && __instance == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer)
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.SidekickPromotes);
            RPCProcedure.sidekickPromotes();
        }

        // Pursuer promotion trigger on exile & suicide (the host sends the call such that everyone recieves the update before a possible game End)
        if (Lawyer.lawyer != null && __instance == Lawyer.target)
        {
            PlayerControl lawyer = Lawyer.lawyer;
            if (AmongUsClient.Instance.AmHost && ((Lawyer.target.isRole(RoleId.Jester) && !Lawyer.isProsecutor) || Lawyer.targetWasGuessed))
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.LawyerPromotesToPursuer);
                RPCProcedure.lawyerPromotesToPursuer();
            }

            if (!Lawyer.targetWasGuessed && !Lawyer.isProsecutor)
            {
                if (Lawyer.lawyer != null) Lawyer.lawyer.Exiled();
                if (Pursuer.pursuer != null) Pursuer.pursuer.Exiled();

                using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write((byte)GhostInfoTypes.DeathReasonAndKiller);
                writer.Write(lawyer.PlayerId);
                writer.Write((byte)CustomDeathReason.LawyerSuicide);
                writer.Write(lawyer.PlayerId);

                GameHistory.overrideDeathReasonAndKiller(lawyer, CustomDeathReason.LawyerSuicide, lawyer);  // TODO: only executed on host?!
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        bool shouldInvert = Invert.invert.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count > 0 && Invert.meetings > 0;
        if (__instance.AmOwner &&
            AmongUsClient.Instance &&
            AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started &&
            !PlayerControl.LocalPlayer.Data.IsDead &&
            shouldInvert &&
            GameData.Instance &&
            __instance.myPlayer.CanMove)
            __instance.body.velocity *= -1;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.IsFlashlightEnabled))]
public static class IsFlashlightEnabledPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
            return true;
        __result = false;
        if (!PlayerControl.LocalPlayer.Data.IsDead && Lighter.exists && PlayerControl.LocalPlayer.isRole(RoleId.Lighter))
        {
            __result = true;
        }

        return false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AdjustLighting))]
public static class AdjustLight
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (__instance == null || PlayerControl.LocalPlayer == null || Lighter.exists) return true;

        bool hasFlashlight = !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.isRole(RoleId.Lighter);
        __instance.SetFlashlightInputMethod();
        __instance.lightSource.SetupLightingForGameplay(hasFlashlight, Lighter.flashlightWidth, __instance.TargetFlashlight.transform);

        return false;
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
public static class GameDataHandleDisconnectPatch
{
    public static void Prefix(GameData __instance, PlayerControl player, DisconnectReasons reason)
    {
        if (MeetingHud.Instance)
        {
            MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, player.PlayerId);
        }
    }
}