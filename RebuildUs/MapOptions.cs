using RebuildUs.Modules;

namespace RebuildUs;

internal static class MapOptions
{
    internal static CustomGameMode GameMode = CustomGameMode.Classic;

    // OPTIONS
    internal static int MaxNumberOfMeetings = 10;
    internal static bool BlockSkippingInMeetings = false;
    internal static bool NoVoteIsSelfVote = false;
    internal static bool HidePlayerNames = false;
    internal static bool ShowGameOverview = true;
    internal static bool AllowParallelMedBayScans = false;

    // UPDATING VALUES
    internal static int CurrentMeetingCount = 0;

    internal static void ClearAndReload()
    {
        CurrentMeetingCount = 0;

        MaxNumberOfMeetings = CustomOptionHolders.MaxNumberOfMeetings.GetSelection();
        BlockSkippingInMeetings = CustomOptionHolders.BlockSkippingInMeetings.GetBool();
        NoVoteIsSelfVote = CustomOptionHolders.NoVoteIsSelfVote.GetBool();
        HidePlayerNames = CustomOptionHolders.HidePlayerNames.GetBool();
        AllowParallelMedBayScans = CustomOptionHolders.AllowParallelMedBayScans.GetBool();
    }
}