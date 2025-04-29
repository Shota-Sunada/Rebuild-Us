using System.Collections.Generic;
using RebuildUs.Localization;
using RebuildUs.Modules;
using RebuildUs.Roles;
using UnityEngine;

namespace RebuildUs;

public static class CustomOptionHolders
{
    public static readonly string[] Percents = ["0", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100"];
    public static readonly string[] Presets = ["Preset 1", "Preset 2", "Random Preset Skeld", "Random Preset Mira HQ", "Random Preset Polus", "Random Preset Airship", "Random Preset Submerged"];

    public static CustomOption PresetSelection;

    public static CustomOption CrewmateRolesCountMin;
    public static CustomOption CrewmateRolesCountMax;
    public static CustomOption CrewmateRolesFill;
    public static CustomOption NeutralRolesCountMin;
    public static CustomOption NeutralRolesCountMax;
    public static CustomOption ImpostorRolesCountMin;
    public static CustomOption ImpostorRolesCountMax;
    public static CustomOption ModifiersCountMin;
    public static CustomOption ModifiersCountMax;

    public static CustomOption MaxNumberOfMeetings;
    public static CustomOption BlockSkippingInMeetings;
    public static CustomOption NoVoteIsSelfVote;
    public static CustomOption HidePlayerNames;
    public static CustomOption AllowParallelMedBayScans;
    public static CustomOption RefundVotesOnDeath;

    public static CustomRoleOption SheriffSpawnRate;
    public static CustomOption SheriffKillCooldown;
    public static CustomOption SheriffMaxShots;
    public static CustomOption SheriffCanKillNeutrals;
    public static CustomOption SheriffKillTargetOnMisfire;
    public static CustomOption SheriffCanKillMadmate;

    public static Dictionary<RoleId, RoleId[]> BlockedRolePairings = [];

    public static void Initialize()
    {
        CustomOption.VanillaSettings = Plugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

        PresetSelection = CustomOption.Create(0, CustomOptionType.General, ("SettingPreset", null), Presets, null, true);

        CrewmateRolesCountMin = CustomOption.Create(10, CustomOptionType.General, ("CrewmateRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, null, true, ("MinMaxRoles", null), UnitType.UnitPlayers);
        CrewmateRolesCountMax = CustomOption.Create(11, CustomOptionType.General, ("CrewmateRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        CrewmateRolesFill = CustomOption.Create(12, CustomOptionType.General, ("CrewmateRolesFill", new Color32(204, 204, 0, 255)), false);
        NeutralRolesCountMin = CustomOption.Create(13, CustomOptionType.General, ("NeutralRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        NeutralRolesCountMax = CustomOption.Create(14, CustomOptionType.General, ("NeutralRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        ImpostorRolesCountMin = CustomOption.Create(15, CustomOptionType.General, ("ImpostorRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        ImpostorRolesCountMax = CustomOption.Create(16, CustomOptionType.General, ("ImpostorRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        ModifiersCountMin = CustomOption.Create(17, CustomOptionType.General, ("ModifiersCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);
        ModifiersCountMax = CustomOption.Create(18, CustomOptionType.General, ("ModifiersCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, unitType: UnitType.UnitPlayers);

        MaxNumberOfMeetings = CustomOption.Create(20, CustomOptionType.General, "MaxNumberOfMeetings", 10f, 0f, 15f, 1f, isHeader: true, header: "GamePlaySettings", unitType: UnitType.UnitTimes);
        BlockSkippingInMeetings = CustomOption.Create(21, CustomOptionType.General, "BlockSkippingInMeetings", false);
        NoVoteIsSelfVote = CustomOption.Create(22, CustomOptionType.General, "NoVoteIsSelfVote", false, BlockSkippingInMeetings);
        HidePlayerNames = CustomOption.Create(23, CustomOptionType.General, "HidePlayerNames", false);
        AllowParallelMedBayScans = CustomOption.Create(24, CustomOptionType.General, "AllowParallelMedBayScans", false);
        RefundVotesOnDeath = CustomOption.Create(25, CustomOptionType.General, "RefundVotesOnDeath", false);

        SheriffSpawnRate = new CustomRoleOption(50, 51, CustomOptionType.Crewmate, ("Sheriff", Colors.SheriffYellow));
        SheriffKillCooldown = CustomOption.Create(52, CustomOptionType.Crewmate, "KillCooldown", 30f, 15f, 60f, 2.5f, SheriffSpawnRate, unitType: UnitType.UnitSeconds);
        SheriffMaxShots = CustomOption.Create(53, CustomOptionType.Crewmate, "SheriffMaxShots", 1f, 1f, 15f, 1f, SheriffSpawnRate, unitType: UnitType.UnitShots);
        SheriffCanKillNeutrals = CustomOption.Create(54, CustomOptionType.Crewmate, "SheriffCanKillNeutrals", true, SheriffSpawnRate);
        SheriffKillTargetOnMisfire = CustomOption.Create(55, CustomOptionType.Crewmate, "SheriffKillTargetOnMisfire", false, SheriffSpawnRate);
        SheriffCanKillMadmate = CustomOption.Create(56, CustomOptionType.Crewmate, "SheriffCanKillMadmate", true, SheriffSpawnRate);
    }
}