using RebuildUs.Modules;

namespace RebuildUs;

public static class MapOptions
{
    public static CustomGameMode GameMode = CustomGameMode.Classic;

    // OPTIONS
    public static int MaxNumberOfMeetings = 10;
    public static bool BlockSkippingInMeetings = false;
    public static bool NoVoteIsSelfVote = false;
    public static bool HidePlayerNames = false;
    public static bool ShowGameResult = true;
    public static bool AllowParallelMedBayScans = false;

    // UPDATING VALUES
    public static int CurrentMeetingCount = 0;

    public static void ClearAndReload()
    {
        CurrentMeetingCount = 0;

        MaxNumberOfMeetings = CustomOptionHolders.MaxNumberOfMeetings.GetSelection();
        BlockSkippingInMeetings = CustomOptionHolders.BlockSkippingInMeetings.GetBool();
        NoVoteIsSelfVote = CustomOptionHolders.NoVoteIsSelfVote.GetBool();
        HidePlayerNames = CustomOptionHolders.HidePlayerNames.GetBool();
        AllowParallelMedBayScans = CustomOptionHolders.AllowParallelMedBayScans.GetBool();
    }
}