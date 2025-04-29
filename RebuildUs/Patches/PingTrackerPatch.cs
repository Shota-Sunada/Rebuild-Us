using HarmonyLib;
using TMPro;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class PingTrackerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static void Postfix(PingTracker __instance)
    {
        __instance.text.alignment = TextAlignmentOptions.TopRight;
        var position = __instance.GetComponent<AspectPosition>();
        position.Alignment = AspectPosition.EdgeAlignments.Top;

        if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
        {
            __instance.text.text = $"{Plugin.MOD_NAME} v{Plugin.MOD_VERSION}\n{__instance.text.text}";
            position.DistanceFromEdge = MeetingHud.Instance ? new(1.25f, 0f, 0) : new(1.55f, 0f, 0);
        }
        else
        {
            __instance.text.text = $"{Plugin.MOD_NAME} v{Plugin.MOD_VERSION}\n<size=50%>By {Plugin.MOD_DEVELOPER}</size>\n{__instance.text.text}";
            position.DistanceFromEdge = new(2.7f, 0.15f, 0);
        }

        position.AdjustPosition();
    }
}