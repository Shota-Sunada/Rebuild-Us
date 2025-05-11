using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class EmergencyMinigamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    public static void UpdatePostfix(EmergencyMinigame __instance)
    {
        var roleCanCallEmergency = true;
        var statusText = "";

        // Deactivate emergency button for Swapper
        if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer && !Swapper.canCallEmergency)
        {
            roleCanCallEmergency = false;
            statusText = "The Swapper can't start an emergency meeting";
        }
        // Potentially deactivate emergency button for Jester
        if (Jester.exists && PlayerControl.LocalPlayer.isRole(RoleId.Jester) && !Jester.canCallEmergency)
        {
            roleCanCallEmergency = false;
            statusText = "The Jester can't start an emergency meeting";
        }
        // Potentially deactivate emergency button for Lawyer/Prosecutor
        if (Lawyer.lawyer != null && Lawyer.lawyer == PlayerControl.LocalPlayer && !Lawyer.canCallEmergency)
        {
            roleCanCallEmergency = false;
            statusText = "The Lawyer can't start an emergency meeting";
            if (Lawyer.isProsecutor) statusText = "The Prosecutor can't start an emergency meeting";
        }

        if (!roleCanCallEmergency)
        {
            __instance.StatusText.text = statusText;
            __instance.NumberText.text = string.Empty;
            __instance.ClosedLid.gameObject.SetActive(true);
            __instance.OpenLid.gameObject.SetActive(false);
            __instance.ButtonActive = false;
            return;
        }

        // Handle max number of meetings
        if (__instance.state == 1)
        {
            int localRemaining = PlayerControl.LocalPlayer.RemainingEmergencies;
            int teamRemaining = Mathf.Max(0, MapOptions.maxNumberOfMeetings - MapOptions.meetingsCount);
            int remaining = Mathf.Min(localRemaining, (Mayor.exists && PlayerControl.LocalPlayer.isRole(RoleId.Mayor)) ? 1 : teamRemaining);
            __instance.NumberText.text = $"{localRemaining.ToString()} and the ship has {teamRemaining.ToString()}";
            __instance.ButtonActive = remaining > 0;
            __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
            __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
            return;
        }
    }
}