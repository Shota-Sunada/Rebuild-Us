using System.Collections.Generic;
using UnityEngine;

namespace RebuildUs;

static class MapOptions
{
    // Set values
    public static int maxNumberOfMeetings = 10;
    public static bool blockSkippingInEmergencyMeetings = false;
    public static bool noVoteIsSelfVote = false;
    public static bool hidePlayerNames = false;
    public static bool ghostsSeeRoles = true;
    public static bool ghostsSeeModifier = true;
    public static bool ghostsSeeInformation = true;
    public static bool ghostsSeeVotes = true;
    public static bool showRoleSummary = true;
    public static bool allowParallelMedBayScans = false;
    public static bool showLighterDarker = true;
    public static bool enableHorseMode = false;
    public static bool shieldFirstKill = false;
    public static bool ShowVentsOnMap = true;
    public static bool ShowChatNotifications = true;
    public static CustomGamemodes gameMode = CustomGamemodes.Classic;

    // Updating values
    public static int meetingsCount = 0;
    public static List<SurvCamera> camerasToAdd = [];
    public static List<Vent> ventsToSeal = [];
    public static Dictionary<byte, PoolablePlayer> playerIcons = [];
    public static string firstKillName;
    public static PlayerControl firstKillPlayer;

    public static void clearAndReloadMapOptions()
    {
        meetingsCount = 0;
        camerasToAdd = [];
        ventsToSeal = [];
        playerIcons = []; ;

        maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
        blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
        noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
        hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
        allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
        shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
        firstKillPlayer = null;
    }

    public static void reloadPluginOptions()
    {
        ghostsSeeRoles = RebuildUs.GhostsSeeRoles.Value;
        ghostsSeeModifier = RebuildUs.GhostsSeeModifier.Value;
        ghostsSeeInformation = RebuildUs.GhostsSeeInformation.Value;
        ghostsSeeVotes = RebuildUs.GhostsSeeVotes.Value;
        showRoleSummary = RebuildUs.ShowRoleSummary.Value;
        showLighterDarker = RebuildUs.ShowLighterDarker.Value;
        enableHorseMode = RebuildUs.EnableHorseMode.Value;
        ShowVentsOnMap = RebuildUs.ShowVentsOnMap.Value;
        ShowChatNotifications = RebuildUs.ShowChatNotifications.Value;

        //Patches.ShouldAlwaysHorseAround.isHorseMode = RebuildUsPlugin.EnableHorseMode.Value;
    }
}