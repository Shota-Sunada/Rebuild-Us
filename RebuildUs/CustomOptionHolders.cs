using System.Collections.Generic;
using RebuildUs.Localization;
using RebuildUs.Modules;
using RebuildUs.Roles;
using UnityEngine;

namespace RebuildUs;

internal static class CustomOptionHolders
{
    public static string[] Rates = ["0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"];
    internal static string[] Presets = ["Preset 1", "Preset 2", "Random Preset Skeld", "Random Preset Mira HQ", "Random Preset Polus", "Random Preset Airship", "Random Preset Submerged"];

    internal static CustomOption PresetSelection;

    internal static CustomOption CrewmateRolesCountMin;
    internal static CustomOption CrewmateRolesCountMax;
    internal static CustomOption CrewmateRolesFill;
    internal static CustomOption NeutralRolesCountMin;
    internal static CustomOption NeutralRolesCountMax;
    internal static CustomOption ImpostorRolesCountMin;
    internal static CustomOption ImpostorRolesCountMax;
    internal static CustomOption ModifiersCountMin;
    internal static CustomOption ModifiersCountMax;

    internal static CustomOption MaxNumberOfMeetings;
    internal static CustomOption BlockSkippingInMeetings;
    internal static CustomOption NoVoteIsSelfVote;
    internal static CustomOption HidePlayerNames;
    internal static CustomOption AllowParallelMedBayScans;
    internal static CustomOption RefundVotesOnDeath;

    internal static CustomRoleOption SheriffSpawnRate;
    internal static CustomOption SheriffKillCooldown;
    internal static CustomOption SheriffMaxShots;
    internal static CustomOption SheriffCanKillNeutrals;
    internal static CustomOption SheriffKillTargetOnMisfire;
    internal static CustomOption SheriffCanKillMadmate;

    internal static Dictionary<RoleId, RoleId[]> BlockedRolePairings = [];

    internal static void Initialize()
    {
        CustomOption.VanillaSettings = Plugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

        PresetSelection = CustomOption.Create(0, CustomOptionType.General, ("SettingPreset", null), Presets, null, true);

        CrewmateRolesCountMin = CustomOption.Create(10, CustomOptionType.General, ("CrewmateRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f, null, true, ("MinMaxRoles", null));
        CrewmateRolesCountMax = CustomOption.Create(11, CustomOptionType.General, ("CrewmateRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f);
        CrewmateRolesFill = CustomOption.Create(12, CustomOptionType.General, ("CrewmateRolesFill", new Color32(204, 204, 0, 255)), false);
        NeutralRolesCountMin = CustomOption.Create(13, CustomOptionType.General, ("NeutralRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f);
        NeutralRolesCountMax = CustomOption.Create(14, CustomOptionType.General, ("NeutralRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f);
        ImpostorRolesCountMin = CustomOption.Create(15, CustomOptionType.General, ("ImpostorRolesCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f);
        ImpostorRolesCountMax = CustomOption.Create(16, CustomOptionType.General, ("ImpostorRolesCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f);
        ModifiersCountMin = CustomOption.Create(17, CustomOptionType.General, ("ModifiersCountMin", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f);
        ModifiersCountMax = CustomOption.Create(18, CustomOptionType.General, ("ModifiersCountMax", new Color32(204, 204, 0, 255)), 0f, 0f, 15f, 1f);

        MaxNumberOfMeetings = CustomOption.Create(20, CustomOptionType.General, "MaxNumberOfMeetings", 10f, 0f, 15f, 1f, isHeader: true, header: "GamePlaySettings");
        BlockSkippingInMeetings = CustomOption.Create(21, CustomOptionType.General, "BlockSkippingInMeetings", false);
        NoVoteIsSelfVote = CustomOption.Create(22, CustomOptionType.General, "NoVoteIsSelfVote", false, BlockSkippingInMeetings);
        HidePlayerNames = CustomOption.Create(23, CustomOptionType.General, "HidePlayerNames", false);
        AllowParallelMedBayScans = CustomOption.Create(24, CustomOptionType.General, "AllowParallelMedBayScans", false);
        RefundVotesOnDeath = CustomOption.Create(25, CustomOptionType.General, "RefundVotesOnDeath", false);

        SheriffSpawnRate = new CustomRoleOption(50, 51, CustomOptionType.Crewmate, ("Sheriff", RolesManager.AllRoles[RoleId.Sheriff].Color));
        SheriffKillCooldown = CustomOption.Create(52, CustomOptionType.Crewmate, "SheriffKillCooldown", 30f, 15f, 60f, 2.5f, SheriffSpawnRate);
        SheriffMaxShots = CustomOption.Create(53, CustomOptionType.Crewmate, "SheriffMaxShots", 1f, 1f, 15f, 1f, SheriffSpawnRate);
        SheriffCanKillNeutrals = CustomOption.Create(54, CustomOptionType.Crewmate, "SheriffCanKillNeutrals", true, SheriffSpawnRate);
        SheriffKillTargetOnMisfire = CustomOption.Create(55, CustomOptionType.Crewmate, "SheriffKillTargetOnMisfire", false, SheriffSpawnRate);
        SheriffCanKillMadmate = CustomOption.Create(56, CustomOptionType.Crewmate, "SheriffCanKillMadmate", true, SheriffSpawnRate);
    }
}