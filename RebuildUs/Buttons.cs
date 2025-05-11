using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using RebuildUs.Objects;
using System.Linq;
using RebuildUs.Patches;

namespace RebuildUs;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
static class HudManagerStartPatch
{
    private static bool initialized = false;

    private static CustomButton shifterShiftButton;
    private static CustomButton morphingButton;
    private static CustomButton camouflagerButton;
    public static CustomButton vampireKillButton;
    public static CustomButton garlicButton;
    private static CustomButton placeJackInTheBoxButton;
    private static CustomButton lightsOutButton;
    public static CustomButton cleanerCleanButton;
    public static CustomButton warlockCurseButton;
    public static CustomButton securityGuardButton;
    public static CustomButton securityGuardCamButton;
    public static CustomButton vultureEatButton;
    public static CustomButton mediumButton;
    public static CustomButton pursuerButton;
    public static CustomButton witchSpellButton;
    public static CustomButton ninjaButton;
    public static CustomButton thiefKillButton;
    public static CustomButton trapperButton;
    public static CustomButton bomberButton;
    public static CustomButton yoyoButton;
    public static CustomButton yoyoAdminTableButton;
    public static CustomButton defuseButton;
    public static CustomButton zoomOutButton;
    public static CustomButton eventKickButton;

    public static PoolablePlayer targetDisplay;

    public static TMPro.TMP_Text securityGuardButtonScrewsText;
    public static TMPro.TMP_Text securityGuardChargesText;
    public static TMPro.TMP_Text pursuerButtonBlanksText;
    public static TMPro.TMP_Text trapperChargesText;

    public static void setCustomButtonCooldowns()
    {
        if (!initialized)
        {
            try
            {
                createButtonsPostfix(HudManager.Instance);
            }
            catch
            {
                RebuildUs.Instance.Logger.LogWarning("Button cooldowns not set, either the gamemode does not require them or there's something wrong.");
                return;
            }
        }
        shifterShiftButton.MaxTimer = 0f;
        morphingButton.MaxTimer = Morphing.cooldown;
        camouflagerButton.MaxTimer = Camouflager.cooldown;
        vampireKillButton.MaxTimer = Vampire.cooldown;
        garlicButton.MaxTimer = 0f;
        placeJackInTheBoxButton.MaxTimer = Trickster.placeBoxCooldown;
        lightsOutButton.MaxTimer = Trickster.lightsOutCooldown;
        cleanerCleanButton.MaxTimer = Cleaner.cooldown;
        warlockCurseButton.MaxTimer = Warlock.cooldown;
        securityGuardButton.MaxTimer = SecurityGuard.cooldown;
        securityGuardCamButton.MaxTimer = SecurityGuard.cooldown;
        vultureEatButton.MaxTimer = Vulture.cooldown;
        mediumButton.MaxTimer = Medium.cooldown;
        pursuerButton.MaxTimer = Pursuer.cooldown;
        witchSpellButton.MaxTimer = Witch.cooldown;
        ninjaButton.MaxTimer = Ninja.cooldown;
        thiefKillButton.MaxTimer = Thief.cooldown;
        trapperButton.MaxTimer = Trapper.cooldown;
        bomberButton.MaxTimer = Bomber.bombCooldown;
        yoyoButton.MaxTimer = Yoyo.markCooldown;
        yoyoAdminTableButton.MaxTimer = Yoyo.adminCooldown;
        yoyoAdminTableButton.effectDuration = 10f;
        defuseButton.MaxTimer = 0f;
        defuseButton.Timer = 0f;

        vampireKillButton.effectDuration = Vampire.delay;
        camouflagerButton.effectDuration = Camouflager.duration;
        morphingButton.effectDuration = Morphing.duration;
        lightsOutButton.effectDuration = Trickster.lightsOutDuration;
        mediumButton.effectDuration = Medium.duration;
        witchSpellButton.effectDuration = Witch.spellCastingDuration;
        securityGuardCamButton.effectDuration = SecurityGuard.duration;
        defuseButton.effectDuration = Bomber.defuseDuration;
        bomberButton.effectDuration = Bomber.destructionTime + Bomber.bombActiveAfter;
        // Already set the timer to the max, as the button is enabled during the game and not available at the start
        lightsOutButton.Timer = lightsOutButton.MaxTimer;
        zoomOutButton.MaxTimer = 0f;
    }

    private static void setButtonTargetDisplay(PlayerControl target, CustomButton button = null, Vector3? offset = null)
    {
        if (target == null || button == null)
        {
            if (targetDisplay != null)
            {  // Reset the poolable player
                targetDisplay.gameObject.SetActive(false);
                GameObject.Destroy(targetDisplay.gameObject);
                targetDisplay = null;
            }
            return;
        }
        // Add poolable player to the button so that the target outfit is shown
        button.actionButton.cooldownTimerText.transform.localPosition = new Vector3(0, 0, -1f);  // Before the poolable player
        targetDisplay = UnityEngine.Object.Instantiate<PoolablePlayer>(Patches.IntroCutsceneOnDestroyPatch.playerPrefab, button.actionButton.transform);
        NetworkedPlayerInfo data = target.Data;
        target.SetPlayerMaterialColors(targetDisplay.cosmetics.currentBodySprite.BodySprite);
        targetDisplay.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
        targetDisplay.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
        targetDisplay.cosmetics.nameText.text = "";  // Hide the name!
        targetDisplay.transform.localPosition = new Vector3(0f, 0.22f, -0.01f);
        if (offset != null) targetDisplay.transform.localPosition += (Vector3)offset;
        targetDisplay.transform.localScale = Vector3.one * 0.33f;
        targetDisplay.setSemiTransparent(false);
        targetDisplay.gameObject.SetActive(true);
    }

    public static void Postfix(HudManager __instance)
    {
        initialized = false;

        try
        {
            createButtonsPostfix(__instance);
        }
        catch { }
    }

    public static void createButtonsPostfix(HudManager __instance)
    {
        // get map id, or raise error to wait...
        var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;

        // Shifter shift
        shifterShiftButton = new CustomButton(
            () =>
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.SetFutureShielded);
                writer.Write(Shifter.currentTarget.PlayerId);
                RPCProcedure.setFutureShifted(Shifter.currentTarget.PlayerId);
            },
            () => { return Shifter.shifter != null && Shifter.shifter == PlayerControl.LocalPlayer && Shifter.futureShift == null && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return Shifter.currentTarget && Shifter.futureShift == null && PlayerControl.LocalPlayer.CanMove; },
            () => { },
            Shifter.getButtonSprite(),
            ButtonOffset.UpperRight,
            __instance,
            __instance.UseButton,
            null,
            true
        );

        // Morphing morph
        morphingButton = new CustomButton(
            () =>
            {
                if (Morphing.sampledTarget != null)
                {
                    using var writer = RPCProcedure.SendRPC(CustomRPC.MorphingMorph);
                    writer.Write(Morphing.sampledTarget.PlayerId);
                    RPCProcedure.morphingMorph(Morphing.sampledTarget.PlayerId);

                    Morphing.sampledTarget = null;
                    morphingButton.effectDuration = Morphing.duration;
                }
                else if (Morphing.currentTarget != null)
                {
                    Morphing.sampledTarget = Morphing.currentTarget;
                    morphingButton.sprite = Morphing.getMorphSprite();
                    morphingButton.effectDuration = 1f;

                    // Add poolable player to the button so that the target outfit is shown
                    setButtonTargetDisplay(Morphing.sampledTarget, morphingButton);
                }
            },
            () => { return Morphing.morphing != null && Morphing.morphing == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return (Morphing.currentTarget || Morphing.sampledTarget) && PlayerControl.LocalPlayer.CanMove && !Helpers.MushroomSabotageActive(); },
            () =>
            {
                morphingButton.Timer = morphingButton.MaxTimer;
                morphingButton.sprite = Morphing.getSampleSprite();
                morphingButton.isEffectActive = false;
                morphingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                Morphing.sampledTarget = null;
                setButtonTargetDisplay(null);
            },
            Morphing.getSampleSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            true,
            Morphing.duration,
            () =>
            {
                if (Morphing.sampledTarget == null)
                {
                    morphingButton.Timer = morphingButton.MaxTimer;
                    morphingButton.sprite = Morphing.getSampleSprite();

                    // Reset the poolable player
                    setButtonTargetDisplay(null);
                }
            }
        );

        // Camouflager camouflage
        camouflagerButton = new CustomButton(
            () =>
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.CamouflagerCamouflage);
                RPCProcedure.camouflagerCamouflage();
            },
            () => { return Camouflager.camouflager != null && Camouflager.camouflager == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                camouflagerButton.Timer = camouflagerButton.MaxTimer;
                camouflagerButton.isEffectActive = false;
                camouflagerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            Camouflager.getButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            true,
            Camouflager.duration,
            () =>
            {
                camouflagerButton.Timer = camouflagerButton.MaxTimer;
            }
        );

        vampireKillButton = new CustomButton(
            () =>
            {
                MurderAttemptResult murder = Helpers.checkMurderAttempt(Vampire.vampire, Vampire.currentTarget);
                if (murder == MurderAttemptResult.PerformKill)
                {
                    if (Vampire.targetNearGarlic)
                    {
                        using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedMurderPlayer);
                        writer.Write(Vampire.vampire.PlayerId);
                        writer.Write(Vampire.currentTarget.PlayerId);
                        writer.Write(Byte.MaxValue);
                        RPCProcedure.uncheckedMurderPlayer(Vampire.vampire.PlayerId, Vampire.currentTarget.PlayerId, Byte.MaxValue);

                        vampireKillButton.hasEffect = false; // Block effect on this click
                        vampireKillButton.Timer = vampireKillButton.MaxTimer;
                    }
                    else
                    {
                        Vampire.bitten = Vampire.currentTarget;
                        // Notify players about bitten
                        using var writer = RPCProcedure.SendRPC(CustomRPC.VampireSetBitten);
                        writer.Write(Vampire.bitten.PlayerId);
                        writer.Write((byte)0);
                        RPCProcedure.vampireSetBitten(Vampire.bitten.PlayerId, 0);

                        byte lastTimer = (byte)Vampire.delay;
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Vampire.delay, new Action<float>((p) =>
                        { // Delayed action
                            if (p <= 1f)
                            {
                                byte timer = (byte)vampireKillButton.Timer;
                                if (timer != lastTimer)
                                {
                                    lastTimer = timer;
                                    using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo); writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                    writer.Write((byte)GhostInfoTypes.VampireTimer);
                                    writer.Write(timer);
                                }
                            }
                            if (p == 1f)
                            {
                                // Perform kill if possible and reset bitten (regardless whether the kill was successful or not)
                                var res = Helpers.checkMurderAttemptAndKill(Vampire.vampire, Vampire.bitten, showAnimation: false);
                                if (res == MurderAttemptResult.PerformKill)
                                {
                                    using var writer = RPCProcedure.SendRPC(CustomRPC.VampireSetBitten);
                                    writer.Write(byte.MaxValue);
                                    writer.Write(byte.MaxValue);
                                    RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
                                }
                            }
                        })));

                        vampireKillButton.hasEffect = true; // Trigger effect on this click
                    }
                }
                else if (murder == MurderAttemptResult.BlankKill)
                {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                    vampireKillButton.hasEffect = false;
                }
                else
                {
                    vampireKillButton.hasEffect = false;
                }
            },
            () => { return Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                if (Vampire.targetNearGarlic && Vampire.canKillNearGarlics)
                {
                    vampireKillButton.actionButton.graphic.sprite = __instance.KillButton.graphic.sprite;
                    vampireKillButton.showButtonText = true;
                }
                else
                {
                    vampireKillButton.actionButton.graphic.sprite = Vampire.getButtonSprite();
                    vampireKillButton.showButtonText = false;
                }
                return Vampire.currentTarget != null && PlayerControl.LocalPlayer.CanMove && (!Vampire.targetNearGarlic || Vampire.canKillNearGarlics);
            },
            () =>
            {
                vampireKillButton.Timer = vampireKillButton.MaxTimer;
                vampireKillButton.isEffectActive = false;
                vampireKillButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            Vampire.getButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            false,
            0f,
            () =>
            {
                vampireKillButton.Timer = vampireKillButton.MaxTimer;
            }
        );

        garlicButton = new CustomButton(
            () =>
            {
                Vampire.localPlacedGarlic = true;
                var pos = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceGarlic, Hazel.SendOption.Reliable);
                writer.WriteBytesAndSize(buff);
                writer.EndMessage();
                RPCProcedure.placeGarlic(buff);
            },
            () => { return !Vampire.localPlacedGarlic && !PlayerControl.LocalPlayer.Data.IsDead && Vampire.garlicsActive; },
            () => { return PlayerControl.LocalPlayer.CanMove && !Vampire.localPlacedGarlic; },
            () => { },
            Vampire.getGarlicButtonSprite(),
            new Vector3(0, -0.06f, 0),
            __instance,
            __instance.KillButton,
            null,
            true
        );

        placeJackInTheBoxButton = new CustomButton(
            () =>
            {
                placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer;

                var pos = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceJackInTheBox, Hazel.SendOption.Reliable);
                writer.WriteBytesAndSize(buff);
                writer.EndMessage();
                RPCProcedure.placeJackInTheBox(buff);
            },
            () => { return Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && !JackInTheBox.hasJackInTheBoxLimitReached(); },
            () => { return PlayerControl.LocalPlayer.CanMove && !JackInTheBox.hasJackInTheBoxLimitReached(); },
            () => { placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer; },
            Trickster.getPlaceBoxButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F
        );

        lightsOutButton = new CustomButton(
            () =>
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.LightsOut);
                RPCProcedure.lightsOut();
            },
            () =>
            {
                return Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && JackInTheBox.hasJackInTheBoxLimitReached() && JackInTheBox.boxesConvertedToVents;
            },
            () => { return PlayerControl.LocalPlayer.CanMove && JackInTheBox.hasJackInTheBoxLimitReached() && JackInTheBox.boxesConvertedToVents; },
            () =>
            {
                lightsOutButton.Timer = lightsOutButton.MaxTimer;
                lightsOutButton.isEffectActive = false;
                lightsOutButton.actionButton.graphic.color = Palette.EnabledColor;
            },
            Trickster.getLightsOutButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.SabotageButton,
            KeyCode.F,
            true,
            Trickster.lightsOutDuration,
            () =>
            {
                lightsOutButton.Timer = lightsOutButton.MaxTimer;
            }
        );

        // Cleaner Clean
        cleanerCleanButton = new CustomButton(
            () =>
            {
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                {
                    if (collider2D.tag == "DeadBody")
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (component && !component.Reported)
                        {
                            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                            Vector2 truePosition2 = component.TruePosition;
                            if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                            {
                                NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                using var writer = RPCProcedure.SendRPC(CustomRPC.CleanBody);
                                writer.Write(playerInfo.PlayerId);
                                writer.Write(Cleaner.cleaner.PlayerId);
                                RPCProcedure.cleanBody(playerInfo.PlayerId, Cleaner.cleaner.PlayerId);

                                Cleaner.cleaner.killTimer = cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer;
                                break;
                            }
                        }
                    }
                }
            },
            () => { return Cleaner.cleaner != null && Cleaner.cleaner == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
            () => { cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer; },
            Cleaner.getButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F
        );

        // Warlock curse
        warlockCurseButton = new CustomButton(
            () =>
            {
                if (Warlock.curseVictim == null)
                {
                    // Apply Curse
                    Warlock.curseVictim = Warlock.currentTarget;
                    warlockCurseButton.sprite = Warlock.getCurseKillButtonSprite();
                    warlockCurseButton.Timer = 1f;

                    // Ghost Info
                    using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write((byte)GhostInfoTypes.WarlockTarget);
                    writer.Write(Warlock.curseVictim.PlayerId);
                }
                else if (Warlock.curseVictim != null && Warlock.curseVictimTarget != null)
                {
                    MurderAttemptResult murder = Helpers.checkMurderAttemptAndKill(Warlock.warlock, Warlock.curseVictimTarget, showAnimation: false);
                    if (murder == MurderAttemptResult.SuppressKill) return;

                    // If blanked or killed
                    if (Warlock.rootTime > 0)
                    {
                        AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
                        PlayerControl.LocalPlayer.moveable = false;
                        PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement so the warlock is not just running straight into the next object
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Warlock.rootTime, new Action<float>((p) =>
                        { // Delayed action
                            if (p == 1f)
                            {
                                PlayerControl.LocalPlayer.moveable = true;
                            }
                        })));
                    }

                    Warlock.curseVictim = null;
                    Warlock.curseVictimTarget = null;
                    warlockCurseButton.sprite = Warlock.getCurseButtonSprite();
                    Warlock.warlock.killTimer = warlockCurseButton.Timer = warlockCurseButton.MaxTimer;

                    using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write((byte)GhostInfoTypes.WarlockTarget);
                    writer.Write(Byte.MaxValue); // This will set it to null!
                }
            },
            () => { return Warlock.warlock != null && Warlock.warlock == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return ((Warlock.curseVictim == null && Warlock.currentTarget != null) || (Warlock.curseVictim != null && Warlock.curseVictimTarget != null)) && PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                warlockCurseButton.Timer = warlockCurseButton.MaxTimer;
                warlockCurseButton.sprite = Warlock.getCurseButtonSprite();
                Warlock.curseVictim = null;
                Warlock.curseVictimTarget = null;
            },
            Warlock.getCurseButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F
        );

        // Security Guard button
        securityGuardButton = new CustomButton(
            () =>
            {
                if (SecurityGuard.ventTarget != null)
                { // Seal vent
                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SealVent, Hazel.SendOption.Reliable);
                    writer.WritePacked(SecurityGuard.ventTarget.Id);
                    writer.EndMessage();
                    RPCProcedure.sealVent(SecurityGuard.ventTarget.Id);
                    SecurityGuard.ventTarget = null;

                }
                else if (!Helpers.isMira() && !Helpers.isFungle() && !SubmergedCompatibility.IsSubmerged)
                { // Place camera if there's no vent and it's not MiraHQ or Submerged
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceCamera, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.placeCamera(buff);
                }
                securityGuardButton.Timer = securityGuardButton.MaxTimer;
            },
            () => { return SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews >= Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice); },
            () =>
            {
                securityGuardButton.actionButton.graphic.sprite = (SecurityGuard.ventTarget == null && !Helpers.isMira() && !Helpers.isFungle() && !SubmergedCompatibility.IsSubmerged) ? SecurityGuard.getPlaceCameraButtonSprite() : SecurityGuard.getCloseVentButtonSprite();
                if (securityGuardButtonScrewsText != null) securityGuardButtonScrewsText.text = $"{SecurityGuard.remainingScrews}/{SecurityGuard.totalScrews}";

                if (SecurityGuard.ventTarget != null)
                    return SecurityGuard.remainingScrews >= SecurityGuard.ventPrice && PlayerControl.LocalPlayer.CanMove;
                return !Helpers.isMira() && !Helpers.isFungle() && !SubmergedCompatibility.IsSubmerged && SecurityGuard.remainingScrews >= SecurityGuard.camPrice && PlayerControl.LocalPlayer.CanMove;
            },
            () => { securityGuardButton.Timer = securityGuardButton.MaxTimer; },
            SecurityGuard.getPlaceCameraButtonSprite(),
            ButtonOffset.LowerRight,
            __instance,
            __instance.UseButton,
            KeyCode.F
        );

        // Security Guard button screws counter
        securityGuardButtonScrewsText = GameObject.Instantiate(securityGuardButton.actionButton.cooldownTimerText, securityGuardButton.actionButton.cooldownTimerText.transform.parent);
        securityGuardButtonScrewsText.text = "";
        securityGuardButtonScrewsText.enableWordWrapping = false;
        securityGuardButtonScrewsText.transform.localScale = Vector3.one * 0.5f;
        securityGuardButtonScrewsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        securityGuardCamButton = new CustomButton(
            () =>
            {
                if (!Helpers.isMira())
                {
                    if (SecurityGuard.minigame == null)
                    {
                        byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                        var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("Surv_Panel") || x.name.Contains("Cam") || x.name.Contains("BinocularsSecurityConsole"));
                        if (Helpers.isSkeld() || mapId == 3) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvConsole"));
                        else if (Helpers.isAirship()) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("task_cams"));
                        if (e == null || Camera.main == null) return;
                        SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                    }
                    SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                    SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                    SecurityGuard.minigame.Begin(null);
                }
                else
                {
                    if (SecurityGuard.minigame == null)
                    {
                        var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                        if (e == null || Camera.main == null) return;
                        SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                    }
                    SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                    SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                    SecurityGuard.minigame.Begin(null);
                }
                SecurityGuard.charges--;

                if (SecurityGuard.cantMove) PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement
            },
            () =>
            {
                return SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews < Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice) && !SubmergedCompatibility.IsSubmerged;
            },
            () =>
            {
                if (securityGuardChargesText != null) securityGuardChargesText.text = $"{SecurityGuard.charges} / {SecurityGuard.maxCharges}";
                securityGuardCamButton.actionButton.graphic.sprite = Helpers.isMira() ? SecurityGuard.getLogSprite() : SecurityGuard.getCamSprite();
                securityGuardCamButton.actionButton.OverrideText(Helpers.isMira() ? "DOORLOG" : "SECURITY");
                return PlayerControl.LocalPlayer.CanMove && SecurityGuard.charges > 0;
            },
            () =>
            {
                securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                securityGuardCamButton.isEffectActive = false;
                securityGuardCamButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            SecurityGuard.getCamSprite(),
            ButtonOffset.LowerRight,
            __instance,
            __instance.UseButton,
            KeyCode.G,
            true,
            0f,
            () =>
            {
                securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                if (Minigame.Instance)
                {
                    SecurityGuard.minigame.ForceClose();
                }
                PlayerControl.LocalPlayer.moveable = true;
            },
            false,
            Helpers.isMira() ? "DOORLOG" : "SECURITY"
        );

        // Security Guard cam button charges
        securityGuardChargesText = GameObject.Instantiate(securityGuardCamButton.actionButton.cooldownTimerText, securityGuardCamButton.actionButton.cooldownTimerText.transform.parent);
        securityGuardChargesText.text = "";
        securityGuardChargesText.enableWordWrapping = false;
        securityGuardChargesText.transform.localScale = Vector3.one * 0.5f;
        securityGuardChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        // Vulture Eat
        vultureEatButton = new CustomButton(
            () =>
            {
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                {
                    if (collider2D.tag == "DeadBody")
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (component && !component.Reported)
                        {
                            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                            Vector2 truePosition2 = component.TruePosition;
                            if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                            {
                                NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                using var writer = RPCProcedure.SendRPC(CustomRPC.CleanBody);
                                writer.Write(playerInfo.PlayerId);
                                writer.Write(Vulture.vulture.PlayerId);
                                RPCProcedure.cleanBody(playerInfo.PlayerId, Vulture.vulture.PlayerId);

                                Vulture.cooldown = vultureEatButton.Timer = vultureEatButton.MaxTimer;
                                break;
                            }
                        }
                    }
                }
            },
            () => { return Vulture.vulture != null && Vulture.vulture == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
            () => { vultureEatButton.Timer = vultureEatButton.MaxTimer; },
            Vulture.getButtonSprite(),
            ButtonOffset.LowerCenter,
            __instance,
            __instance.KillButton,
            KeyCode.F
        );

        // Medium button
        mediumButton = new CustomButton(
            () =>
            {
                if (Medium.target != null)
                {
                    Medium.soulTarget = Medium.target;
                    mediumButton.hasEffect = true;
                }
            },
            () => { return Medium.medium != null && Medium.medium == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                if (mediumButton.isEffectActive && Medium.target != Medium.soulTarget)
                {
                    Medium.soulTarget = null;
                    mediumButton.Timer = 0f;
                    mediumButton.isEffectActive = false;
                }
                return Medium.target != null && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                mediumButton.Timer = mediumButton.MaxTimer;
                mediumButton.isEffectActive = false;
                Medium.soulTarget = null;
            },
            Medium.getQuestionSprite(),
            ButtonOffset.LowerRight,
            __instance,
            __instance.UseButton,
            KeyCode.F,
            true,
            Medium.duration,
            () =>
            {
                mediumButton.Timer = mediumButton.MaxTimer;
                if (Medium.target == null || Medium.target.player == null) return;
                string msg = Medium.getInfo(Medium.target.player, Medium.target.killerIfExisting, Medium.target.deathReason);
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);

                // Ghost Info
                using var writer = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                writer.Write(Medium.target.player.PlayerId);
                writer.Write((byte)GhostInfoTypes.MediumInfo);
                writer.Write(msg);

                // Remove soul
                if (Medium.oneTimeUse)
                {
                    float closestDistance = float.MaxValue;
                    SpriteRenderer target = null;

                    foreach ((DeadPlayer db, Vector3 ps) in Medium.deadBodies)
                    {
                        if (db == Medium.target)
                        {
                            Tuple<DeadPlayer, Vector3> deadBody = Tuple.Create(db, ps);
                            Medium.deadBodies.Remove(deadBody);
                            break;
                        }

                    }
                    foreach (SpriteRenderer rend in Medium.souls)
                    {
                        float distance = Vector2.Distance(rend.transform.position, PlayerControl.LocalPlayer.GetTruePosition());
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            target = rend;
                        }
                    }

                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5f, new Action<float>((p) =>
                    {
                        if (target != null)
                        {
                            var tmp = target.color;
                            tmp.a = Mathf.Clamp01(1 - p);
                            target.color = tmp;
                        }
                        if (p == 1f && target != null && target.gameObject != null) UnityEngine.Object.Destroy(target.gameObject);
                    })));

                    Medium.souls.Remove(target);
                }
            }
        );

        // Pursuer button
        pursuerButton = new CustomButton(
            () =>
            {
                if (Pursuer.target != null)
                {
                    using var writer = RPCProcedure.SendRPC(CustomRPC.SetBlanked);
                    writer.Write(Pursuer.target.PlayerId);
                    writer.Write(Byte.MaxValue);
                    RPCProcedure.setBlanked(Pursuer.target.PlayerId, Byte.MaxValue);

                    Pursuer.target = null;

                    Pursuer.blanks++;
                    pursuerButton.Timer = pursuerButton.MaxTimer;
                }

            },
            () => { return Pursuer.pursuer != null && Pursuer.pursuer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && Pursuer.blanks < Pursuer.blanksNumber; },
            () =>
            {
                if (pursuerButtonBlanksText != null) pursuerButtonBlanksText.text = $"{Pursuer.blanksNumber - Pursuer.blanks}";

                return Pursuer.blanksNumber > Pursuer.blanks && PlayerControl.LocalPlayer.CanMove && Pursuer.target != null;
            },
            () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
            Pursuer.getTargetSprite(),
            ButtonOffset.LowerRight,
            __instance,
            __instance.UseButton,
            KeyCode.F
        );

        // Pursuer button blanks left
        pursuerButtonBlanksText = GameObject.Instantiate(pursuerButton.actionButton.cooldownTimerText, pursuerButton.actionButton.cooldownTimerText.transform.parent);
        pursuerButtonBlanksText.text = "";
        pursuerButtonBlanksText.enableWordWrapping = false;
        pursuerButtonBlanksText.transform.localScale = Vector3.one * 0.5f;
        pursuerButtonBlanksText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        // Witch Spell button
        witchSpellButton = new CustomButton(
            () =>
            {
                if (Witch.currentTarget != null)
                {
                    Witch.spellCastingTarget = Witch.currentTarget;
                }
            },
            () => { return Witch.witch != null && Witch.witch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                if (witchSpellButton.isEffectActive && Witch.spellCastingTarget != Witch.currentTarget)
                {
                    Witch.spellCastingTarget = null;
                    witchSpellButton.Timer = 0f;
                    witchSpellButton.isEffectActive = false;
                }
                return PlayerControl.LocalPlayer.CanMove && Witch.currentTarget != null;
            },
            () =>
            {
                witchSpellButton.Timer = witchSpellButton.MaxTimer;
                witchSpellButton.isEffectActive = false;
                Witch.spellCastingTarget = null;
            },
            Witch.getButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            true,
            Witch.spellCastingDuration,
            () =>
            {
                if (Witch.spellCastingTarget == null) return;
                MurderAttemptResult attempt = Helpers.checkMurderAttempt(Witch.witch, Witch.spellCastingTarget);
                if (attempt == MurderAttemptResult.PerformKill)
                {
                    using var writer = RPCProcedure.SendRPC(CustomRPC.SetFutureSpelled);
                    writer.Write(Witch.currentTarget.PlayerId);
                    RPCProcedure.setFutureSpelled(Witch.currentTarget.PlayerId);
                }
                if (attempt == MurderAttemptResult.BlankKill || attempt == MurderAttemptResult.PerformKill)
                {
                    Witch.currentCooldownAddition += Witch.cooldownAddition;
                    witchSpellButton.MaxTimer = Witch.cooldown + Witch.currentCooldownAddition;
                    Patches.PlayerControlFixedUpdatePatch.miniCooldownUpdate();  // Modifies the MaxTimer if the witch is the mini
                    witchSpellButton.Timer = witchSpellButton.MaxTimer;
                    if (Witch.triggerBothCooldowns)
                    {
                        float multiplier = (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) ? (Mini.isGrownUp() ? 0.66f : 2f) : 1f;
                        Witch.witch.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier;
                    }
                }
                else
                {
                    witchSpellButton.Timer = 0f;
                }
                Witch.spellCastingTarget = null;
            }
        );

        // Ninja mark and assassinate button
        ninjaButton = new CustomButton(
            () =>
            {
                MessageWriter writer;
                if (Ninja.ninjaMarked != null)
                {
                    // Murder attempt with teleport
                    MurderAttemptResult attempt = Helpers.checkMurderAttempt(Ninja.ninja, Ninja.ninjaMarked);
                    if (attempt == MurderAttemptResult.PerformKill)
                    {
                        // Create first trace before killing
                        var pos = PlayerControl.LocalPlayer.transform.position;
                        byte[] buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                        writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceNinjaTrace, Hazel.SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.EndMessage();
                        RPCProcedure.placeNinjaTrace(buff);

                        using var invisibleWriter = RPCProcedure.SendRPC(CustomRPC.SetInvisible);
                        invisibleWriter.Write(Ninja.ninja.PlayerId);
                        invisibleWriter.Write(byte.MinValue);
                        RPCProcedure.setInvisible(Ninja.ninja.PlayerId, byte.MinValue);

                        // Perform Kill
                        using var writer2 = RPCProcedure.SendRPC(CustomRPC.UncheckedMurderPlayer);
                        writer2.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer2.Write(Ninja.ninjaMarked.PlayerId);
                        writer2.Write(byte.MaxValue);
                        if (SubmergedCompatibility.IsSubmerged)
                        {
                            SubmergedCompatibility.ChangeFloor(Ninja.ninjaMarked.transform.localPosition.y > -7);
                        }
                        RPCProcedure.uncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, Ninja.ninjaMarked.PlayerId, byte.MaxValue);

                        // Create Second trace after killing
                        pos = Ninja.ninjaMarked.transform.position;
                        buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                        MessageWriter writer3 = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceNinjaTrace, Hazel.SendOption.Reliable);
                        writer3.WriteBytesAndSize(buff);
                        writer3.EndMessage();
                        RPCProcedure.placeNinjaTrace(buff);
                    }

                    if (attempt == MurderAttemptResult.BlankKill || attempt == MurderAttemptResult.PerformKill)
                    {
                        ninjaButton.Timer = ninjaButton.MaxTimer;
                        Ninja.ninja.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    }
                    else if (attempt == MurderAttemptResult.SuppressKill)
                    {
                        ninjaButton.Timer = 0f;
                    }
                    Ninja.ninjaMarked = null;
                    return;
                }
                if (Ninja.currentTarget != null)
                {
                    Ninja.ninjaMarked = Ninja.currentTarget;
                    ninjaButton.Timer = 5f;

                    // Ghost Info
                    using var writer4 = RPCProcedure.SendRPC(CustomRPC.ShareGhostInfo);
                    writer4.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer4.Write((byte)GhostInfoTypes.NinjaMarked);
                    writer4.Write(Ninja.ninjaMarked.PlayerId);
                }
            },
            () => { return Ninja.ninja != null && Ninja.ninja == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {  // CouldUse
                ninjaButton.sprite = Ninja.ninjaMarked != null ? Ninja.getKillButtonSprite() : Ninja.getMarkButtonSprite();
                return (Ninja.currentTarget != null || Ninja.ninjaMarked != null && !TransportationToolPatches.isUsingTransportation(Ninja.ninjaMarked)) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {  // on meeting ends
                ninjaButton.Timer = ninjaButton.MaxTimer;
                Ninja.ninjaMarked = null;
            },
            Ninja.getMarkButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F
        );

        // Trapper button
        trapperButton = new CustomButton(
            () =>
            {
                var pos = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetTrap, Hazel.SendOption.Reliable);
                writer.WriteBytesAndSize(buff);
                writer.EndMessage();
                RPCProcedure.setTrap(buff);

                trapperButton.Timer = trapperButton.MaxTimer;
            },
            () => { return Trapper.trapper != null && Trapper.trapper == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                if (trapperChargesText != null) trapperChargesText.text = $"{Trapper.charges} / {Trapper.maxCharges}";
                return PlayerControl.LocalPlayer.CanMove && Trapper.charges > 0;
            },
            () => { trapperButton.Timer = trapperButton.MaxTimer; },
            Trapper.getButtonSprite(),
            ButtonOffset.LowerRight,
            __instance,
            __instance.UseButton,
            KeyCode.F
        );

        // Bomber button
        bomberButton = new CustomButton(
            () =>
            {
                if (Helpers.checkMurderAttempt(Bomber.bomber, Bomber.bomber, ignoreMedic: true) != MurderAttemptResult.BlankKill)
                {
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceBomb, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.placeBomb(buff);
                }

                bomberButton.Timer = bomberButton.MaxTimer;
                Bomber.isPlanted = true;
            },
            () => { return Bomber.bomber != null && Bomber.bomber == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return PlayerControl.LocalPlayer.CanMove && !Bomber.isPlanted; },
            () => { bomberButton.Timer = bomberButton.MaxTimer; },
            Bomber.getButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            true,
            Bomber.destructionTime,
            () =>
            {
                bomberButton.Timer = bomberButton.MaxTimer;
                bomberButton.isEffectActive = false;
                bomberButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            }
        );

        defuseButton = new CustomButton(
            () =>
            {
                defuseButton.hasEffect = true;
            },
            () =>
            {
                if (shifterShiftButton.hasButton())
                    defuseButton.positionOffset = new Vector3(0f, 2f, 0f);
                else
                    defuseButton.positionOffset = new Vector3(0f, 1f, 0f);
                return Bomber.bomb != null && Bomb.canDefuse && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () =>
            {
                if (defuseButton.isEffectActive && !Bomb.canDefuse)
                {
                    defuseButton.Timer = 0f;
                    defuseButton.isEffectActive = false;
                }
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                defuseButton.Timer = 0f;
                defuseButton.isEffectActive = false;
            },
            Bomb.getDefuseSprite(),
            ButtonOffset.UpperRight,
            __instance,
            __instance.KillButton,
            null,
            true,
            Bomber.defuseDuration,
            () =>
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.DefuseBomb);
                RPCProcedure.defuseBomb();

                defuseButton.Timer = 0f;
                Bomb.canDefuse = false;
            },
            true
        );

        thiefKillButton = new CustomButton(
            () =>
            {
                PlayerControl thief = Thief.thief;
                PlayerControl target = Thief.currentTarget;
                var result = Helpers.checkMurderAttempt(thief, target);
                if (result == MurderAttemptResult.BlankKill)
                {
                    thiefKillButton.Timer = thiefKillButton.MaxTimer;
                    return;
                }

                if (Thief.suicideFlag)
                {
                    // Suicide
                    using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedMurderPlayer);
                    writer.Write(thief.PlayerId);
                    writer.Write(thief.PlayerId);
                    writer.Write(0);
                    RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, thief.PlayerId, 0);
                    Thief.thief.clearAllTasks();
                }

                // Steal role if survived.
                if (!Thief.thief.Data.IsDead && result == MurderAttemptResult.PerformKill)
                {
                    using var writer = RPCProcedure.SendRPC(CustomRPC.ThiefStealsRole);
                    writer.Write(target.PlayerId);
                    RPCProcedure.thiefStealsRole(target.PlayerId);
                }
                // Kill the victim (after becoming their role - so that no win is triggered for other teams)
                if (result == MurderAttemptResult.PerformKill)
                {
                    using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedMurderPlayer);
                    writer.Write(thief.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(byte.MaxValue);
                    RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, target.PlayerId, byte.MaxValue);
                }
            },
            () => { return Thief.thief != null && PlayerControl.LocalPlayer == Thief.thief && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return Thief.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
            () => { thiefKillButton.Timer = thiefKillButton.MaxTimer; },
            __instance.KillButton.graphic.sprite,
            ButtonOffset.UpperRight,
            __instance,
            __instance.KillButton,
            KeyCode.Q
        );

        // Trapper Charges
        trapperChargesText = GameObject.Instantiate(trapperButton.actionButton.cooldownTimerText, trapperButton.actionButton.cooldownTimerText.transform.parent);
        trapperChargesText.text = "";
        trapperChargesText.enableWordWrapping = false;
        trapperChargesText.transform.localScale = Vector3.one * 0.5f;
        trapperChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        // Yoyo button
        yoyoButton = new CustomButton(
            () =>
            {
                var pos = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                if (Yoyo.markedLocation == null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.YoyoMarkLocation, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.yoyoMarkLocation(buff);
                    yoyoButton.sprite = Yoyo.getBlinkButtonSprite();
                    yoyoButton.Timer = 10f;
                    yoyoButton.hasEffect = false;
                    yoyoButton.buttonText = "Blink";
                }
                else
                {
                    // Jump to location
                    var exit = (Vector3)Yoyo.markedLocation;
                    if (SubmergedCompatibility.IsSubmerged)
                    {
                        SubmergedCompatibility.ChangeFloor(exit.y > -7);
                    }
                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.YoyoBlink, Hazel.SendOption.Reliable);
                    writer.Write(Byte.MaxValue);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.yoyoBlink(true, buff);
                    yoyoButton.effectDuration = Yoyo.blinkDuration;
                    yoyoButton.Timer = 10f;
                    yoyoButton.hasEffect = true;
                    yoyoButton.buttonText = "Returning...";
                }
            },
            () => { return Yoyo.yoyo != null && Yoyo.yoyo == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                if (Yoyo.markStaysOverMeeting)
                {
                    yoyoButton.Timer = 10f;
                }
                else
                {
                    Yoyo.markedLocation = null;
                    yoyoButton.Timer = yoyoButton.MaxTimer;
                    yoyoButton.sprite = Yoyo.getMarkButtonSprite();
                    yoyoButton.buttonText = "Mark Location";
                }
            },
            Yoyo.getMarkButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            false,
            Yoyo.blinkDuration,
            () =>
            {
                if (TransportationToolPatches.isUsingTransportation(Yoyo.yoyo))
                {
                    yoyoButton.Timer = 0.5f;
                    yoyoButton.DeputyTimer = 0.5f;
                    yoyoButton.isEffectActive = true;
                    yoyoButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    return;
                }
                else if (Yoyo.yoyo.inVent)
                {
                    __instance.ImpostorVentButton.DoClick();
                }

                // jump back!
                var pos = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));
                var exit = (Vector3)Yoyo.markedLocation;
                if (SubmergedCompatibility.IsSubmerged)
                {
                    SubmergedCompatibility.ChangeFloor(exit.y > -7);
                }
                MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.YoyoBlink, Hazel.SendOption.Reliable);
                writer.Write((byte)0);
                writer.WriteBytesAndSize(buff);
                writer.EndMessage();
                RPCProcedure.yoyoBlink(false, buff);

                yoyoButton.Timer = yoyoButton.MaxTimer;
                yoyoButton.isEffectActive = false;
                yoyoButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                yoyoButton.hasEffect = false;
                yoyoButton.sprite = Yoyo.getMarkButtonSprite();
                yoyoButton.buttonText = "Mark Location";
                if (Minigame.Instance)
                {
                    Minigame.Instance.Close();
                }
            },
            buttonText: "Mark Location"
        );

        yoyoAdminTableButton = new CustomButton(
            () =>
            {
                if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                {
                    HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                    __instance.InitMap();
                    MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                }
            },
            () => { return Yoyo.yoyo != null && Yoyo.yoyo == PlayerControl.LocalPlayer && Yoyo.hasAdminTable && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                return true;
            },
            () =>
            {
                yoyoAdminTableButton.Timer = yoyoAdminTableButton.MaxTimer;
                yoyoAdminTableButton.isEffectActive = false;
                yoyoAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            Hacker.getAdminSprite(),
            ButtonOffset.LowerCenter,
            __instance,
            __instance.KillButton,
            KeyCode.G,
            true,
            0f,
            () =>
            {
                yoyoAdminTableButton.Timer = yoyoAdminTableButton.MaxTimer;
                if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
            },
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3,
            "ADMIN"
        );

        zoomOutButton = new CustomButton(
            () =>
            {
                Helpers.toggleZoom();
            },
            () =>
            {
                if (PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.Data.IsDead || (PlayerControl.LocalPlayer.Data.Role.IsImpostor && !CustomOptionHolder.deadImpsBlockSabotage.getBool())) return false;
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
                int numberOfLeftTasks = playerTotal - playerCompleted;
                return numberOfLeftTasks <= 0 || !CustomOptionHolder.finishTasksBeforeHauntingOrZoomingOut.getBool();
            },
            () => { return true; },
            () => { return; },
            null,  // Invisible button!
            new Vector3(0.4f, 2.8f, 0),
            __instance,
            __instance.UseButton,
            KeyCode.KeypadPlus
        );
        zoomOutButton.Timer = 0f;

        eventKickButton = new CustomButton(
            () =>
            {
                EventUtility.kickTarget();
            },
            () => { return EventUtility.isEnabled && Mini.mini != null && !Mini.mini.Data.IsDead && PlayerControl.LocalPlayer != Mini.mini; },
            () => { return EventUtility.currentTarget != null; },
            () => { },
            EventUtility.getKickButtonSprite(),
            ButtonOffset.HighRight,
            __instance,
            __instance.UseButton,
            KeyCode.K,
            true,
            3f,
            () =>
            {
                // onEffectEnds
                eventKickButton.Timer = 69;
            },
            buttonText: "KICK"
        );

        // Set the default (or settings from the previous game) timers / durations when spawning the buttons
        initialized = true;
        setCustomButtonCooldowns();
    }
}