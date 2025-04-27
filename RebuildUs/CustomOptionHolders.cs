using RebuildUs.Localization;
using RebuildUs.Modules;
using UnityEngine;

namespace RebuildUs;

internal static class CustomOptionHolders
{
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

    internal static CustomOption SheriffSpawnRate;
    internal static CustomOption SheriffKillCooldown;
    internal static CustomOption SheriffMaxShots;
    internal static CustomOption SheriffCanKillNeutrals;
    internal static CustomOption SheriffKillTargetOnMisfire;
    internal static CustomOption SheriffCanKillMadmate;

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

        MaxNumberOfMeetings = CustomOption.Create(20, CustomOptionType.General, ("MaxNumberOfMeetings", null), 10f, 0f, 15f, 1f, isHeader: true, header: ("GamePlaySettings", null));
        BlockSkippingInMeetings = CustomOption.Create(21, CustomOptionType.General, ("BlockSkippingInMeetings", null), false);
        NoVoteIsSelfVote = CustomOption.Create(22, CustomOptionType.General, ("NoVoteIsSelfVote", null), false, BlockSkippingInMeetings);
        HidePlayerNames = CustomOption.Create(23, CustomOptionType.General, ("HidePlayerNames", null), false);
        AllowParallelMedBayScans = CustomOption.Create(24, CustomOptionType.General, ("AllowParallelMedBayScans", null), false);
        RefundVotesOnDeath = CustomOption.Create(25, CustomOptionType.General, ("RefundVotesOnDeath", null), false);
    }
}