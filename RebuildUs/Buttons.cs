using RebuildUs.Roles;

namespace RebuildUs;

public static class Buttons
{
    private static bool initialized = false;

    public static void SetCustomButtonCooldowns()
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

        RoleData.SetButtonCooldowns();
    }

    public static void CreateButtons(HudManager __instance)
    {
        initialized = false;
        RoleData.MakeButtons(__instance);
        initialized = true;
        SetCustomButtonCooldowns();
    }
}