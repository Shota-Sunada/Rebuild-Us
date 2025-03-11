using RebuildUs.Modules;

namespace RebuildUs;

internal static class CustomOptionHolders
{
    internal static CustomOption CrewmateRolesCountMin;
    internal static CustomOption CrewmateRolesCountMax;
    internal static CustomOption CrewmateRolesFill;
    internal static CustomOption NeutralRolesCountMin;
    internal static CustomOption NeutralRolesCountMax;
    internal static CustomOption ImpostorRolesCountMin;
    internal static CustomOption ImpostorRolesCountMax;
    internal static CustomOption ModifiersCountMin;
    internal static CustomOption ModifiersCountMax;

    internal static void Initialize()
    {
        CrewmateRolesCountMin = CustomOption.Create(10, CustomOptionType.General, "CrewmateRolesCountMin", 0f, 0f, 15f, 1f, null, true, "Min/Max Roles");
        CrewmateRolesCountMax = CustomOption.Create(11, CustomOptionType.General, "CrewmateRolesCountMax", 0f, 0f, 15f, 1f);
        CrewmateRolesFill = CustomOption.Create(12, CustomOptionType.General, "CrewmateRolesFill", 0f, 0f, 15f, 1f);
        NeutralRolesCountMin = CustomOption.Create(13, CustomOptionType.General, "NeutralRolesCountMin", 0f, 0f, 15f, 1f);
        NeutralRolesCountMax = CustomOption.Create(14, CustomOptionType.General, "NeutralRolesCountMax", 0f, 0f, 15f, 1f);
        ImpostorRolesCountMin = CustomOption.Create(15, CustomOptionType.General, "ImpostorRolesCountMin", 0f, 0f, 15f, 1f);
        ImpostorRolesCountMax = CustomOption.Create(16, CustomOptionType.General, "ImpostorRolesCountMax", 0f, 0f, 15f, 1f);
        ModifiersCountMin = CustomOption.Create(17, CustomOptionType.General, "ModifiersCountMin", 0f, 0f, 15f, 1f);
        ModifiersCountMax = CustomOption.Create(18, CustomOptionType.General, "ModifiersCountMax", false);
    }
}