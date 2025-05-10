using System.Collections.Generic;

namespace RebuildUs.Roles;

public static class Vip
{
    public static List<PlayerControl> vip = [];
    public static bool showColor = true;

    public static void clearAndReload()
    {
        vip = [];
        showColor = CustomOptionHolder.modifierVipShowColor.getBool();
    }
}