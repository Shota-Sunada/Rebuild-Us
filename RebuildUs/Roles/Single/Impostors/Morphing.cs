using UnityEngine;

namespace RebuildUs.Roles;

public static class Morphing
{
    public static PlayerControl morphing;
    public static Color color = Palette.ImpostorRed;
    private static Sprite sampleSprite;
    private static Sprite morphSprite;

    public static float cooldown = 30f;
    public static float duration = 10f;

    public static PlayerControl currentTarget;
    public static PlayerControl sampledTarget;
    public static PlayerControl morphTarget;
    public static float morphTimer = 0f;

    public static void resetMorph()
    {
        morphTarget = null;
        morphTimer = 0f;
        if (morphing == null) return;
        morphing.setDefaultLook();
    }

    public static void clearAndReload()
    {
        resetMorph();
        morphing = null;
        currentTarget = null;
        sampledTarget = null;
        morphTarget = null;
        morphTimer = 0f;
        cooldown = CustomOptionHolder.morphingCooldown.getFloat();
        duration = CustomOptionHolder.morphingDuration.getFloat();
    }

    public static Sprite getSampleSprite()
    {
        if (sampleSprite) return sampleSprite;
        sampleSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.SampleButton.png", 115f);
        return sampleSprite;
    }

    public static Sprite getMorphSprite()
    {
        if (morphSprite) return morphSprite;
        morphSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.MorphButton.png", 115f);
        return morphSprite;
    }
}