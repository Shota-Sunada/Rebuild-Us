namespace RebuildUs.Helpers;

internal static class PlayerHelpers
{
    public static bool IsDead(this PlayerControl player)
    {
        return
            player == null ||
            player.Data.IsDead ||
            player.Data.Disconnected;
    }

    internal static bool IsAlive(this PlayerControl player)
    {
        return !IsDead(player);
    }
}