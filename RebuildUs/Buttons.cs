using AmongUs.GameOptions;
using RebuildUs.Roles.RoleBase;

namespace RebuildUs;

internal static class Buttons
{
    private static bool initialized = false;

    internal static void SetCustomButtonCooldowns()
    {
        if (!initialized)
        {
            try
            {
                CreateButtons(HudManager.Instance);
            }
            catch
            {
                Plugin.Instance.Logger.LogWarning("Buttons cooldown was not set, either the gamemode does not require them or there is something wrong.");
                return;
            }
        }

        RoleHelpers.SetButtonCooldowns();
    }

    internal static void CreateButtons(HudManager __instance)
    {
        initialized = false;
        RoleHelpers.MakeButtons(__instance);
        initialized = true;
        SetCustomButtonCooldowns();
    }
}