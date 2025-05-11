using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class SabotageButtonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.Refresh))]
    public static void RefreshPostfix()
    {
        // Mafia disable sabotage button for Janitor and sometimes for Mafioso
        var blockSabotageJanitor = Mafia.Janitor.janitor != null && PlayerControl.LocalPlayer.isRole(RoleId.Janitor);
        var blockSabotageMafioso = Mafia.Mafioso.mafioso != null && PlayerControl.LocalPlayer.isRole(RoleId.Mafioso) && Mafia.Godfather.godfather != null && !Mafia.Godfather.godfather.isDead();
        if (blockSabotageJanitor || blockSabotageMafioso)
        {
            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.SetDisabled();
        }
    }
}