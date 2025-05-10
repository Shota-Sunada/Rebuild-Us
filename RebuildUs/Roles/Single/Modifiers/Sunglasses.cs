using System.Collections.Generic;

namespace RebuildUs.Roles;

public static class Sunglasses
{
    public static List<PlayerControl> sunglasses = [];
    public static int vision = 1;

    public static void clearAndReload()
    {
        sunglasses = [];
        vision = CustomOptionHolder.modifierSunglassesVision.getSelection() + 1;
    }
}