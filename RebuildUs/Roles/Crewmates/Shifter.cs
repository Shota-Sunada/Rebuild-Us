using System.Collections.Generic;
using UnityEngine;

namespace RebuildUs.Roles;

public static class Shifter
{
    public static PlayerControl shifter;
    public static List<int> pastShifters = [];
    public static Color Color = new Color32(102, 102, 102, byte.MaxValue);

    public static PlayerControl futureShift;
    public static PlayerControl currentTarget;
    public static bool shiftModifiers = false;
    public static bool shiftsMedicShield = false;

    public static bool isNeutral = false;
    public static bool shiftPastShifters = false;

    public static void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
    {
        if (futureShift == player) futureShift = null;
    }

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.ShiftButton.png", 115f);
        return buttonSprite;
    }

    public static void clearAndReload()
    {
        shifter = null;
        pastShifters = new List<int>();
        currentTarget = null;
        futureShift = null;
        shiftModifiers = CustomOptionHolder.shifterShiftsModifiers.getBool();
        shiftPastShifters = CustomOptionHolder.shifterPastShifters.getBool();
        shiftsMedicShield = CustomOptionHolder.shifterShiftsMedicShield.getBool();
        isNeutral = false;
    }
}