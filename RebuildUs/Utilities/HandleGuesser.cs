using UnityEngine;

namespace RebuildUs.Utilities;

public static class HandleGuesser
{
    private static Sprite targetSprite;
    public static bool hasMultipleShotsPerMeeting = false;
    public static bool killsThroughShield = true;
    public static bool evilGuesserCanGuessSpy = true;
    public static bool guesserCantGuessSnitch = false;

    public static Sprite getTargetSprite()
    {
        if (targetSprite) return targetSprite;
        targetSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.TargetIcon.png", 150f);
        return targetSprite;
    }

    public static bool isGuesser(byte playerId)
    {
        return Guesser.isGuesser(playerId);
    }

    public static void clear(byte playerId)
    {
        Guesser.clear(playerId);
    }

    public static int remainingShots(byte playerId, bool shoot = false)
    {
        return Guesser.remainingShots(playerId, shoot);
    }

    public static void clearAndReload()
    {
        Guesser.clearAndReload();
        guesserCantGuessSnitch = CustomOptionHolder.guesserCantGuessSnitchIfTaksDone.getBool();
        hasMultipleShotsPerMeeting = CustomOptionHolder.guesserHasMultipleShotsPerMeeting.getBool();
        killsThroughShield = CustomOptionHolder.guesserKillsThroughShield.getBool();
        evilGuesserCanGuessSpy = CustomOptionHolder.guesserEvilCanKillSpy.getBool();
    }
}