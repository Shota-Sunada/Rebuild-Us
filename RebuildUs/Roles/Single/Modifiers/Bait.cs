using System.Collections.Generic;
using UnityEngine;

namespace RebuildUs.Roles;

public static class Bait
{
    public static List<PlayerControl> bait = [];
    public static Dictionary<DeadPlayer, float> active = [];
    public static Color color = new Color32(0, 247, 255, byte.MaxValue);

    public static float reportDelayMin = 0f;
    public static float reportDelayMax = 0f;
    public static bool showKillFlash = true;

    public static void clearAndReload()
    {
        bait = [];
        active = [];
        reportDelayMin = CustomOptionHolder.modifierBaitReportDelayMin.getFloat();
        reportDelayMax = CustomOptionHolder.modifierBaitReportDelayMax.getFloat();
        if (reportDelayMin > reportDelayMax) reportDelayMin = reportDelayMax;
        showKillFlash = CustomOptionHolder.modifierBaitShowKillFlash.getBool();
    }
}