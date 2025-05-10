using System.Collections.Generic;

namespace RebuildUs.Roles;

public static class Invert
{
    public static List<PlayerControl> invert = [];
    public static int meetings = 3;

    public static void clearAndReload()
    {
        invert = [];
        meetings = (int)CustomOptionHolder.modifierInvertDuration.getFloat();
    }
}