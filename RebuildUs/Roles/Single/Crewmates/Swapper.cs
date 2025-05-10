using UnityEngine;

namespace RebuildUs.Roles;

public static class Swapper
{
    public static PlayerControl swapper;
    public static Color Color = new Color32(134, 55, 86, byte.MaxValue);
    private static Sprite spriteCheck;
    public static bool canCallEmergency = false;
    public static bool canOnlySwapOthers = false;
    public static int numSwaps = 2;

    public static byte playerId1 = byte.MaxValue;
    public static byte playerId2 = byte.MaxValue;

    public static Sprite getCheckSprite()
    {
        if (spriteCheck) return spriteCheck;
        spriteCheck = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SwapperCheck.png", 150f);
        return spriteCheck;
    }

    public static void clearAndReload()
    {
        swapper = null;
        playerId1 = byte.MaxValue;
        playerId2 = byte.MaxValue;
        canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
        canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
        numSwaps = Mathf.RoundToInt(CustomOptionHolder.swapperNumSwaps.getFloat());
    }
}