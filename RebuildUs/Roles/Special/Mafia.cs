using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

public static class Mafia
{
    public static Color Color = Palette.ImpostorRed;

    private static CustomButton janitorCleanButton;

    public static void MakeButtons(HudManager __instance)
    {
        janitorCleanButton = new CustomButton(
            () =>
            {
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                {
                    if (collider2D.tag == "DeadBody")
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (component && !component.Reported)
                        {
                            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                            Vector2 truePosition2 = component.TruePosition;
                            if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                            {
                                NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                using var writer = RPCProcedure.SendRPC(CustomRPC.CleanBody);
                                writer.Write(playerInfo.PlayerId);
                                writer.Write(Janitor.janitor.PlayerId);
                                RPCProcedure.cleanBody(playerInfo.PlayerId, Janitor.janitor.PlayerId);
                                janitorCleanButton.Timer = janitorCleanButton.MaxTimer;
                                break;
                            }
                        }
                    }
                }
            },
            () => { return Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
            () => { janitorCleanButton.Timer = janitorCleanButton.MaxTimer; },
            Janitor.getButtonSprite(),
            ButtonOffset.UpperLeft,
            __instance,
            __instance.UseButton,
            KeyCode.F
        );
    }

    public static void SetButtonCooldowns()
    {
        janitorCleanButton.MaxTimer = Janitor.cooldown;
    }

    public static class Godfather
    {
        public static PlayerControl godfather;

        public static void clearAndReload()
        {
            godfather = null;
        }
    }

    public static class Mafioso
    {
        public static PlayerControl mafioso;

        public static void clearAndReload()
        {
            mafioso = null;
        }
    }

    public static class Janitor
    {
        public static PlayerControl janitor;

        public static float cooldown = 30f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.CleanButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload()
        {
            janitor = null;
            cooldown = CustomOptionHolder.janitorCooldown.getFloat();
        }
    }
}