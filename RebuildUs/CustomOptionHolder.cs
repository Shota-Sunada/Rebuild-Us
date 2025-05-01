using System.Collections.Generic;
using RebuildUs.Modules;
using RebuildUs.Roles.Crewmate;
using UnityEngine;
using static RebuildUs.RebuildUs;

namespace RebuildUs;

public class CustomOptionHolder
{
    public static string[] rates = ["0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"];
    public static readonly string[] Percents = ["0", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100"];
    public static readonly string[] Presets = ["Preset 1", "Preset 2", "Random Preset Skeld", "Random Preset Mira HQ", "Random Preset Polus", "Random Preset Airship", "Random Preset Submerged"];

    public const int PRESET_OPTION_ID = 0;

    public static CustomOption presetSelection;
    public static CustomOption activateRoles;

    public static CustomOption crewmateRolesCountMin;
    public static CustomOption crewmateRolesCountMax;
    public static CustomOption crewmateRolesFill;
    public static CustomOption neutralRolesCountMin;
    public static CustomOption neutralRolesCountMax;
    public static CustomOption impostorRolesCountMin;
    public static CustomOption impostorRolesCountMax;
    public static CustomOption modifiersCountMin;
    public static CustomOption modifiersCountMax;

    public static CustomOption anyPlayerCanStopStart;
    public static CustomOption enableEventMode;
    public static CustomOption eventReallyNoMini;
    public static CustomOption eventKicksPerRound;
    public static CustomOption eventHeavyAge;
    public static CustomOption deadImpsBlockSabotage;

    public static CustomRoleOption mafiaSpawnRate;
    public static CustomOption mafiosoCanSabotage;
    public static CustomOption mafiosoCanRepair;
    public static CustomOption mafiosoCanVent;
    public static CustomOption janitorCooldown;
    public static CustomOption janitorCanSabotage;
    public static CustomOption janitorCanRepair;
    public static CustomOption janitorCanVent;

    public static CustomOption morphingSpawnRate;
    public static CustomOption morphingCooldown;
    public static CustomOption morphingDuration;

    public static CustomOption camouflagerSpawnRate;
    public static CustomOption camouflagerCooldown;
    public static CustomOption camouflagerDuration;
    public static CustomOption camouflagerRandomColors;

    public static CustomOption vampireSpawnRate;
    public static CustomOption vampireKillDelay;
    public static CustomOption vampireCooldown;
    public static CustomOption vampireCanKillNearGarlics;

    public static CustomOption eraserSpawnRate;
    public static CustomOption eraserCooldown;
    public static CustomOption eraserCooldownIncrease;
    public static CustomOption eraserCanEraseAnyone;

    public static CustomOption guesserSpawnRate;
    public static CustomOption guesserIsImpGuesserRate;
    public static CustomOption guesserNumberOfShots;
    public static CustomOption guesserHasMultipleShotsPerMeeting;
    public static CustomOption guesserKillsThroughShield;
    public static CustomOption guesserEvilCanKillSpy;
    public static CustomOption guesserSpawnBothRate;
    public static CustomOption guesserCantGuessSnitchIfTaksDone;

    public static CustomOption jesterSpawnRate;
    public static CustomOption jesterCanCallEmergency;
    public static CustomOption jesterCanSabotage;
    public static CustomOption jesterHasImpostorVision;

    public static CustomOption arsonistSpawnRate;
    public static CustomOption arsonistCooldown;
    public static CustomOption arsonistDuration;
    public static CustomOption arsonistCanBeLovers;

    public static CustomOption jackalSpawnRate;
    public static CustomOption jackalKillCooldown;
    public static CustomOption jackalCanUseVents;
    public static CustomOption jackalCanSabotageLights;
    public static CustomOption teamJackalHaveImpostorVision;
    public static CustomOption jackalCanCreateSidekick;
    public static CustomOption jackalCreateSidekickCooldown;
    public static CustomOption sidekickPromotesToJackal;
    public static CustomOption jackalPromotedFromSidekickCanCreateSidekick;
    public static CustomOption sidekickCanKill;
    public static CustomOption sidekickCanUseVents;
    public static CustomOption sidekickCanSabotageLights;
    public static CustomOption jackalCanCreateSidekickFromImpostor;

    public static CustomOption bountyHunterSpawnRate;
    public static CustomOption bountyHunterBountyDuration;
    public static CustomOption bountyHunterReducedCooldown;
    public static CustomOption bountyHunterPunishmentTime;
    public static CustomOption bountyHunterShowArrow;
    public static CustomOption bountyHunterArrowUpdateInterval;

    public static CustomOption witchSpawnRate;
    public static CustomOption witchCooldown;
    public static CustomOption witchAdditionalCooldown;
    public static CustomOption witchCanSpellAnyone;
    public static CustomOption witchSpellCastingDuration;
    public static CustomOption witchTriggerBothCooldowns;
    public static CustomOption witchVoteSavesTargets;

    public static CustomOption ninjaSpawnRate;
    public static CustomOption ninjaCooldown;
    public static CustomOption ninjaKnowsTargetLocation;
    public static CustomOption ninjaTraceTime;
    public static CustomOption ninjaTraceColorTime;
    public static CustomOption ninjaInvisibleDuration;

    public static CustomOption mayorSpawnRate;
    public static CustomOption mayorNumVotes;
    public static CustomOption mayorCanSeeVoteColors;
    public static CustomOption mayorTasksNeededToSeeVoteColors;
    public static CustomOption mayorMeetingButton;
    public static CustomOption mayorMaxRemoteMeetings;
    public static CustomOption mayorChooseSingleVote;

    public static CustomOption portalmakerSpawnRate;
    public static CustomOption portalmakerCooldown;
    public static CustomOption portalmakerUsePortalCooldown;
    public static CustomOption portalmakerLogOnlyColorType;
    public static CustomOption portalmakerLogHasTime;
    public static CustomOption portalmakerCanPortalFromAnywhere;

    public static CustomOption engineerSpawnRate;
    public static CustomOption engineerNumberOfFixes;
    public static CustomOption engineerHighlightForImpostors;
    public static CustomOption engineerHighlightForTeamJackal;

    public static CustomOption sheriffSpawnRate;
    public static CustomOption sheriffCooldown;
    public static CustomOption sheriffNumShots;
    public static CustomOption sheriffMisfireKillsTarget;
    public static CustomOption sheriffCanKillNeutrals;

    public static CustomOption lighterSpawnRate;
    public static CustomOption lighterModeLightsOnVision;
    public static CustomOption lighterModeLightsOffVision;
    public static CustomOption lighterCooldown;
    public static CustomOption lighterDuration;
    public static CustomOption lighterFlashlightWidth;

    public static CustomOption detectiveSpawnRate;
    public static CustomOption detectiveAnonymousFootprints;
    public static CustomOption detectiveFootprintInterval;
    public static CustomOption detectiveFootprintDuration;
    public static CustomOption detectiveReportNameDuration;
    public static CustomOption detectiveReportColorDuration;

    public static CustomOption timeMasterSpawnRate;
    public static CustomOption timeMasterCooldown;
    public static CustomOption timeMasterRewindTime;
    public static CustomOption timeMasterShieldDuration;

    public static CustomOption medicSpawnRate;
    public static CustomOption medicShowShielded;
    public static CustomOption medicShowAttemptToShielded;
    public static CustomOption medicSetOrShowShieldAfterMeeting;
    public static CustomOption medicShowAttemptToMedic;
    public static CustomOption medicSetShieldAfterMeeting;

    public static CustomOption swapperSpawnRate;
    public static CustomOption swapperCanCallEmergency;
    public static CustomOption swapperCanOnlySwapOthers;
    public static CustomOption swapperSwapsNumber;
    public static CustomOption swapperRechargeTasksNumber;

    public static CustomOption seerSpawnRate;
    public static CustomOption seerMode;
    public static CustomOption seerSoulDuration;
    public static CustomOption seerLimitSoulDuration;

    public static CustomOption hackerSpawnRate;
    public static CustomOption hackerCooldown;
    public static CustomOption hackerHackingDuration;
    public static CustomOption hackerOnlyColorType;
    public static CustomOption hackerToolsNumber;
    public static CustomOption hackerRechargeTasksNumber;
    public static CustomOption hackerNoMove;

    public static CustomOption trackerSpawnRate;
    public static CustomOption trackerUpdateInterval;
    public static CustomOption trackerResetTargetAfterMeeting;
    public static CustomOption trackerCanTrackCorpses;
    public static CustomOption trackerCorpsesTrackingCooldown;
    public static CustomOption trackerCorpsesTrackingDuration;
    public static CustomOption trackerTrackingMethod;

    public static CustomOption snitchSpawnRate;
    public static CustomOption snitchLeftTasksForReveal;
    public static CustomOption snitchMode;
    public static CustomOption snitchTargets;

    public static CustomOption spySpawnRate;
    public static CustomOption spyCanDieToSheriff;
    public static CustomOption spyImpostorsCanKillAnyone;
    public static CustomOption spyCanEnterVents;
    public static CustomOption spyHasImpostorVision;

    public static CustomOption tricksterSpawnRate;
    public static CustomOption tricksterPlaceBoxCooldown;
    public static CustomOption tricksterLightsOutCooldown;
    public static CustomOption tricksterLightsOutDuration;

    public static CustomOption cleanerSpawnRate;
    public static CustomOption cleanerCooldown;

    public static CustomOption warlockSpawnRate;
    public static CustomOption warlockCooldown;
    public static CustomOption warlockRootTime;

    public static CustomOption securityGuardSpawnRate;
    public static CustomOption securityGuardCooldown;
    public static CustomOption securityGuardTotalScrews;
    public static CustomOption securityGuardCamPrice;
    public static CustomOption securityGuardVentPrice;
    public static CustomOption securityGuardCamDuration;
    public static CustomOption securityGuardCamMaxCharges;
    public static CustomOption securityGuardCamRechargeTasksNumber;
    public static CustomOption securityGuardNoMove;

    public static CustomOption vultureSpawnRate;
    public static CustomOption vultureCooldown;
    public static CustomOption vultureNumberToWin;
    public static CustomOption vultureCanUseVents;
    public static CustomOption vultureShowArrows;

    public static CustomOption mediumSpawnRate;
    public static CustomOption mediumCooldown;
    public static CustomOption mediumDuration;
    public static CustomOption mediumOneTimeUse;
    public static CustomOption mediumChanceAdditionalInfo;

    public static CustomOption lawyerSpawnRate;
    public static CustomOption lawyerIsProsecutorChance;
    public static CustomOption lawyerTargetCanBeJester;
    public static CustomOption lawyerVision;
    public static CustomOption lawyerKnowsRole;
    public static CustomOption lawyerCanCallEmergency;
    public static CustomOption pursuerCooldown;
    public static CustomOption pursuerBlanksNumber;

    public static CustomOption thiefSpawnRate;
    public static CustomOption thiefCooldown;
    public static CustomOption thiefHasImpVision;
    public static CustomOption thiefCanUseVents;
    public static CustomOption thiefCanKillSheriff;
    public static CustomOption thiefCanStealWithGuess;


    public static CustomOption trapperSpawnRate;
    public static CustomOption trapperCooldown;
    public static CustomOption trapperMaxCharges;
    public static CustomOption trapperRechargeTasksNumber;
    public static CustomOption trapperTrapNeededTriggerToReveal;
    public static CustomOption trapperAnonymousMap;
    public static CustomOption trapperInfoType;
    public static CustomOption trapperTrapDuration;

    public static CustomOption bomberSpawnRate;
    public static CustomOption bomberBombDestructionTime;
    public static CustomOption bomberBombDestructionRange;
    public static CustomOption bomberBombHearRange;
    public static CustomOption bomberDefuseDuration;
    public static CustomOption bomberBombCooldown;
    public static CustomOption bomberBombActiveAfter;

    public static CustomOption yoyoSpawnRate;
    public static CustomOption yoyoBlinkDuration;
    public static CustomOption yoyoMarkCooldown;
    public static CustomOption yoyoMarkStaysOverMeeting;
    public static CustomOption yoyoHasAdminTable;
    public static CustomOption yoyoAdminTableCooldown;
    public static CustomOption yoyoSilhouetteVisibility;



    public static CustomOption modifiersAreHidden;

    public static CustomOption modifierBait;
    public static CustomOption modifierBaitQuantity;
    public static CustomOption modifierBaitReportDelayMin;
    public static CustomOption modifierBaitReportDelayMax;
    public static CustomOption modifierBaitShowKillFlash;

    public static CustomOption modifierLover;
    public static CustomOption modifierLoverImpLoverRate;
    public static CustomOption modifierLoverBothDie;
    public static CustomOption modifierLoverEnableChat;

    public static CustomOption modifierBloody;
    public static CustomOption modifierBloodyQuantity;
    public static CustomOption modifierBloodyDuration;

    public static CustomOption modifierAntiTeleport;
    public static CustomOption modifierAntiTeleportQuantity;

    public static CustomOption modifierTieBreaker;

    public static CustomOption modifierSunglasses;
    public static CustomOption modifierSunglassesQuantity;
    public static CustomOption modifierSunglassesVision;

    public static CustomOption modifierMini;
    public static CustomOption modifierMiniGrowingUpDuration;
    public static CustomOption modifierMiniGrowingUpInMeeting;

    public static CustomOption modifierVip;
    public static CustomOption modifierVipQuantity;
    public static CustomOption modifierVipShowColor;

    public static CustomOption modifierInvert;
    public static CustomOption modifierInvertQuantity;
    public static CustomOption modifierInvertDuration;

    public static CustomOption modifierChameleon;
    public static CustomOption modifierChameleonQuantity;
    public static CustomOption modifierChameleonHoldDuration;
    public static CustomOption modifierChameleonFadeDuration;
    public static CustomOption modifierChameleonMinVisibility;

    public static CustomOption modifierArmored;

    public static CustomOption modifierShifter;
    public static CustomOption modifierShifterShiftsMedicShield;

    public static CustomOption maxNumberOfMeetings;
    public static CustomOption blockSkippingInEmergencyMeetings;
    public static CustomOption noVoteIsSelfVote;
    public static CustomOption hidePlayerNames;
    public static CustomOption allowParallelMedBayScans;
    public static CustomOption shieldFirstKill;
    public static CustomOption finishTasksBeforeHauntingOrZoomingOut;
    public static CustomOption camsNightVision;
    public static CustomOption camsNoNightVisionIfImpVision;

    public static CustomOption dynamicMap;
    public static CustomOption dynamicMapEnableSkeld;
    public static CustomOption dynamicMapEnableMira;
    public static CustomOption dynamicMapEnablePolus;
    public static CustomOption dynamicMapEnableAirShip;
    public static CustomOption dynamicMapEnableFungle;
    public static CustomOption dynamicMapEnableSubmerged;
    public static CustomOption dynamicMapSeparateSettings;

    internal static Dictionary<byte, byte[]> blockedRolePairings = [];

    public static void Load()
    {
        CustomOption.vanillaSettings = RebuildUsPlugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

        // Role Options
        presetSelection = CustomOption.Create(PRESET_OPTION_ID, CustomOptionType.General, ("SettingPreset", null), Presets, null, true);

        if (Utilities.EventUtility.canBeEnabled) enableEventMode = CustomOption.Create(1, CustomOptionType.General, ("Enable Special Mode", Color.green), true, null, true);

        #region General Settings

        // Using new id's for the options to not break compatibilty with older versions
        crewmateRolesCountMin = CustomOption.Create(10, CustomOptionType.General, ("CrewmateRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, null, true, ("MinMaxRoles", null), UnitType.UnitPlayers);
        crewmateRolesCountMax = CustomOption.Create(11, CustomOptionType.General, ("CrewmateRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        crewmateRolesFill = CustomOption.Create(12, CustomOptionType.General, ("CrewmateRolesFill", new Color32(204, 204, 0, 255)), false);
        neutralRolesCountMin = CustomOption.Create(13, CustomOptionType.General, ("NeutralRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        neutralRolesCountMax = CustomOption.Create(14, CustomOptionType.General, ("NeutralRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        impostorRolesCountMin = CustomOption.Create(15, CustomOptionType.General, ("ImpostorRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        impostorRolesCountMax = CustomOption.Create(16, CustomOptionType.General, ("ImpostorRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        modifiersCountMin = CustomOption.Create(17, CustomOptionType.General, ("ModifiersCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        modifiersCountMax = CustomOption.Create(18, CustomOptionType.General, ("ModifiersCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);

        maxNumberOfMeetings = CustomOption.Create(20, CustomOptionType.General, "Number Of Meetings (excluding Mayor meeting)", 10, 0, 15, 1, null, true, "Gameplay Settings");
        anyPlayerCanStopStart = CustomOption.Create(21, CustomOptionType.General, ("Any Player Can Stop The Start", new Color(204f / 255f, 204f / 255f, 0, 1f)), false, null, false);
        blockSkippingInEmergencyMeetings = CustomOption.Create(22, CustomOptionType.General, "Block Skipping In Emergency Meetings", false);
        noVoteIsSelfVote = CustomOption.Create(23, CustomOptionType.General, "No Vote Is Self Vote", false, blockSkippingInEmergencyMeetings);
        hidePlayerNames = CustomOption.Create(24, CustomOptionType.General, "Hide Player Names", false);
        allowParallelMedBayScans = CustomOption.Create(25, CustomOptionType.General, "Allow Parallel MedBay Scans", false);
        shieldFirstKill = CustomOption.Create(26, CustomOptionType.General, "Shield Last Game First Kill", false);
        finishTasksBeforeHauntingOrZoomingOut = CustomOption.Create(27, CustomOptionType.General, "Finish Tasks Before Haunting Or Zooming Out", true);
        deadImpsBlockSabotage = CustomOption.Create(28, CustomOptionType.General, "Block Dead Impostor From Sabotaging", false, null, false);
        camsNightVision = CustomOption.Create(29, CustomOptionType.General, "Cams Switch To Night Vision If Lights Are Off", false, null, true, "Night Vision Cams");
        camsNoNightVisionIfImpVision = CustomOption.Create(30, CustomOptionType.General, "Impostor Vision Ignores Night Vision Cams", false, camsNightVision, false);

        dynamicMap = CustomOption.Create(80, CustomOptionType.General, "Play On A Random Map", false, null, true, "Random Maps");
        dynamicMapEnableSkeld = CustomOption.Create(81, CustomOptionType.General, "Skeld", rates, dynamicMap, false);
        dynamicMapEnableMira = CustomOption.Create(82, CustomOptionType.General, "Mira", rates, dynamicMap, false);
        dynamicMapEnablePolus = CustomOption.Create(83, CustomOptionType.General, "Polus", rates, dynamicMap, false);
        dynamicMapEnableAirShip = CustomOption.Create(84, CustomOptionType.General, "Airship", rates, dynamicMap, false);
        dynamicMapEnableFungle = CustomOption.Create(85, CustomOptionType.General, "Fungle", rates, dynamicMap, false);
        dynamicMapEnableSubmerged = CustomOption.Create(86, CustomOptionType.General, "Submerged", rates, dynamicMap, false);
        dynamicMapSeparateSettings = CustomOption.Create(87, CustomOptionType.General, "Use Random Map Setting Presets", false, dynamicMap, false);

        #endregion

        #region Impostor Roles

        mafiaSpawnRate = new CustomRoleOption(100, 101, CustomOptionType.Impostor, ("mafia", Janitor.color), 1);
        mafiosoCanSabotage = CustomOption.Create(102, CustomOptionType.Impostor, "MafiosoCanSabotage", false, mafiaSpawnRate);
        mafiosoCanRepair = CustomOption.Create(103, CustomOptionType.Impostor, "MafiosoCanRepair", false, mafiaSpawnRate);
        mafiosoCanVent = CustomOption.Create(104, CustomOptionType.Impostor, "MafiosoCanVent", false, mafiaSpawnRate);
        janitorCooldown = CustomOption.Create(105, CustomOptionType.Impostor, "JanitorCooldown", 30f, 2.5f, 60f, 2.5f, mafiaSpawnRate, unitType: UnitType.UnitSeconds);
        janitorCanSabotage = CustomOption.Create(106, CustomOptionType.Impostor, "JanitorCanSabotage", false, mafiaSpawnRate);
        janitorCanRepair = CustomOption.Create(107, CustomOptionType.Impostor, "JanitorCanRepair", false, mafiaSpawnRate);
        janitorCanVent = CustomOption.Create(108, CustomOptionType.Impostor, "JanitorCanVent", false, mafiaSpawnRate);

        morphingSpawnRate = new CustomRoleOption(120, 121, CustomOptionType.Impostor, ("morphing", Morphing.color), 1);
        morphingCooldown = CustomOption.Create(122, CustomOptionType.Impostor, "morphingCooldown", 30f, 2.5f, 60f, 2.5f, morphingSpawnRate, unitType: UnitType.UnitSeconds);
        morphingDuration = CustomOption.Create(123, CustomOptionType.Impostor, "morphingDuration", 10f, 1f, 20f, 0.5f, morphingSpawnRate, unitType: UnitType.UnitSeconds);

        camouflagerSpawnRate = new CustomRoleOption(130, 131, CustomOptionType.Impostor, ("camouflager", Camouflager.color), 1);
        camouflagerCooldown = CustomOption.Create(132, CustomOptionType.Impostor, "camouflagerCooldown", 30f, 2.5f, 60f, 2.5f, camouflagerSpawnRate, unitType: UnitType.UnitSeconds);
        camouflagerDuration = CustomOption.Create(133, CustomOptionType.Impostor, "camouflagerDuration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate, unitType: UnitType.UnitSeconds);
        camouflagerRandomColors = CustomOption.Create(134, CustomOptionType.Impostor, "camouflagerRandomColors", false, camouflagerSpawnRate);

        vampireSpawnRate = new CustomRoleOption(140, 141, CustomOptionType.Impostor, ("vampire", Vampire.color), 1);
        vampireKillDelay = CustomOption.Create(142, CustomOptionType.Impostor, "vampireKillDelay", 10f, 1f, 20f, 1f, vampireSpawnRate, unitType: UnitType.UnitSeconds);
        vampireCooldown = CustomOption.Create(143, CustomOptionType.Impostor, "vampireCooldown", 30f, 2.5f, 60f, 2.5f, vampireSpawnRate, unitType: UnitType.UnitSeconds);
        vampireCanKillNearGarlics = CustomOption.Create(144, CustomOptionType.Impostor, "vampireCanKillNearGarlics", true, vampireSpawnRate);

        eraserSpawnRate = new CustomRoleOption(150, 151, CustomOptionType.Impostor, ("eraser", Eraser.color), 1);
        eraserCooldown = CustomOption.Create(152, CustomOptionType.Impostor, "eraserCooldown", 30f, 5f, 120f, 5f, eraserSpawnRate, unitType: UnitType.UnitSeconds);
        eraserCooldownIncrease = CustomOption.Create(153, CustomOptionType.Impostor, "eraserCooldownIncrease", 10f, 0f, 120f, 2.5f, eraserSpawnRate, unitType: UnitType.UnitSeconds);
        eraserCanEraseAnyone = CustomOption.Create(154, CustomOptionType.Impostor, "eraserCanEraseAnyone", false, eraserSpawnRate);

        tricksterSpawnRate = new CustomRoleOption(160, 161, CustomOptionType.Impostor, ("trickster", Trickster.color), 1);
        tricksterPlaceBoxCooldown = CustomOption.Create(162, CustomOptionType.Impostor, "tricksterPlaceBoxCooldown", 10f, 2.5f, 30f, 2.5f, tricksterSpawnRate, unitType: UnitType.UnitSeconds);
        tricksterLightsOutCooldown = CustomOption.Create(163, CustomOptionType.Impostor, "tricksterLightsOutCooldown", 30f, 5f, 60f, 5f, tricksterSpawnRate, unitType: UnitType.UnitSeconds);
        tricksterLightsOutDuration = CustomOption.Create(164, CustomOptionType.Impostor, "tricksterLightsOutDuration", 15f, 5f, 60f, 2.5f, tricksterSpawnRate, unitType: UnitType.UnitSeconds);

        cleanerSpawnRate = new CustomRoleOption(170, 171, CustomOptionType.Impostor, ("cleaner", Cleaner.color), 1);
        cleanerCooldown = CustomOption.Create(172, CustomOptionType.Impostor, "cleanerCooldown", 30f, 2.5f, 60f, 2.5f, cleanerSpawnRate, unitType: UnitType.UnitSeconds);

        warlockSpawnRate = new CustomRoleOption(180, 181, CustomOptionType.Impostor, ("warlock", Warlock.color), 1);
        warlockCooldown = CustomOption.Create(182, CustomOptionType.Impostor, "warlockCooldown", 30f, 2.5f, 60f, 2.5f, warlockSpawnRate, unitType: UnitType.UnitSeconds);
        warlockRootTime = CustomOption.Create(183, CustomOptionType.Impostor, "warlockRootTime", 5f, 0f, 15f, 1f, warlockSpawnRate, unitType: UnitType.UnitSeconds);

        bountyHunterSpawnRate = new CustomRoleOption(190, 191, CustomOptionType.Impostor, ("bountyHunter", BountyHunter.color), 1);
        bountyHunterBountyDuration = CustomOption.Create(192, CustomOptionType.Impostor, "bountyHunterBountyDuration", 60f, 10f, 180f, 10f, bountyHunterSpawnRate, unitType: UnitType.UnitSeconds);
        bountyHunterReducedCooldown = CustomOption.Create(193, CustomOptionType.Impostor, "bountyHunterReducedCooldown", 2.5f, 2.5f, 30f, 2.5f, bountyHunterSpawnRate, unitType: UnitType.UnitSeconds);
        bountyHunterPunishmentTime = CustomOption.Create(194, CustomOptionType.Impostor, "bountyHunterPunishmentTime", 20f, 0f, 60f, 2.5f, bountyHunterSpawnRate, unitType: UnitType.UnitSeconds);
        bountyHunterShowArrow = CustomOption.Create(195, CustomOptionType.Impostor, "bountyHunterShowArrow", true, bountyHunterSpawnRate);
        bountyHunterArrowUpdateInterval = CustomOption.Create(196, CustomOptionType.Impostor, "bountyHunterArrowUpdateInterval", 15f, 2.5f, 60f, 2.5f, bountyHunterShowArrow, unitType: UnitType.UnitSeconds);

        witchSpawnRate = new CustomRoleOption(200, 201, CustomOptionType.Impostor, ("witch", Witch.color), 1);
        witchCooldown = CustomOption.Create(202, CustomOptionType.Impostor, "witchSpellCooldown", 30f, 2.5f, 120f, 2.5f, witchSpawnRate, unitType: UnitType.UnitSeconds);
        witchAdditionalCooldown = CustomOption.Create(203, CustomOptionType.Impostor, "witchAdditionalCooldown", 10f, 0f, 60f, 5f, witchSpawnRate, unitType: UnitType.UnitSeconds);
        witchCanSpellAnyone = CustomOption.Create(204, CustomOptionType.Impostor, "witchCanSpellAnyone", false, witchSpawnRate);
        witchSpellCastingDuration = CustomOption.Create(205, CustomOptionType.Impostor, "witchSpellDuration", 1f, 0f, 10f, 1f, witchSpawnRate, unitType: UnitType.UnitSeconds);
        witchTriggerBothCooldowns = CustomOption.Create(206, CustomOptionType.Impostor, "witchTriggerBoth", true, witchSpawnRate);
        witchVoteSavesTargets = CustomOption.Create(207, CustomOptionType.Impostor, "witchSaveTargets", true, witchSpawnRate);

        ninjaSpawnRate = new CustomRoleOption(210, 211, CustomOptionType.Impostor, ("Ninja", Ninja.color), 1);
        ninjaCooldown = CustomOption.Create(212, CustomOptionType.Impostor, "Ninja Mark Cooldown", 30f, 10f, 120f, 5f, ninjaSpawnRate, unitType: UnitType.UnitSeconds);
        ninjaKnowsTargetLocation = CustomOption.Create(213, CustomOptionType.Impostor, "Ninja Knows Location Of Target", true, ninjaSpawnRate);
        ninjaTraceTime = CustomOption.Create(214, CustomOptionType.Impostor, "Trace Duration", 5f, 1f, 20f, 0.5f, ninjaSpawnRate, unitType: UnitType.UnitSeconds);
        ninjaTraceColorTime = CustomOption.Create(215, CustomOptionType.Impostor, "Time Till Trace Color Has Faded", 2f, 0f, 20f, 0.5f, ninjaSpawnRate, unitType: UnitType.UnitSeconds);
        ninjaInvisibleDuration = CustomOption.Create(216, CustomOptionType.Impostor, "Time The Ninja Is Invisible", 3f, 0f, 20f, 1f, ninjaSpawnRate);

        bomberSpawnRate = new CustomRoleOption(220, 221, CustomOptionType.Impostor, ("Bomber", Bomber.color), 1);
        bomberBombDestructionTime = CustomOption.Create(222, CustomOptionType.Impostor, "Bomb Destruction Time", 20f, 2.5f, 120f, 2.5f, bomberSpawnRate, unitType: UnitType.UnitSeconds);
        bomberBombDestructionRange = CustomOption.Create(223, CustomOptionType.Impostor, "Bomb Destruction Range", 50f, 5f, 150f, 5f, bomberSpawnRate);
        bomberBombHearRange = CustomOption.Create(224, CustomOptionType.Impostor, "Bomb Hear Range", 60f, 5f, 150f, 5f, bomberSpawnRate);
        bomberDefuseDuration = CustomOption.Create(225, CustomOptionType.Impostor, "Bomb Defuse Duration", 3f, 0.5f, 30f, 0.5f, bomberSpawnRate, unitType: UnitType.UnitSeconds);
        bomberBombCooldown = CustomOption.Create(226, CustomOptionType.Impostor, "Bomb Cooldown", 15f, 2.5f, 30f, 2.5f, bomberSpawnRate, unitType: UnitType.UnitSeconds);
        bomberBombActiveAfter = CustomOption.Create(227, CustomOptionType.Impostor, "Bomb Is Active After", 3f, 0.5f, 15f, 0.5f, bomberSpawnRate, unitType: UnitType.UnitSeconds);

        yoyoSpawnRate = new CustomRoleOption(230, 231, CustomOptionType.Impostor, ("Yo-Yo", Yoyo.color), 1);
        yoyoBlinkDuration = CustomOption.Create(232, CustomOptionType.Impostor, "Blink Duration", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate, unitType: UnitType.UnitSeconds);
        yoyoMarkCooldown = CustomOption.Create(233, CustomOptionType.Impostor, "Mark Location Cooldown", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate, unitType: UnitType.UnitSeconds);
        yoyoMarkStaysOverMeeting = CustomOption.Create(234, CustomOptionType.Impostor, "Marked Location Stays After Meeting", true, yoyoSpawnRate);
        yoyoHasAdminTable = CustomOption.Create(235, CustomOptionType.Impostor, "Has Admin Table", true, yoyoSpawnRate);
        yoyoAdminTableCooldown = CustomOption.Create(236, CustomOptionType.Impostor, "Admin Table Cooldown", 20f, 2.5f, 120f, 2.5f, yoyoHasAdminTable, unitType: UnitType.UnitSeconds);
        yoyoSilhouetteVisibility = CustomOption.Create(237, CustomOptionType.Impostor, "Silhouette Visibility", ["0", "10", "20", "30", "40", "50"], yoyoSpawnRate, unitType: UnitType.UnitPercent);

        #endregion

        #region Neutral Roles

        // guesserSpawnRate = CustomOption.Create(310, CustomOptionType.Neutral, cs("Guesser", Guesser.color),1);
        // guesserIsImpGuesserRate = CustomOption.Create(311, CustomOptionType.Neutral, "Chance That The Guesser Is An Impostor", rates, guesserSpawnRate);
        // guesserNumberOfShots = CustomOption.Create(312, CustomOptionType.Neutral, "Guesser Number Of Shots", 2f, 1f, 15f, 1f, guesserSpawnRate);
        // guesserHasMultipleShotsPerMeeting = CustomOption.Create(313, CustomOptionType.Neutral, "Guesser Can Shoot Multiple Times Per Meeting", false, guesserSpawnRate);
        // guesserKillsThroughShield = CustomOption.Create(315, CustomOptionType.Neutral, "Guesses Ignore The Medic Shield", true, guesserSpawnRate);
        // guesserEvilCanKillSpy = CustomOption.Create(316, CustomOptionType.Neutral, "Evil Guesser Can Guess The Spy", true, guesserSpawnRate);
        // guesserSpawnBothRate = CustomOption.Create(317, CustomOptionType.Neutral, "Both Guesser Spawn Rate", rates, guesserSpawnRate);
        // guesserCantGuessSnitchIfTaksDone = CustomOption.Create(318, CustomOptionType.Neutral, "Guesser Can't Guess Snitch When Tasks Completed", true, guesserSpawnRate);

        jesterSpawnRate = new CustomRoleOption(500, 501, CustomOptionType.Neutral, ("jester", Jester.color), 1);
        jesterCanCallEmergency = CustomOption.Create(502, CustomOptionType.Neutral, "jesterCanCallEmergency", true, jesterSpawnRate);
        jesterCanSabotage = CustomOption.Create(503, CustomOptionType.Neutral, "jesterCanSabotage", true, jesterSpawnRate);
        jesterHasImpostorVision = CustomOption.Create(504, CustomOptionType.Neutral, "jesterHasImpostorVision", false, jesterSpawnRate);

        arsonistSpawnRate = new CustomRoleOption(510, 511, CustomOptionType.Neutral, ("arsonist", Arsonist.color), 1);
        arsonistCooldown = CustomOption.Create(512, CustomOptionType.Neutral, "arsonistCooldown", 12.5f, 2.5f, 60f, 2.5f, arsonistSpawnRate, unitType: UnitType.UnitSeconds);
        arsonistDuration = CustomOption.Create(513, CustomOptionType.Neutral, "arsonistDuration", 3f, 0f, 10f, 1f, arsonistSpawnRate, unitType: UnitType.UnitSeconds);
        arsonistCanBeLovers = CustomOption.Create(514, CustomOptionType.Neutral, "arsonistCanBeLovers", false, arsonistSpawnRate);

        jackalSpawnRate = new CustomRoleOption(520, 521, CustomOptionType.Neutral, ("Jackal", Jackal.color), 1);
        jackalKillCooldown = CustomOption.Create(522, CustomOptionType.Neutral, "Jackal/Sidekick Kill Cooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate, unitType: UnitType.UnitSeconds);
        jackalCanUseVents = CustomOption.Create(523, CustomOptionType.Neutral, "Jackal Can Use Vents", true, jackalSpawnRate);
        jackalCanSabotageLights = CustomOption.Create(524, CustomOptionType.Neutral, "Jackal Can Sabotage Lights", true, jackalSpawnRate);
        teamJackalHaveImpostorVision = CustomOption.Create(525, CustomOptionType.Neutral, "Jackal And Sidekick Have Impostor Vision", false, jackalSpawnRate);
        jackalCanCreateSidekick = CustomOption.Create(526, CustomOptionType.Neutral, "Jackal Can Create A Sidekick", false, jackalSpawnRate);
        jackalCreateSidekickCooldown = CustomOption.Create(527, CustomOptionType.Neutral, "Jackal Create Sidekick Cooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate, unitType: UnitType.UnitSeconds);
        sidekickPromotesToJackal = CustomOption.Create(528, CustomOptionType.Neutral, "Sidekick Gets Promoted To Jackal On Jackal Death", false, jackalCanCreateSidekick);
        jackalPromotedFromSidekickCanCreateSidekick = CustomOption.Create(529, CustomOptionType.Neutral, "Jackals Promoted From Sidekick Can Create A Sidekick", true, sidekickPromotesToJackal);
        sidekickCanKill = CustomOption.Create(530, CustomOptionType.Neutral, "Sidekick Can Kill", false, jackalCanCreateSidekick);
        sidekickCanUseVents = CustomOption.Create(531, CustomOptionType.Neutral, "Sidekick Can Use Vents", true, jackalCanCreateSidekick);
        sidekickCanSabotageLights = CustomOption.Create(532, CustomOptionType.Neutral, "Sidekick Can Sabotage Lights", true, jackalCanCreateSidekick);
        jackalCanCreateSidekickFromImpostor = CustomOption.Create(533, CustomOptionType.Neutral, "Jackals Can Make An Impostor To His Sidekick", true, jackalCanCreateSidekick);

        vultureSpawnRate = new CustomRoleOption(540, 541, CustomOptionType.Neutral, ("vulture", Vulture.color), 1);
        vultureCooldown = CustomOption.Create(542, CustomOptionType.Neutral, "vultureCooldown", 15f, 2.5f, 60f, 2.5f, vultureSpawnRate, unitType: UnitType.UnitSeconds);
        vultureNumberToWin = CustomOption.Create(543, CustomOptionType.Neutral, "vultureNumberToWin", 4f, 1f, 12f, 1f, vultureSpawnRate, unitType: UnitType.UnitTimes);
        vultureCanUseVents = CustomOption.Create(544, CustomOptionType.Neutral, "vultureCanUseVents", true, vultureSpawnRate);
        vultureShowArrows = CustomOption.Create(545, CustomOptionType.Neutral, "vultureShowArrows", true, vultureSpawnRate);

        lawyerSpawnRate = new CustomRoleOption(550, 551, CustomOptionType.Neutral, ("Lawyer", Lawyer.color), 1);
        lawyerIsProsecutorChance = CustomOption.Create(552, CustomOptionType.Neutral, "Chance That The Lawyer Is Prosecutor", rates, lawyerSpawnRate, unitType: UnitType.UnitPercent);
        lawyerVision = CustomOption.Create(553, CustomOptionType.Neutral, "Vision", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate);
        lawyerKnowsRole = CustomOption.Create(554, CustomOptionType.Neutral, "Lawyer/Prosecutor Knows Target Role", false, lawyerSpawnRate);
        lawyerCanCallEmergency = CustomOption.Create(555, CustomOptionType.Neutral, "Lawyer/Prosecutor Can Call Emergency Meeting", true, lawyerSpawnRate);
        lawyerTargetCanBeJester = CustomOption.Create(556, CustomOptionType.Neutral, "Lawyer Target Can Be The Jester", false, lawyerSpawnRate);
        pursuerCooldown = CustomOption.Create(557, CustomOptionType.Neutral, "Pursuer Blank Cooldown", 30f, 5f, 60f, 2.5f, lawyerSpawnRate);
        pursuerBlanksNumber = CustomOption.Create(558, CustomOptionType.Neutral, "Pursuer Number Of Blanks", 5f, 1f, 20f, 1f, lawyerSpawnRate);

        thiefSpawnRate = new CustomRoleOption(560, 561, CustomOptionType.Neutral, ("Thief", Thief.color), 1);
        thiefCooldown = CustomOption.Create(562, CustomOptionType.Neutral, "Thief Cooldown", 30f, 5f, 120f, 5f, thiefSpawnRate, unitType: UnitType.UnitSeconds);
        thiefCanKillSheriff = CustomOption.Create(563, CustomOptionType.Neutral, "Thief Can Kill Sheriff", true, thiefSpawnRate);
        thiefHasImpVision = CustomOption.Create(564, CustomOptionType.Neutral, "Thief Has Impostor Vision", true, thiefSpawnRate);
        thiefCanUseVents = CustomOption.Create(565, CustomOptionType.Neutral, "Thief Can Use Vents", true, thiefSpawnRate);
        thiefCanStealWithGuess = CustomOption.Create(566, CustomOptionType.Neutral, "Thief Can Guess To Steal A Role (If Guesser)", false, thiefSpawnRate);

        #endregion

        #region Crewmate Roles

        mayorSpawnRate = new CustomRoleOption(700, 701, CustomOptionType.Crewmate, ("Mayor", Mayor.color), 1);
        mayorNumVotes = CustomOption.Create(702, CustomOptionType.Crewmate, "mayorNumVotes", 2f, 2f, 10f, 1f, mayorSpawnRate, unitType: UnitType.UnitVotes);
        mayorCanSeeVoteColors = CustomOption.Create(703, CustomOptionType.Crewmate, "Mayor Can See Vote Colors", false, mayorSpawnRate);
        mayorTasksNeededToSeeVoteColors = CustomOption.Create(704, CustomOptionType.Crewmate, "Completed Tasks Needed To See Vote Colors", 5f, 0f, 20f, 1f, mayorCanSeeVoteColors, unitType: UnitType.UnitTimes);
        mayorMeetingButton = CustomOption.Create(705, CustomOptionType.Crewmate, "Mobile Emergency Button", true, mayorSpawnRate);
        mayorMaxRemoteMeetings = CustomOption.Create(706, CustomOptionType.Crewmate, "Number Of Remote Meetings", 1f, 1f, 5f, 1f, mayorMeetingButton, unitType: UnitType.UnitTimes);
        mayorChooseSingleVote = CustomOption.Create(707, CustomOptionType.Crewmate, "Mayor Can Choose Single Vote", ["Off", "On (Before Voting)", "On (Until Meeting Ends)"], mayorSpawnRate);

        engineerSpawnRate = new CustomRoleOption(710, 711, CustomOptionType.Crewmate, ("Engineer", Engineer.color), 1);
        engineerNumberOfFixes = CustomOption.Create(712, CustomOptionType.Crewmate, "Number Of Sabotage Fixes", 1f, 1f, 3f, 1f, engineerSpawnRate, unitType: UnitType.UnitTimes);
        engineerHighlightForImpostors = CustomOption.Create(713, CustomOptionType.Crewmate, "Impostors See Vents Highlighted", true, engineerSpawnRate);
        engineerHighlightForTeamJackal = CustomOption.Create(714, CustomOptionType.Crewmate, "Jackal and Sidekick See Vents Highlighted ", true, engineerSpawnRate);

        sheriffSpawnRate = new CustomRoleOption(720, 721, CustomOptionType.Crewmate, ("Sheriff", Roles.Crewmate.Sheriff.color), 15);
        sheriffCooldown = CustomOption.Create(722, CustomOptionType.Crewmate, "Sheriff Cooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate, unitType: UnitType.UnitSeconds);
        sheriffNumShots = CustomOption.Create(723, CustomOptionType.Crewmate, "Sheriff Num Shots", 2f, 1f, 15f, 1f, sheriffSpawnRate, unitType: UnitType.UnitTimes);
        sheriffMisfireKillsTarget = CustomOption.Create(724, CustomOptionType.Crewmate, "sheriffMisfireKillsTarget", false, sheriffSpawnRate);
        sheriffCanKillNeutrals = CustomOption.Create(725, CustomOptionType.Crewmate, "Sheriff Can Kill Neutrals", false, sheriffSpawnRate);

        lighterSpawnRate = new CustomRoleOption(730, 731, CustomOptionType.Crewmate, ("Lighter", Lighter.color), 1);
        lighterModeLightsOnVision = CustomOption.Create(732, CustomOptionType.Crewmate, "Vision On Lights On", 1.5f, 0.25f, 5f, 0.25f, lighterSpawnRate);
        lighterModeLightsOffVision = CustomOption.Create(733, CustomOptionType.Crewmate, "Vision On Lights Off", 0.5f, 0.25f, 5f, 0.25f, lighterSpawnRate);
        lighterCooldown = CustomOption.Create(734, CustomOptionType.Crewmate, "lighterCooldown", 30f, 5f, 120f, 5f, lighterSpawnRate, unitType: UnitType.UnitSeconds);
        lighterDuration = CustomOption.Create(735, CustomOptionType.Crewmate, "lighterDuration", 5f, 2.5f, 60f, 2.5f, lighterSpawnRate, unitType: UnitType.UnitSeconds);
        lighterFlashlightWidth = CustomOption.Create(736, CustomOptionType.Crewmate, "Flashlight Width", 0.3f, 0.1f, 1f, 0.1f, lighterSpawnRate);

        detectiveSpawnRate = new CustomRoleOption(740, 741, CustomOptionType.Crewmate, ("Detective", Detective.color), 1);
        detectiveAnonymousFootprints = CustomOption.Create(742, CustomOptionType.Crewmate, "Anonymous Footprints", false, detectiveSpawnRate);
        detectiveFootprintInterval = CustomOption.Create(743, CustomOptionType.Crewmate, "Footprint Interval", 0.5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, unitType: UnitType.UnitSeconds);
        detectiveFootprintDuration = CustomOption.Create(744, CustomOptionType.Crewmate, "Footprint Duration", 5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, unitType: UnitType.UnitSeconds);
        detectiveReportNameDuration = CustomOption.Create(745, CustomOptionType.Crewmate, "Time Where Detective Reports Will Have Name", 0, 0, 60, 2.5f, detectiveSpawnRate, unitType: UnitType.UnitSeconds);
        detectiveReportColorDuration = CustomOption.Create(746, CustomOptionType.Crewmate, "Time Where Detective Reports Will Have Color Type", 20, 0, 120, 2.5f, detectiveSpawnRate, unitType: UnitType.UnitSeconds);

        timeMasterSpawnRate = new CustomRoleOption(750, 751, CustomOptionType.Crewmate, ("Time Master", TimeMaster.color), 1);
        timeMasterCooldown = CustomOption.Create(752, CustomOptionType.Crewmate, "Time Master Cooldown", 30f, 10f, 120f, 2.5f, timeMasterSpawnRate, unitType: UnitType.UnitSeconds);
        timeMasterRewindTime = CustomOption.Create(753, CustomOptionType.Crewmate, "Rewind Time", 3f, 1f, 10f, 1f, timeMasterSpawnRate, unitType: UnitType.UnitSeconds);
        timeMasterShieldDuration = CustomOption.Create(754, CustomOptionType.Crewmate, "Time Master Shield Duration", 3f, 1f, 20f, 1f, timeMasterSpawnRate, unitType: UnitType.UnitSeconds);

        medicSpawnRate = new CustomRoleOption(760, 761, CustomOptionType.Crewmate, ("Medic", Medic.color), 1);
        medicShowShielded = CustomOption.Create(762, CustomOptionType.Crewmate, "Show Shielded Player", ["Everyone", "Shielded + Medic", "Medic"], medicSpawnRate);
        medicShowAttemptToShielded = CustomOption.Create(763, CustomOptionType.Crewmate, "Shielded Player Sees Murder Attempt", false, medicSpawnRate);
        medicSetOrShowShieldAfterMeeting = CustomOption.Create(764, CustomOptionType.Crewmate, "Shield Will Be Activated", ["Instantly", "Instantly, Visible\nAfter Meeting", "After Meeting"], medicSpawnRate);
        medicShowAttemptToMedic = CustomOption.Create(765, CustomOptionType.Crewmate, "Medic Sees Murder Attempt On Shielded Player", false, medicSpawnRate);

        seerSpawnRate = new CustomRoleOption(770, 771, CustomOptionType.Crewmate, ("Seer", Seer.color), 1);
        seerMode = CustomOption.Create(772, CustomOptionType.Crewmate, "Seer Mode", ["Show Death Flash + Souls", "Show Death Flash", "Show Souls"], seerSpawnRate);
        seerLimitSoulDuration = CustomOption.Create(773, CustomOptionType.Crewmate, "Seer Limit Soul Duration", false, seerSpawnRate);
        seerSoulDuration = CustomOption.Create(774, CustomOptionType.Crewmate, "Seer Soul Duration", 15f, 0f, 120f, 5f, seerLimitSoulDuration, unitType: UnitType.UnitSeconds);

        hackerSpawnRate = new CustomRoleOption(780, 781, CustomOptionType.Crewmate, ("Hacker", Hacker.color), 1);
        hackerCooldown = CustomOption.Create(782, CustomOptionType.Crewmate, "Hacker Cooldown", 30f, 5f, 60f, 5f, hackerSpawnRate, unitType: UnitType.UnitSeconds);
        hackerHackingDuration = CustomOption.Create(783, CustomOptionType.Crewmate, "Hacker Duration", 10f, 2.5f, 60f, 2.5f, hackerSpawnRate, unitType: UnitType.UnitSeconds);
        hackerOnlyColorType = CustomOption.Create(784, CustomOptionType.Crewmate, "Hacker Only Sees Color Type", false, hackerSpawnRate);
        hackerToolsNumber = CustomOption.Create(785, CustomOptionType.Crewmate, "Max Mobile Gadget Charges", 5f, 1f, 30f, 1f, hackerSpawnRate);
        hackerRechargeTasksNumber = CustomOption.Create(786, CustomOptionType.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 5f, 1f, hackerSpawnRate);
        hackerNoMove = CustomOption.Create(787, CustomOptionType.Crewmate, "Cant Move During Mobile Gadget Duration", true, hackerSpawnRate);

        trackerSpawnRate = new CustomRoleOption(790, 791, CustomOptionType.Crewmate, ("Tracker", Tracker.color), 1);
        trackerUpdateInterval = CustomOption.Create(792, CustomOptionType.Crewmate, "Tracker Update Interval", 5f, 1f, 30f, 1f, trackerSpawnRate, unitType: UnitType.UnitSeconds);
        trackerResetTargetAfterMeeting = CustomOption.Create(793, CustomOptionType.Crewmate, "Tracker Reset Target After Meeting", false, trackerSpawnRate);
        trackerCanTrackCorpses = CustomOption.Create(794, CustomOptionType.Crewmate, "Tracker Can Track Corpses", true, trackerSpawnRate);
        trackerCorpsesTrackingCooldown = CustomOption.Create(795, CustomOptionType.Crewmate, "Corpses Tracking Cooldown", 30f, 5f, 120f, 5f, trackerCanTrackCorpses, unitType: UnitType.UnitSeconds);
        trackerCorpsesTrackingDuration = CustomOption.Create(796, CustomOptionType.Crewmate, "Corpses Tracking Duration", 5f, 2.5f, 30f, 2.5f, trackerCanTrackCorpses, unitType: UnitType.UnitSeconds);
        trackerTrackingMethod = CustomOption.Create(797, CustomOptionType.Crewmate, "How Tracker Gets Target Location", ["Arrow Only", "Proximity Dectector Only", "Arrow + Proximity"], trackerSpawnRate);

        snitchSpawnRate = new CustomRoleOption(800, 801, CustomOptionType.Crewmate, ("Snitch", Snitch.color), 1);
        snitchLeftTasksForReveal = CustomOption.Create(802, CustomOptionType.Crewmate, "Task Count Where The Snitch Will Be Revealed", 5f, 0f, 25f, 1f, snitchSpawnRate);
        snitchMode = CustomOption.Create(803, CustomOptionType.Crewmate, "Information Mode", ["Chat", "Map", "Chat & Map"], snitchSpawnRate);
        snitchTargets = CustomOption.Create(804, CustomOptionType.Crewmate, "Targets", ["All Evil Players", "Killing Players"], snitchSpawnRate);

        spySpawnRate = new CustomRoleOption(810, 811, CustomOptionType.Crewmate, ("Spy", Spy.color), 1);
        spyCanDieToSheriff = CustomOption.Create(812, CustomOptionType.Crewmate, "Spy Can Die To Sheriff", false, spySpawnRate);
        spyImpostorsCanKillAnyone = CustomOption.Create(813, CustomOptionType.Crewmate, "Impostors Can Kill Anyone If There Is A Spy", true, spySpawnRate);
        spyCanEnterVents = CustomOption.Create(814, CustomOptionType.Crewmate, "Spy Can Enter Vents", false, spySpawnRate);
        spyHasImpostorVision = CustomOption.Create(815, CustomOptionType.Crewmate, "Spy Has Impostor Vision", false, spySpawnRate);

        portalmakerSpawnRate = new CustomRoleOption(820, 821, CustomOptionType.Crewmate, ("Portalmaker", Portalmaker.color), 1);
        portalmakerCooldown = CustomOption.Create(822, CustomOptionType.Crewmate, "Portalmaker Cooldown", 30f, 10f, 60f, 2.5f, portalmakerSpawnRate, unitType: UnitType.UnitSeconds);
        portalmakerUsePortalCooldown = CustomOption.Create(823, CustomOptionType.Crewmate, "Use Portal Cooldown", 30f, 10f, 60f, 2.5f, portalmakerSpawnRate, unitType: UnitType.UnitSeconds);
        portalmakerLogOnlyColorType = CustomOption.Create(824, CustomOptionType.Crewmate, "Portalmaker Log Only Shows Color Type", true, portalmakerSpawnRate);
        portalmakerLogHasTime = CustomOption.Create(825, CustomOptionType.Crewmate, "Log Shows Time", true, portalmakerSpawnRate);
        portalmakerCanPortalFromAnywhere = CustomOption.Create(826, CustomOptionType.Crewmate, "Can Port To Portal From Everywhere", true, portalmakerSpawnRate);

        securityGuardSpawnRate = new CustomRoleOption(830, 831, CustomOptionType.Crewmate, ("Security Guard", SecurityGuard.color), 1);
        securityGuardCooldown = CustomOption.Create(832, CustomOptionType.Crewmate, "Security Guard Cooldown", 30f, 10f, 60f, 2.5f, securityGuardSpawnRate, unitType: UnitType.UnitSeconds);
        securityGuardTotalScrews = CustomOption.Create(833, CustomOptionType.Crewmate, "Security Guard Number Of Screws", 7f, 1f, 15f, 1f, securityGuardSpawnRate);
        securityGuardCamPrice = CustomOption.Create(834, CustomOptionType.Crewmate, "Number Of Screws Per Cam", 2f, 1f, 15f, 1f, securityGuardSpawnRate);
        securityGuardVentPrice = CustomOption.Create(835, CustomOptionType.Crewmate, "Number Of Screws Per Vent", 1f, 1f, 15f, 1f, securityGuardSpawnRate);
        securityGuardCamDuration = CustomOption.Create(836, CustomOptionType.Crewmate, "Security Guard Duration", 10f, 2.5f, 60f, 2.5f, securityGuardSpawnRate, unitType: UnitType.UnitSeconds);
        securityGuardCamMaxCharges = CustomOption.Create(837, CustomOptionType.Crewmate, "Gadget Max Charges", 5f, 1f, 30f, 1f, securityGuardSpawnRate);
        securityGuardCamRechargeTasksNumber = CustomOption.Create(838, CustomOptionType.Crewmate, "Number Of Tasks Needed For Recharging", 3f, 1f, 10f, 1f, securityGuardSpawnRate);
        securityGuardNoMove = CustomOption.Create(839, CustomOptionType.Crewmate, "Cant Move During Cam Duration", true, securityGuardSpawnRate);

        mediumSpawnRate = new CustomRoleOption(850, 851, CustomOptionType.Crewmate, ("Medium", Medium.color), 1);
        mediumCooldown = CustomOption.Create(852, CustomOptionType.Crewmate, "Medium Questioning Cooldown", 30f, 5f, 120f, 5f, mediumSpawnRate, unitType: UnitType.UnitSeconds);
        mediumDuration = CustomOption.Create(853, CustomOptionType.Crewmate, "Medium Questioning Duration", 3f, 0f, 15f, 1f, mediumSpawnRate, unitType: UnitType.UnitSeconds);
        mediumOneTimeUse = CustomOption.Create(854, CustomOptionType.Crewmate, "Each Soul Can Only Be Questioned Once", false, mediumSpawnRate);
        mediumChanceAdditionalInfo = CustomOption.Create(855, CustomOptionType.Crewmate, "Chance That The Answer Contains\nAdditional Information", rates, mediumSpawnRate);

        trapperSpawnRate = new CustomRoleOption(860, 861, CustomOptionType.Crewmate, ("Trapper", Trapper.color), 1);
        trapperCooldown = CustomOption.Create(862, CustomOptionType.Crewmate, "Trapper Cooldown", 30f, 5f, 120f, 5f, trapperSpawnRate, unitType: UnitType.UnitSeconds);
        trapperMaxCharges = CustomOption.Create(863, CustomOptionType.Crewmate, "Max Traps Charges", 5f, 1f, 15f, 1f, trapperSpawnRate);
        trapperRechargeTasksNumber = CustomOption.Create(864, CustomOptionType.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 15f, 1f, trapperSpawnRate);
        trapperTrapNeededTriggerToReveal = CustomOption.Create(865, CustomOptionType.Crewmate, "Trap Needed Trigger To Reveal", 3f, 2f, 10f, 1f, trapperSpawnRate);
        trapperAnonymousMap = CustomOption.Create(866, CustomOptionType.Crewmate, "Show Anonymous Map", false, trapperSpawnRate);
        trapperInfoType = CustomOption.Create(867, CustomOptionType.Crewmate, "Trap Information Type", ["Role", "Good/Evil Role", "Name"], trapperSpawnRate);
        trapperTrapDuration = CustomOption.Create(868, CustomOptionType.Crewmate, "Trap Duration", 5f, 1f, 15f, 1f, trapperSpawnRate, unitType: UnitType.UnitSeconds);

        #endregion

        #region Modifiers

        modifiersAreHidden = CustomOption.Create(1200, CustomOptionType.Modifier, ("VIP, Bait & Bloody Are Hidden", Color.yellow), true, null, true, ("Hide After Death Modifiers", Color.yellow));

        modifierBloody = new CustomRoleOption(1210, 1211, CustomOptionType.Modifier, ("Bloody", Color.yellow), 3);
        modifierBloodyDuration = CustomOption.Create(1212, CustomOptionType.Modifier, "Trail Duration", 10f, 3f, 60f, 1f, modifierBloody);

        modifierAntiTeleport = new CustomRoleOption(1220, 1221, CustomOptionType.Modifier, ("Anti Teleport", Color.yellow), 3);

        modifierTieBreaker = new CustomRoleOption(1230, 1231, CustomOptionType.Modifier, ("Tie Breaker", Color.yellow), 3);

        modifierBait = new CustomRoleOption(1240, 1241, CustomOptionType.Modifier, ("Bait", Color.yellow), 3);
        modifierBaitReportDelayMin = CustomOption.Create(1242, CustomOptionType.Modifier, "Bait Report Delay Min", 0f, 0f, 10f, 1f, modifierBait, unitType: UnitType.UnitSeconds);
        modifierBaitReportDelayMax = CustomOption.Create(1243, CustomOptionType.Modifier, "Bait Report Delay Max", 0f, 0f, 10f, 1f, modifierBait, unitType: UnitType.UnitSeconds);
        modifierBaitShowKillFlash = CustomOption.Create(1244, CustomOptionType.Modifier, "Warn The Killer With A Flash", true, modifierBait);

        modifierLover = new CustomRoleOption(1250, 1251, CustomOptionType.Modifier, ("Lovers", Color.yellow), 1);
        modifierLoverImpLoverRate = CustomOption.Create(1252, CustomOptionType.Modifier, "Chance That One Lover Is Impostor", rates, modifierLover);
        modifierLoverBothDie = CustomOption.Create(1253, CustomOptionType.Modifier, "Both Lovers Die", true, modifierLover);
        modifierLoverEnableChat = CustomOption.Create(1254, CustomOptionType.Modifier, "Enable Lover Chat", true, modifierLover);

        modifierSunglasses = new CustomRoleOption(1260, 1261, CustomOptionType.Modifier, ("Sunglasses", Color.yellow), 3);
        modifierSunglassesVision = CustomOption.Create(1262, CustomOptionType.Modifier, "Vision With Sunglasses", ["-10%", "-20%", "-30%", "-40%", "-50%"], modifierSunglasses);

        modifierMini = new CustomRoleOption(1270, 1271, CustomOptionType.Modifier, ("Mini", Color.yellow), 3);
        modifierMiniGrowingUpDuration = CustomOption.Create(1272, CustomOptionType.Modifier, "Mini Growing Up Duration", 400f, 100f, 1500f, 100f, modifierMini);
        modifierMiniGrowingUpInMeeting = CustomOption.Create(1273, CustomOptionType.Modifier, "Mini Grows Up In Meeting", true, modifierMini);
        if (Utilities.EventUtility.canBeEnabled || Utilities.EventUtility.isEnabled)
        {
            eventKicksPerRound = CustomOption.Create(1274, CustomOptionType.Modifier, ("Maximum Kicks Mini Suffers", Color.green), 4f, 0f, 14f, 1f, modifierMini);
            eventHeavyAge = CustomOption.Create(1275, CustomOptionType.Modifier, ("Age At Which Mini Is Heavy", Color.green), 12f, 6f, 18f, 0.5f, modifierMini);
            eventReallyNoMini = CustomOption.Create(1276, CustomOptionType.Modifier, ("Really No Mini :(", Color.green), false, modifierMini);
        }

        modifierVip = new CustomRoleOption(1280, 1281, CustomOptionType.Modifier, ("VIP", Color.yellow), 3);
        modifierVipShowColor = CustomOption.Create(1282, CustomOptionType.Modifier, "Show Team Color", true, modifierVip);

        modifierInvert = new CustomRoleOption(1290, 1291, CustomOptionType.Modifier, ("Invert", Color.yellow), 3);
        modifierInvertDuration = CustomOption.Create(1292, CustomOptionType.Modifier, "Number Of Meetings Inverted", 3f, 1f, 15f, 1f, modifierInvert, unitType: UnitType.UnitSeconds);

        modifierChameleon = new CustomRoleOption(1300, 1301, CustomOptionType.Modifier, ("Chameleon", Color.yellow), 3);
        modifierChameleonHoldDuration = CustomOption.Create(1302, CustomOptionType.Modifier, "Time Until Fading Starts", 3f, 1f, 10f, 0.5f, modifierChameleon, unitType: UnitType.UnitSeconds);
        modifierChameleonFadeDuration = CustomOption.Create(1303, CustomOptionType.Modifier, "Fade Duration", 1f, 0.25f, 10f, 0.25f, modifierChameleon, unitType: UnitType.UnitSeconds);
        modifierChameleonMinVisibility = CustomOption.Create(1304, CustomOptionType.Modifier, "Minimum Visibility", ["0%", "10%", "20%", "30%", "40%", "50%"], modifierChameleon);

        modifierArmored = new CustomRoleOption(1310, 1311, CustomOptionType.Modifier, ("Armored", Color.yellow), 1);

        modifierShifter = new CustomRoleOption(1320, 1321, CustomOptionType.Modifier, ("Shifter", Color.yellow), 1);
        modifierShifterShiftsMedicShield = CustomOption.Create(1322, CustomOptionType.Modifier, "Can Shift Medic Shield", false, modifierShifter);

        #endregion

        blockedRolePairings.Add((byte)RoleId.Vampire, [(byte)RoleId.Warlock]);
        blockedRolePairings.Add((byte)RoleId.Warlock, [(byte)RoleId.Vampire]);
        blockedRolePairings.Add((byte)RoleId.Spy, [(byte)RoleId.Mini]);
        blockedRolePairings.Add((byte)RoleId.Mini, [(byte)RoleId.Spy]);
        blockedRolePairings.Add((byte)RoleId.Vulture, [(byte)RoleId.Cleaner]);
        blockedRolePairings.Add((byte)RoleId.Cleaner, [(byte)RoleId.Vulture]);
    }
}