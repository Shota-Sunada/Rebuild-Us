using HarmonyLib;
using Hazel;
using static RebuildUs.RebuildUs;
using static RebuildUs.HudManagerStartPatch;
using static RebuildUs.GameHistory;
using static RebuildUs.MapOptions;
using RebuildUs.Objects;
using RebuildUs.Patches;
using RebuildUs.Roles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using RebuildUs.Utilities;
using RebuildUs.CustomGameModes;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using RebuildUs.Modules;

namespace RebuildUs;

public static class RPCProcedure
{
    public static RPCSender SendRPC(uint netId, CustomRPC callId, int targetId = -1)
    {
        return new RPCSender(netId, callId, targetId);
    }

    public static RPCSender SendRPC(CustomRPC callId, int targetId = -1)
    {
        return new RPCSender(PlayerControl.LocalPlayer.NetId, callId, targetId);
    }

    // Main Controls
    public static void resetVariables()
    {
        Garlic.clearGarlics();
        JackInTheBox.clearJackInTheBoxes();
        NinjaTrace.clearTraces();
        Silhouette.clearSilhouettes();
        Portal.clearPortals();
        Bloodytrail.resetSprites();
        Trap.clearTraps();
        clearAndReloadMapOptions();
        clearAndReloadRoles();
        clearGameHistory();
        setCustomButtonCooldowns();
        reloadPluginOptions();
        Helpers.toggleZoom(reset: true);
        GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 0;
        SurveillanceMinigamePatch.nightVisionOverlays = null;
        EventUtility.clearAndReload();
        MapBehaviourPatch.clearAndReload();
        HudManagerUpdate.CloseSummary();
    }

    public static void HandleShareOptions(byte numberOfOptions, MessageReader reader)
    {
        try
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                uint optionId = reader.ReadPackedUInt32();
                uint selection = reader.ReadPackedUInt32();
                var option = CustomOption.options.First(option => option.Key == (int)optionId).Value;
                option.updateSelection((int)optionId, (int)selection, i == numberOfOptions - 1);
            }
        }
        catch (Exception e)
        {
            RebuildUsPlugin.Instance.Logger.LogError("Error while deserializing options: " + e.Message);
        }
    }

    public static void forceEnd()
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data.Role.IsImpostor)
            {

                GameData.Instance.GetPlayerById(player.PlayerId); // player.RemoveInfected(); (was removed in 2022.12.08, no idea if we ever need that part again, replaced by these 2 lines.)
                player.CoSetRole(RoleTypes.Crewmate, true);

                player.MurderPlayer(player);
                player.Data.IsDead = true;
            }
        }
    }

    public static void shareGamemode(byte gm)
    {
        MapOptions.gameMode = (CustomGamemodes)gm;
        LobbyViewSettingsPatch.currentButtons?.ForEach(x => x.gameObject?.Destroy());
        LobbyViewSettingsPatch.currentButtons?.Clear();
        LobbyViewSettingsPatch.currentButtonTypes?.Clear();
    }

    public static void stopStart(byte playerId)
    {
        if (!CustomOptionHolder.anyPlayerCanStopStart.getBool())
            return;
        SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
        if (AmongUsClient.Instance.AmHost)
        {
            GameStartManager.Instance.ResetStartState();
            PlayerControl.LocalPlayer.RpcSendChat($"{Helpers.playerById(playerId).Data.PlayerName} stopped the game start!");
        }
    }

    public static void workaroundSetRoles(byte numberOfRoles, MessageReader reader)
    {
        for (int i = 0; i < numberOfRoles; i++)
        {
            byte playerId = (byte)reader.ReadPackedUInt32();
            byte roleId = (byte)reader.ReadPackedUInt32();
            try
            {
                setRole(roleId, playerId);
            }
            catch (Exception e)
            {
                RebuildUsPlugin.Instance.Logger.LogError("Error while deserializing roles: " + e.Message);
            }
        }

    }

    public static void setRole(byte roleId, byte playerId)
    {
        PlayerControl.AllPlayerControls.ToArray().DoIf(
            x => x.PlayerId == playerId,
            x => x.setRole((RoleId)roleId)
        );
    }

    public static void setModifier(byte modifierId, byte playerId, byte flag)
    {
        PlayerControl player = Helpers.playerById(playerId);
        switch ((RoleId)modifierId)
        {
            case RoleId.Bait:
                Bait.bait.Add(player);
                break;
            case RoleId.Bloody:
                Bloody.bloody.Add(player);
                break;
            case RoleId.AntiTeleport:
                AntiTeleport.antiTeleport.Add(player);
                break;
            case RoleId.Tiebreaker:
                Tiebreaker.tiebreaker = player;
                break;
            case RoleId.Sunglasses:
                Sunglasses.sunglasses.Add(player);
                break;
            case RoleId.Mini:
                Mini.mini = player;
                break;
            case RoleId.Vip:
                Vip.vip.Add(player);
                break;
            case RoleId.Invert:
                Invert.invert.Add(player);
                break;
            case RoleId.Chameleon:
                Chameleon.chameleon.Add(player);
                break;
            case RoleId.Armored:
                Armored.armored = player;
                break;
            case RoleId.Shifter:
                Shifter.shifter = player;
                break;
        }
    }

    public static void versionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
    {
        System.Version ver;
        if (revision < 0)
            ver = new System.Version(major, minor, build);
        else
            ver = new System.Version(major, minor, build, revision);
        GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
    }

    public static void useUncheckedVent(int ventId, byte playerId, byte isEnter)
    {
        PlayerControl player = Helpers.playerById(playerId);
        if (player == null) return;
        // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
        MessageReader reader = new();
        byte[] bytes = BitConverter.GetBytes(ventId);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        reader.Buffer = bytes;
        reader.Length = bytes.Length;

        JackInTheBox.startAnimation(ventId);
        player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
    }

    public static void uncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        PlayerControl source = Helpers.playerById(sourceId);
        PlayerControl target = Helpers.playerById(targetId);
        if (source != null && target != null)
        {
            if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
            source.MurderPlayer(target);
        }
    }

    public static void uncheckedCmdReportDeadBody(byte sourceId, byte targetId)
    {
        PlayerControl source = Helpers.playerById(sourceId);
        var t = targetId == Byte.MaxValue ? null : Helpers.playerById(targetId).Data;
        if (source != null) source.ReportDeadBody(t);
    }

    public static void uncheckedExilePlayer(byte targetId)
    {
        PlayerControl target = Helpers.playerById(targetId);
        if (target != null) target.Exiled();
    }

    public static void dynamicMapOption(byte mapId)
    {
        GameOptionsManager.Instance.currentNormalGameOptions.MapId = mapId;
    }

    public static void setGameStarting()
    {
        GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 5f;
    }

    // Role functionality

    public static void engineerFixLights()
    {
        SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
    }

    public static void engineerFixSubmergedOxygen()
    {
        SubmergedCompatibility.RepairOxygen();
    }

    public static void engineerUsedRepair(byte playerId)
    {
        var player = Helpers.playerById(playerId);
        Engineer.getRole(player).remainingFixes--;
        if (Helpers.shouldShowGhostInfo())
        {
            Helpers.showFlash(Engineer.Color, 0.5f, "Engineer Fix"); ;
        }
    }

    public static void cleanBody(byte playerId, byte cleaningPlayerId)
    {
        if (Medium.futureDeadBodies != null)
        {
            var deadBody = Medium.futureDeadBodies.Find(x => x.Item1.player.PlayerId == playerId).Item1;
            if (deadBody != null) deadBody.wasCleaned = true;
        }

        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; i++)
        {
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
            {
                UnityEngine.Object.Destroy(array[i].gameObject);
            }
        }
        if (Vulture.vulture != null && cleaningPlayerId == Vulture.vulture.PlayerId)
        {
            Vulture.eatenBodies++;
            if (Vulture.eatenBodies == Vulture.vultureNumberToWin)
            {
                Vulture.triggerVultureWin = true;
            }
        }
    }

    public static void sheriffKill(byte sheriffId, byte targetId, bool misfire)
    {
        var sheriff = Helpers.playerById(sheriffId);
        var target = Helpers.playerById(targetId);
        if (sheriff == null || target == null) return;

        var role = Sheriff.getRole(sheriff);
        if (role != null)
        {
            role.numShots--;
        }

        if (misfire)
        {
            sheriff.MurderPlayer(sheriff);
            finalStatuses[sheriffId] = FinalStatus.Misfire;

            if (!Sheriff.misfireKillsTarget) return;
            finalStatuses[targetId] = FinalStatus.Misfire;
        }

        sheriff.MurderPlayer(target);
    }

    public static void timeMasterRewindTime()
    {
        TimeMaster.shieldActive = false; // Shield is no longer active when rewinding
        if (TimeMaster.exists && PlayerControl.LocalPlayer.isRole(RoleId.TimeMaster))
        {
            TimeMaster.resetTimeMasterButton();
        }
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>((p) =>
        {
            if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
        })));

        if (!TimeMaster.exists || PlayerControl.LocalPlayer.isRole(RoleId.TimeMaster)) return; // Time Master himself does not rewind

        TimeMaster.isRewinding = true;

        if (MapBehaviour.Instance)
            MapBehaviour.Instance.Close();
        if (Minigame.Instance)
            Minigame.Instance.ForceClose();
        PlayerControl.LocalPlayer.moveable = false;
    }

    public static void timeMasterShield()
    {
        TimeMaster.shieldActive = true;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.shieldDuration, new Action<float>((p) =>
        {
            if (p == 1f) TimeMaster.shieldActive = false;
        })));
    }

    public static void medicSetShielded(byte medicId, byte shieldedId)
    {
        var player = Helpers.playerById(medicId);
        var medic = Medic.getRole(player);

        medic.usedShield = true;
        medic.shielded = Helpers.playerById(shieldedId);
        medic.futureShielded = null;
    }

    public static void shieldedMurderAttempt(byte medicId)
    {
        var player = Helpers.playerById(medicId);
        var medic = Medic.getRole(player);

        if (!Medic.exists || medic == null || medic.shielded == null) return;

        bool isShieldedAndShow = medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded;
        isShieldedAndShow = isShieldedAndShow && (medic.meetingAfterShielding || !Medic.showShieldAfterMeeting);  // Dont show attempt, if shield is not shown yet
        bool isMedicAndShow = PlayerControl.LocalPlayer.isRole(RoleId.Medic) && Medic.showAttemptToMedic;

        if (isShieldedAndShow || isMedicAndShow || Helpers.shouldShowGhostInfo()) Helpers.showFlash(Palette.ImpostorRed, duration: 0.5f, "Failed Murder Attempt on Shielded Player");
    }

    public static void shifterShift(byte targetId)
    {
        PlayerControl oldShifter = Shifter.shifter;
        PlayerControl player = Helpers.playerById(targetId);
        if (player == null || oldShifter == null) return;

        Shifter.futureShift = null;
        if (!Shifter.isNeutral)
        {
            Shifter.clearAndReload();
        }

        // Suicide (exile) when impostor or impostor variants
        if (!Shifter.isNeutral && (player.Data.Role.IsImpostor || player.isNeutral()) || oldShifter.Data.IsDead || player.hasModifier(ModifierId.Madmate))
        {
            oldShifter.Exiled();
            finalStatuses[oldShifter.PlayerId] = FinalStatus.Suicide;
            overrideDeathReasonAndKiller(oldShifter, CustomDeathReason.Shift, player);
            if (oldShifter == Lawyer.target && AmongUsClient.Instance.AmHost && Lawyer.lawyer != null)
            {
                using var writer = SendRPC(CustomRPC.LawyerPromotesToPursuer);
                lawyerPromotesToPursuer();
            }
            return;
        }

        if (Shifter.shiftModifiers)
        {
            // Switch shield
            foreach (var medic in Medic.players)
            {
                if (medic.shielded != null && medic.shielded == player)
                {
                    medic.shielded = oldShifter;
                }
                else if (medic.shielded != null && medic.shielded == oldShifter)
                {
                    medic.shielded = player;
                }
            }

            player.swapModifiers(oldShifter);
            Lovers.swapLovers(oldShifter, player);
        }

        player.swapRoles(oldShifter);

        if (Shifter.isNeutral)
        {
            Shifter.shifter = player;
            Shifter.pastShifters.Add(oldShifter.PlayerId);

            if (player.Data.Role.IsImpostor)
            {
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(oldShifter, RoleTypes.Impostor);
            }
        }

        // Set cooldowns to max for both players
        if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
            CustomButton.ResetAllCooldowns();
    }

    public static void swapperSwap(byte playerId1, byte playerId2)
    {
        if (MeetingHud.Instance)
        {
            Swapper.playerId1 = playerId1;
            Swapper.playerId2 = playerId2;
        }
    }

    public static void morphingMorph(byte playerId)
    {
        PlayerControl target = Helpers.playerById(playerId);
        if (Morphing.morphing == null || target == null) return;

        Morphing.morphTimer = Morphing.duration;
        Morphing.morphTarget = target;
        if (Camouflager.camouflageTimer <= 0f)
            Morphing.morphing.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
    }

    public static void camouflagerCamouflage()
    {
        if (Camouflager.camouflager == null) return;

        Camouflager.camouflageTimer = Camouflager.duration;
        if (Helpers.MushroomSabotageActive()) return; // Dont overwrite the fungle "camo"
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            player.setLook("", 6, "", "", "", "");

    }

    public static void vampireSetBitten(byte targetId, byte performReset)
    {
        if (performReset != 0)
        {
            Vampire.bitten = null;
            return;
        }

        if (Vampire.vampire == null) return;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == targetId && !player.Data.IsDead)
            {
                Vampire.bitten = player;
            }
        }
    }

    public static void placeGarlic(byte[] buff)
    {
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        new Garlic(position);
    }

    public static void trackerUsedTracker(byte targetId)
    {
        Tracker.usedTracker = true;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            if (player.PlayerId == targetId)
                Tracker.tracked = player;
    }

    public static void jackalCreatesSidekick(byte targetId)
    {
        PlayerControl player = Helpers.playerById(targetId);
        if (player == null) return;
        if (Lawyer.target == player && Lawyer.isProsecutor && Lawyer.lawyer != null && !Lawyer.lawyer.Data.IsDead) Lawyer.isProsecutor = false;

        if (!Jackal.canCreateSidekickFromImpostor && player.Data.Role.IsImpostor)
        {
            Jackal.fakeSidekick = player;
        }
        else
        {
            bool wasSpy = Spy.spy != null && player == Spy.spy;
            bool wasImpostor = player.Data.Role.IsImpostor;  // This can only be reached if impostors can be sidekicked.
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            if (player == Lawyer.lawyer && Lawyer.target != null)
            {
                Transform playerInfoTransform = Lawyer.target.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
            erasePlayerRoles(player.PlayerId, RoleId.Sidekick, true);
            Sidekick.sidekick = player;
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
            if (wasSpy || wasImpostor) Sidekick.wasTeamRed = true;
            Sidekick.wasSpy = wasSpy;
            Sidekick.wasImpostor = wasImpostor;
        }
        Jackal.canCreateSidekick = false;
    }

    public static void sidekickPromotes()
    {
        Jackal.removeCurrentJackal();
        Jackal.jackal = Sidekick.sidekick;
        Jackal.canCreateSidekick = Jackal.jackalPromotedFromSidekickCanCreateSidekick;
        Jackal.wasTeamRed = Sidekick.wasTeamRed;
        Jackal.wasSpy = Sidekick.wasSpy;
        Jackal.wasImpostor = Sidekick.wasImpostor;
        Sidekick.clearAndReload();
        return;
    }

    public static void erasePlayerRoles(byte playerId, RoleId newRole = RoleId.NoRole, bool ignoreModifier = true)
    {
        PlayerControl player = Helpers.playerById(playerId);
        if (player == null || !player.canBeErased()) return;

        // Don't give a former neutral role tasks because that destroys the balance.
        if (player.isNeutral())
        {
            player.clearAllTasks();
        }

        player.eraseAllRoles();
        if (!ignoreModifier) player.eraseAllModifiers(newRole);
    }

    public static void setFutureErased(byte playerId)
    {
        PlayerControl player = Helpers.playerById(playerId);
        if (Eraser.futureErased == null)
            Eraser.futureErased = [];
        if (player != null)
        {
            Eraser.futureErased.Add(player);
        }
    }

    public static void setFutureShifted(byte playerId)
    {
        Shifter.futureShift = Helpers.playerById(playerId);
    }

    public static void setFutureShielded(byte medicId, byte playerId)
    {
        var player = Helpers.playerById(medicId);
        var medic = Medic.getRole(player);

        medic.futureShielded = Helpers.playerById(playerId);
        medic.usedShield = true;
    }

    public static void setFutureSpelled(byte playerId)
    {
        PlayerControl player = Helpers.playerById(playerId);
        if (Witch.futureSpelled == null)
            Witch.futureSpelled = [];
        if (player != null)
        {
            Witch.futureSpelled.Add(player);
        }
    }

    public static void placeNinjaTrace(byte[] buff)
    {
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        new NinjaTrace(position, Ninja.traceTime);
        if (PlayerControl.LocalPlayer != Ninja.ninja)
            Ninja.ninjaMarked = null;
    }

    public static void setInvisible(byte playerId, byte flag)
    {
        PlayerControl target = Helpers.playerById(playerId);
        if (target == null) return;
        if (flag == byte.MaxValue)
        {
            target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);

            if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive()) target.setDefaultLook();
            Ninja.isInvisble = false;
            return;
        }

        target.setLook("", 6, "", "", "", "");
        Color color = Color.clear;
        bool canSee = PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead;
        if (canSee) color.a = 0.1f;
        target.cosmetics.currentBodySprite.BodySprite.color = color;
        target.cosmetics.colorBlindText.gameObject.SetActive(false);
        target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(canSee ? 0.1f : 0f);
        Ninja.invisibleTimer = Ninja.invisibleDuration;
        Ninja.isInvisble = true;
    }

    public static void placePortal(byte[] buff)
    {
        Vector3 position = Vector2.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        new Portal(position);
    }

    public static void usePortal(byte playerId, byte exit)
    {
        Portal.startTeleport(playerId, exit);
    }

    public static void placeJackInTheBox(byte[] buff)
    {
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        new JackInTheBox(position);
    }

    public static void lightsOut()
    {
        Trickster.lightsOutTimer = Trickster.lightsOutDuration;
        // If the local player is impostor indicate lights out
        if (Helpers.hasImpVision(GameData.Instance.GetPlayerById(PlayerControl.LocalPlayer.PlayerId)))
        {
            new CustomMessage("Lights are out", Trickster.lightsOutDuration);
        }
    }

    public static void placeCamera(byte[] buff)
    {
        var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>();
        if (referenceCamera == null) return; // Mira HQ

        SecurityGuard.remainingScrews -= SecurityGuard.camPrice;
        SecurityGuard.placedCameras++;

        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));

        var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
        camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
        camera.CamName = $"Security Camera {SecurityGuard.placedCameras}";
        camera.Offset = new Vector3(0f, 0f, camera.Offset.z);
        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 || GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4) camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship

        if (SubmergedCompatibility.IsSubmerged)
        {
            // remove 2d box collider of console, so that no barrier can be created. (irrelevant for now, but who knows... maybe we need it later)
            var fixConsole = camera.transform.FindChild("FixConsole");
            if (fixConsole != null)
            {
                var boxCollider = fixConsole.GetComponent<BoxCollider2D>();
                if (boxCollider != null) UnityEngine.Object.Destroy(boxCollider);
            }
        }


        if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard)
        {
            camera.gameObject.SetActive(true);
            camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            camera.gameObject.SetActive(false);
        }
        MapOptions.camerasToAdd.Add(camera);
    }

    public static void sealVent(int ventId)
    {
        Vent vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
        if (vent == null) return;

        SecurityGuard.remainingScrews -= SecurityGuard.ventPrice;
        if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard)
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
            rend.color = new Color(1f, 1f, 1f, 0.5f);
            vent.name = "FutureSealedVent_" + vent.name;
        }

        MapOptions.ventsToSeal.Add(vent);
    }

    public static void arsonistWin()
    {
        Arsonist.triggerArsonistWin = true;
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            if (p != Arsonist.arsonist && !p.Data.IsDead)
            {
                p.Exiled();
                overrideDeathReasonAndKiller(p, CustomDeathReason.Arson, Arsonist.arsonist);
            }
        }
    }

    public static void lawyerSetTarget(byte playerId)
    {
        Lawyer.target = Helpers.playerById(playerId);
    }

    public static void lawyerPromotesToPursuer()
    {
        PlayerControl player = Lawyer.lawyer;
        PlayerControl client = Lawyer.target;
        Lawyer.clearAndReload(false);

        Pursuer.pursuer = player;

        if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null)
        {
            Transform playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
            TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
            if (playerInfo != null) playerInfo.text = "";
        }
    }

    public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId)
    {
        PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
        if (dyingTarget == null) return;
        if (Lawyer.target != null && dyingTarget == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses
        PlayerControl dyingLoverPartner = Lovers.bothDie ? dyingTarget.getPartner() : null; // Lover check
        if (Lawyer.target != null && dyingLoverPartner == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses

        PlayerControl guesser = Helpers.playerById(killerId);
        if (Thief.thief != null && Thief.thief.PlayerId == killerId && Thief.canStealWithGuess)
        {
            RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
            if (!Thief.thief.Data.IsDead && !Thief.isFailedThiefKill(dyingTarget, guesser, roleInfo))
            {
                RPCProcedure.thiefStealsRole(dyingTarget.PlayerId);
            }
        }

        bool lawyerDiedAdditionally = false;
        if (Lawyer.lawyer != null && !Lawyer.isProsecutor && Lawyer.lawyer.PlayerId == killerId && Lawyer.target != null && Lawyer.target.PlayerId == dyingTargetId)
        {
            // Lawyer guessed client.
            if (PlayerControl.LocalPlayer == Lawyer.lawyer)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Lawyer.lawyer.Data, Lawyer.lawyer.Data);
                if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            }
            Lawyer.lawyer.Exiled();
            lawyerDiedAdditionally = true;
            GameHistory.overrideDeathReasonAndKiller(Lawyer.lawyer, CustomDeathReason.LawyerSuicide, guesser);
        }

        dyingTarget.Exiled();
        GameHistory.overrideDeathReasonAndKiller(dyingTarget, CustomDeathReason.Guess, guesser);
        byte partnerId = dyingLoverPartner != null ? dyingLoverPartner.PlayerId : dyingTargetId;

        HandleGuesser.remainingShots(killerId, true);
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == dyingTargetId || pva.TargetPlayerId == partnerId || lawyerDiedAdditionally && Lawyer.lawyer.PlayerId == pva.TargetPlayerId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                    MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, pva.TargetPlayerId);
                }

                //Give players back their vote if target is shot dead
                if (pva.VotedFor != dyingTargetId && pva.VotedFor != partnerId && (!lawyerDiedAdditionally || Lawyer.lawyer.PlayerId != pva.VotedFor)) continue;
                pva.UnsetVote();
                var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                if (!voteAreaPlayer.AmOwner) continue;
                MeetingHud.Instance.ClearVote();

            }
            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
        if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
            if (PlayerControl.LocalPlayer == dyingTarget)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            }
            else if (dyingLoverPartner != null && PlayerControl.LocalPlayer == dyingLoverPartner)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data, dyingLoverPartner.Data);
                if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            }

        // remove shoot button from targets for all guessers and close their guesserUI
        if (HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer != guesser && !PlayerControl.LocalPlayer.Data.IsDead && HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0 && MeetingHud.Instance)
        {
            MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
            if (dyingLoverPartner != null)
                MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingLoverPartner.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

            if (MeetingHudPatch.guesserUI != null && MeetingHudPatch.guesserUIExitButton != null)
            {
                if (MeetingHudPatch.guesserCurrentTarget == dyingTarget.PlayerId)
                    MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                else if (dyingLoverPartner != null && MeetingHudPatch.guesserCurrentTarget == dyingLoverPartner.PlayerId)
                    MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            }
        }


        PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
        if (PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null && guesser != null)
        {
            RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
            string msg = $"{guesser.Data.PlayerName} guessed the role {roleInfo?.name ?? ""} for {guessedTarget.Data.PlayerName}!";
            if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg);
            if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
        }
    }

    public static void setBlanked(byte playerId, byte value)
    {
        PlayerControl target = Helpers.playerById(playerId);
        if (target == null) return;
        Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
        if (value > 0) Pursuer.blankedList.Add(target);
    }

    public static void bloody(byte killerPlayerId, byte bloodyPlayerId)
    {
        if (Bloody.active.ContainsKey(killerPlayerId)) return;
        Bloody.active.Add(killerPlayerId, Bloody.duration);
        Bloody.bloodyKillerMap.Add(killerPlayerId, bloodyPlayerId);
    }

    public static void setFirstKill(byte playerId)
    {
        PlayerControl target = Helpers.playerById(playerId);
        if (target == null) return;
        MapOptions.firstKillPlayer = target;
    }

    public static void setTiebreak()
    {
        Tiebreaker.isTiebreak = true;
    }

    public static void thiefStealsRole(byte playerId)
    {
        PlayerControl target = Helpers.playerById(playerId);
        PlayerControl thief = Thief.thief;
        if (target == null) return;
        if (target.isRole(RoleId.Sheriff)) Sheriff.getRole(target).player = thief;
        if (target == Jackal.jackal)
        {
            Jackal.jackal = thief;
            Jackal.formerJackals.Add(target);
        }
        if (target == Sidekick.sidekick)
        {
            Sidekick.sidekick = thief;
            Jackal.formerJackals.Add(target);
        }
        if (target == Guesser.evilGuesser) Guesser.evilGuesser = thief;
        if (target == Godfather.godfather) Godfather.godfather = thief;
        if (target == Mafioso.mafioso) Mafioso.mafioso = thief;
        if (target == Janitor.janitor) Janitor.janitor = thief;
        if (target == Morphing.morphing) Morphing.morphing = thief;
        if (target == Camouflager.camouflager) Camouflager.camouflager = thief;
        if (target == Vampire.vampire) Vampire.vampire = thief;
        if (target == Eraser.eraser) Eraser.eraser = thief;
        if (target == Trickster.trickster) Trickster.trickster = thief;
        if (target == Cleaner.cleaner) Cleaner.cleaner = thief;
        if (target == Warlock.warlock) Warlock.warlock = thief;
        if (target == BountyHunter.bountyHunter) BountyHunter.bountyHunter = thief;
        if (target == Witch.witch)
        {
            Witch.witch = thief;
            if (MeetingHud.Instance)
                if (Witch.witchVoteSavesTargets)  // In a meeting, if the thief guesses the witch, all targets are saved or no target is saved.
                    Witch.futureSpelled = [];
                else  // If thief kills witch during the round, remove the thief from the list of spelled people, keep the rest
                    Witch.futureSpelled.RemoveAll(x => x.PlayerId == thief.PlayerId);
        }
        if (target == Ninja.ninja) Ninja.ninja = thief;
        if (target == Bomber.bomber) Bomber.bomber = thief;
        if (target == Yoyo.yoyo)
        {
            Yoyo.yoyo = thief;
            Yoyo.markedLocation = null;
        }
        if (target.Data.Role.IsImpostor)
        {
            RoleManager.Instance.SetRole(Thief.thief, RoleTypes.Impostor);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(Thief.thief.killTimer, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
        }
        if (Lawyer.lawyer != null && target == Lawyer.target)
            Lawyer.target = thief;
        if (Thief.thief == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
        Thief.clearAndReload();
        Thief.formerThief = thief;  // After clearAndReload, else it would get reset...
    }

    public static void setTrap(byte[] buff)
    {
        if (Trapper.trapper == null) return;
        Trapper.charges -= 1;
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        new Trap(position);
    }

    public static void triggerTrap(byte playerId, byte trapId)
    {
        Trap.triggerTrap(playerId, trapId);
    }

    public static void receiveGhostInfo(byte senderId, MessageReader reader)
    {
        PlayerControl sender = Helpers.playerById(senderId);

        GhostInfoTypes infoType = (GhostInfoTypes)reader.ReadByte();
        switch (infoType)
        {
            case GhostInfoTypes.ArsonistDouse:
                Arsonist.dousedPlayers.Add(Helpers.playerById(reader.ReadByte()));
                break;
            case GhostInfoTypes.BountyTarget:
                BountyHunter.bounty = Helpers.playerById(reader.ReadByte());
                break;
            case GhostInfoTypes.NinjaMarked:
                Ninja.ninjaMarked = Helpers.playerById(reader.ReadByte());
                break;
            case GhostInfoTypes.WarlockTarget:
                Warlock.curseVictim = Helpers.playerById(reader.ReadByte());
                break;
            case GhostInfoTypes.MediumInfo:
                string mediumInfo = reader.ReadString();
                if (Helpers.shouldShowGhostInfo())
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, mediumInfo);
                break;
            case GhostInfoTypes.DetectiveOrMedicInfo:
                string detectiveInfo = reader.ReadString();
                if (Helpers.shouldShowGhostInfo())
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, detectiveInfo);
                break;
            case GhostInfoTypes.BlankUsed:
                Pursuer.blankedList.Remove(sender);
                break;
            case GhostInfoTypes.VampireTimer:
                HudManagerStartPatch.vampireKillButton.Timer = (float)reader.ReadByte();
                break;
            case GhostInfoTypes.DeathReasonAndKiller:
                GameHistory.overrideDeathReasonAndKiller(Helpers.playerById(reader.ReadByte()), (CustomDeathReason)reader.ReadByte(), Helpers.playerById(reader.ReadByte()));
                break;
        }
    }

    public static void placeBomb(byte[] buff)
    {
        if (Bomber.bomber == null) return;
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        new Bomb(position);
    }

    public static void defuseBomb()
    {
        Bomber.clearBomb();
        bomberButton.Timer = bomberButton.MaxTimer;
        bomberButton.isEffectActive = false;
        bomberButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    public static void shareRoom(byte playerId, byte roomId)
    {
        if (Snitch.playerRoomMap.ContainsKey(playerId)) Snitch.playerRoomMap[playerId] = roomId;
        else Snitch.playerRoomMap.Add(playerId, roomId);
    }

    public static void yoyoMarkLocation(byte[] buff)
    {
        if (Yoyo.yoyo == null) return;
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        Yoyo.markLocation(position);
        new Silhouette(position, -1, false);
    }

    public static void yoyoBlink(bool isFirstJump, byte[] buff)
    {
        if (Yoyo.yoyo == null || Yoyo.markedLocation == null) return;
        var markedPos = (Vector3)Yoyo.markedLocation;
        Yoyo.yoyo.NetTransform.SnapTo(markedPos);

        var markedSilhouette = Silhouette.silhouettes.FirstOrDefault(s => s.gameObject.transform.position.x == markedPos.x && s.gameObject.transform.position.y == markedPos.y);
        if (markedSilhouette != null)
            markedSilhouette.permanent = false;

        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        // Create Silhoutte At Start Position:
        if (isFirstJump)
        {
            Yoyo.markLocation(position);
            new Silhouette(position, Yoyo.blinkDuration, true);
        }
        else
        {
            new Silhouette(position, 5, true);
            Yoyo.markedLocation = null;
        }
        if (Chameleon.chameleon.Any(x => x.PlayerId == Yoyo.yoyo.PlayerId)) // Make the Yoyo visible if chameleon!
            Chameleon.lastMoved[Yoyo.yoyo.PlayerId] = Time.time;
    }

    public static void breakArmor()
    {
        if (Armored.armored == null || Armored.isBrokenArmor) return;
        Armored.isBrokenArmor = true;
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            Armored.armored.ShowFailedMurder();
        }
    }

    public static void setLovers(byte playerId1, byte playerId2)
    {
        Lovers.addCouple(Helpers.playerById(playerId1), Helpers.playerById(playerId2));
    }

    public static void updateMeeting(byte targetId, bool dead = true)
    {
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == targetId && pva.AmDead != dead)
                {
                    pva.SetDead(pva.DidReport, dead);
                    pva.Overlay.gameObject.SetActive(dead);
                }

                // Give players back their vote if target is shot dead
                if (Helpers.RefundVotes && dead)
                {
                    if (pva.VotedFor != targetId) continue;
                    pva.UnsetVote();
                    var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();
                }
            }

            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
    }

    public static void setShifterType(bool isNeutral)
    {
        Shifter.isNeutral = isNeutral;
    }

    public static void uncheckedSetTasks(byte playerId, byte[] taskTypeIds)
    {
        var player = Helpers.playerById(playerId);
        player.clearAllTasks();

        player.Data.SetTasks(taskTypeIds);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
class RPCHandlerPatch
{
    static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        byte packetId = callId;
        switch (packetId)
        {

            // Main Controls

            case (byte)CustomRPC.ResetVariables:
                RPCProcedure.resetVariables();
                break;
            case (byte)CustomRPC.ShareOptions:
                RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                break;
            case (byte)CustomRPC.ForceEnd:
                RPCProcedure.forceEnd();
                break;
            case (byte)CustomRPC.WorkaroundSetRoles:
                RPCProcedure.workaroundSetRoles(reader.ReadByte(), reader);
                break;
            case (byte)CustomRPC.SetRole:
                byte roleId = reader.ReadByte();
                byte playerId = reader.ReadByte();
                RPCProcedure.setRole(roleId, playerId);
                break;
            case (byte)CustomRPC.SetModifier:
                byte modifierId = reader.ReadByte();
                byte pId = reader.ReadByte();
                byte flag = reader.ReadByte();
                RPCProcedure.setModifier(modifierId, pId, flag);
                break;
            case (byte)CustomRPC.VersionHandshake:
                byte major = reader.ReadByte();
                byte minor = reader.ReadByte();
                byte patch = reader.ReadByte();
                float timer = reader.ReadSingle();
                if (!AmongUsClient.Instance.AmHost && timer >= 0f) GameStartManagerPatch.timer = timer;
                int versionOwnerId = reader.ReadPackedInt32();
                byte revision = 0xFF;
                Guid guid;
                if (reader.Length - reader.Position >= 17)
                { // enough bytes left to read
                    revision = reader.ReadByte();
                    // GUID
                    byte[] gbytes = reader.ReadBytes(16);
                    guid = new Guid(gbytes);
                }
                else
                {
                    guid = new Guid(new byte[16]);
                }
                RPCProcedure.versionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                break;
            case (byte)CustomRPC.UseUncheckedVent:
                int ventId = reader.ReadPackedInt32();
                byte ventingPlayer = reader.ReadByte();
                byte isEnter = reader.ReadByte();
                RPCProcedure.useUncheckedVent(ventId, ventingPlayer, isEnter);
                break;
            case (byte)CustomRPC.UncheckedMurderPlayer:
                byte source = reader.ReadByte();
                byte target = reader.ReadByte();
                byte showAnimation = reader.ReadByte();
                RPCProcedure.uncheckedMurderPlayer(source, target, showAnimation);
                break;
            case (byte)CustomRPC.UncheckedExilePlayer:
                byte exileTarget = reader.ReadByte();
                RPCProcedure.uncheckedExilePlayer(exileTarget);
                break;
            case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                byte reportSource = reader.ReadByte();
                byte reportTarget = reader.ReadByte();
                RPCProcedure.uncheckedCmdReportDeadBody(reportSource, reportTarget);
                break;
            case (byte)CustomRPC.DynamicMapOption:
                byte mapId = reader.ReadByte();
                RPCProcedure.dynamicMapOption(mapId);
                break;
            case (byte)CustomRPC.SetGameStarting:
                RPCProcedure.setGameStarting();
                break;

            // Role functionality

            case (byte)CustomRPC.EngineerFixLights:
                RPCProcedure.engineerFixLights();
                break;
            case (byte)CustomRPC.EngineerFixSubmergedOxygen:
                RPCProcedure.engineerFixSubmergedOxygen();
                break;
            case (byte)CustomRPC.EngineerUsedRepair:
                RPCProcedure.engineerUsedRepair(reader.ReadByte());
                break;
            case (byte)CustomRPC.CleanBody:
                RPCProcedure.cleanBody(reader.ReadByte(), reader.ReadByte());
                break;
            case (byte)CustomRPC.SheriffKill:
                RPCProcedure.sheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                break;
            case (byte)CustomRPC.TimeMasterRewindTime:
                RPCProcedure.timeMasterRewindTime();
                break;
            case (byte)CustomRPC.TimeMasterShield:
                RPCProcedure.timeMasterShield();
                break;
            case (byte)CustomRPC.MedicSetShielded:
                RPCProcedure.medicSetShielded(reader.ReadByte(), reader.ReadByte());
                break;
            case (byte)CustomRPC.ShieldedMurderAttempt:
                RPCProcedure.shieldedMurderAttempt(reader.ReadByte());
                break;
            case (byte)CustomRPC.ShifterShift:
                RPCProcedure.shifterShift(reader.ReadByte());
                break;
            case (byte)CustomRPC.SwapperSwap:
                byte playerId1 = reader.ReadByte();
                byte playerId2 = reader.ReadByte();
                RPCProcedure.swapperSwap(playerId1, playerId2);
                break;
            case (byte)CustomRPC.MorphingMorph:
                RPCProcedure.morphingMorph(reader.ReadByte());
                break;
            case (byte)CustomRPC.CamouflagerCamouflage:
                RPCProcedure.camouflagerCamouflage();
                break;
            case (byte)CustomRPC.VampireSetBitten:
                byte bittenId = reader.ReadByte();
                byte reset = reader.ReadByte();
                RPCProcedure.vampireSetBitten(bittenId, reset);
                break;
            case (byte)CustomRPC.PlaceGarlic:
                RPCProcedure.placeGarlic(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.TrackerUsedTracker:
                RPCProcedure.trackerUsedTracker(reader.ReadByte());
                break;
            case (byte)CustomRPC.JackalCreatesSidekick:
                RPCProcedure.jackalCreatesSidekick(reader.ReadByte());
                break;
            case (byte)CustomRPC.SidekickPromotes:
                RPCProcedure.sidekickPromotes();
                break;
            case (byte)CustomRPC.ErasePlayerRoles:
                byte eraseTarget = reader.ReadByte();
                RPCProcedure.erasePlayerRoles(eraseTarget);
                Eraser.alreadyErased.Add(eraseTarget);
                break;
            case (byte)CustomRPC.SetFutureErased:
                RPCProcedure.setFutureErased(reader.ReadByte());
                break;
            case (byte)CustomRPC.SetFutureShifted:
                RPCProcedure.setFutureShifted(reader.ReadByte());
                break;
            case (byte)CustomRPC.SetFutureShielded:
                RPCProcedure.setFutureShielded(reader.ReadByte(), reader.ReadByte());
                break;
            case (byte)CustomRPC.PlaceNinjaTrace:
                RPCProcedure.placeNinjaTrace(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.PlacePortal:
                RPCProcedure.placePortal(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.UsePortal:
                RPCProcedure.usePortal(reader.ReadByte(), reader.ReadByte());
                break;
            case (byte)CustomRPC.PlaceJackInTheBox:
                RPCProcedure.placeJackInTheBox(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.LightsOut:
                RPCProcedure.lightsOut();
                break;
            case (byte)CustomRPC.PlaceCamera:
                RPCProcedure.placeCamera(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.SealVent:
                RPCProcedure.sealVent(reader.ReadPackedInt32());
                break;
            case (byte)CustomRPC.ArsonistWin:
                RPCProcedure.arsonistWin();
                break;
            case (byte)CustomRPC.GuesserShoot:
                byte killerId = reader.ReadByte();
                byte dyingTarget = reader.ReadByte();
                byte guessedTarget = reader.ReadByte();
                byte guessedRoleId = reader.ReadByte();
                RPCProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleId);
                break;
            case (byte)CustomRPC.LawyerSetTarget:
                RPCProcedure.lawyerSetTarget(reader.ReadByte());
                break;
            case (byte)CustomRPC.LawyerPromotesToPursuer:
                RPCProcedure.lawyerPromotesToPursuer();
                break;
            case (byte)CustomRPC.SetBlanked:
                var pid = reader.ReadByte();
                var blankedValue = reader.ReadByte();
                RPCProcedure.setBlanked(pid, blankedValue);
                break;
            case (byte)CustomRPC.SetFutureSpelled:
                RPCProcedure.setFutureSpelled(reader.ReadByte());
                break;
            case (byte)CustomRPC.Bloody:
                byte bloodyKiller = reader.ReadByte();
                byte bloodyDead = reader.ReadByte();
                RPCProcedure.bloody(bloodyKiller, bloodyDead);
                break;
            case (byte)CustomRPC.SetFirstKill:
                byte firstKill = reader.ReadByte();
                RPCProcedure.setFirstKill(firstKill);
                break;
            case (byte)CustomRPC.SetTiebreak:
                RPCProcedure.setTiebreak();
                break;
            case (byte)CustomRPC.SetInvisible:
                byte invisiblePlayer = reader.ReadByte();
                byte invisibleFlag = reader.ReadByte();
                RPCProcedure.setInvisible(invisiblePlayer, invisibleFlag);
                break;
            case (byte)CustomRPC.ThiefStealsRole:
                byte thiefTargetId = reader.ReadByte();
                RPCProcedure.thiefStealsRole(thiefTargetId);
                break;
            case (byte)CustomRPC.SetTrap:
                RPCProcedure.setTrap(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.TriggerTrap:
                byte trappedPlayer = reader.ReadByte();
                byte trapId = reader.ReadByte();
                RPCProcedure.triggerTrap(trappedPlayer, trapId);
                break;
            case (byte)CustomRPC.PlaceBomb:
                RPCProcedure.placeBomb(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.DefuseBomb:
                RPCProcedure.defuseBomb();
                break;
            case (byte)CustomRPC.ShareGamemode:
                byte gm = reader.ReadByte();
                RPCProcedure.shareGamemode(gm);
                break;
            case (byte)CustomRPC.StopStart:
                RPCProcedure.stopStart(reader.ReadByte());
                break;
            case (byte)CustomRPC.YoyoMarkLocation:
                RPCProcedure.yoyoMarkLocation(reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.YoyoBlink:
                RPCProcedure.yoyoBlink(reader.ReadByte() == byte.MaxValue, reader.ReadBytesAndSize());
                break;
            case (byte)CustomRPC.BreakArmor:
                RPCProcedure.breakArmor();
                break;
            case (byte)CustomRPC.SetLovers:
                RPCProcedure.setLovers(reader.ReadByte(), reader.ReadByte());
                break;
            case (byte)CustomRPC.SetShifterType:
                RPCProcedure.setShifterType(reader.ReadBoolean());
                break;

            case (byte)CustomRPC.ShareRoom:
                byte roomPlayer = reader.ReadByte();
                byte roomId = reader.ReadByte();
                RPCProcedure.shareRoom(roomPlayer, roomId);
                break;
            case (byte)CustomRPC.EventKick:
                byte kickSource = reader.ReadByte();
                byte kickTarget = reader.ReadByte();
                EventUtility.handleKick(Helpers.playerById(kickSource), Helpers.playerById(kickTarget), reader.ReadSingle());
                break;
        }
    }
}

// RPCの送信を行うクラス
// このクラスはIDisposableを実装しているため、usingステートメントを使って使い捨てることができます
// 例:
// using (var rpc = new RPCSender(0, 0))
// {
//     rpc.Write(0);
//     rpc.Write(1);
// } // Disposeが呼ばれる
// ↑っていうのをCopilotが生成してくれた
public class RPCSender(uint netId, CustomRPC callId, int targetId = -1) : IDisposable
{
    // Send RPC to player with netId
    private readonly MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(netId, (byte)callId, SendOption.Reliable, targetId);

    public void Dispose()
    {
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public void Write(bool value)
    {
        writer.Write(value);
    }

    public void Write(byte value)
    {
        writer.Write(value);
    }

    public void Write(uint value, bool isPacked = false)
    {
        if (isPacked)
        {
            writer.WritePacked(value);
        }
        else
        {
            writer.Write(value);
        }
    }

    public void Write(int value, bool isPacked = false)
    {
        if (isPacked)
        {
            writer.WritePacked(value);
        }
        else
        {
            writer.Write(value);
        }
    }

    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(string value)
    {
        writer.Write(value);
    }

    public void WriteBytesAndSize(Il2CppStructArray<byte> bytes)
    {
        writer.WriteBytesAndSize(bytes);
    }
}