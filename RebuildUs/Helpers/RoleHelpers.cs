namespace RebuildUs.Helpers;

internal static class RoleHelpers
{
    internal static bool IsCrewmate(this PlayerControl player)
    {
        return player != null && !player.IsImpostor() && !player.IsNeutral();
    }

    internal static bool IsImpostor(this PlayerControl player)
    {
        return player != null && player.Data.Role.IsImpostor;
    }

    internal static bool IsNeutral(this PlayerControl player)
    {
        return false;
    }
}