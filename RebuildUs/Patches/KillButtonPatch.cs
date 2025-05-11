using HarmonyLib;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class KillButtonPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public static bool DoClickPrefix(KillButton __instance)
    {
        if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove)
        {
            // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
            var res = Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget);
            // Handle blank kill
            if (res == MurderAttemptResult.BlankKill)
            {
                PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                if (PlayerControl.LocalPlayer == Cleaner.cleaner)
                {
                    Cleaner.cleaner.killTimer = HudManagerStartPatch.cleanerCleanButton.Timer = HudManagerStartPatch.cleanerCleanButton.MaxTimer;
                }
                else if (PlayerControl.LocalPlayer == Warlock.warlock)
                {
                    Warlock.warlock.killTimer = HudManagerStartPatch.warlockCurseButton.Timer = HudManagerStartPatch.warlockCurseButton.MaxTimer;
                }
                else if (PlayerControl.LocalPlayer == Mini.mini && Mini.mini.Data.Role.IsImpostor)
                {
                    Mini.mini.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * (Mini.isGrownUp() ? 0.66f : 2f));
                }
                else if (PlayerControl.LocalPlayer == Witch.witch)
                {
                    Witch.witch.killTimer = HudManagerStartPatch.witchSpellButton.Timer = HudManagerStartPatch.witchSpellButton.MaxTimer;
                }
                else if (PlayerControl.LocalPlayer == Ninja.ninja)
                {
                    Ninja.ninja.killTimer = HudManagerStartPatch.ninjaButton.Timer = HudManagerStartPatch.ninjaButton.MaxTimer;
                }
            }
            __instance.SetTarget(null);
        }
        return false;
    }
}