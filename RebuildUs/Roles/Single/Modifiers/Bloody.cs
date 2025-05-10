using System.Collections.Generic;

namespace RebuildUs.Roles;

public static class Bloody
{
    public static List<PlayerControl> bloody = [];
    public static Dictionary<byte, float> active = [];
    public static Dictionary<byte, byte> bloodyKillerMap = [];

    public static float duration = 5f;

    public static void clearAndReload()
    {
        bloody = [];
        active = [];
        bloodyKillerMap = [];
        duration = CustomOptionHolder.modifierBloodyDuration.getFloat();
    }
}