using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

public static class Trickster
{
    public static PlayerControl trickster;
    public static Color Color = Palette.ImpostorRed;
    public static float placeBoxCooldown = 30f;
    public static float lightsOutCooldown = 30f;
    public static float lightsOutDuration = 10f;
    public static float lightsOutTimer = 0f;

    private static Sprite placeBoxButtonSprite;
    private static Sprite lightOutButtonSprite;
    private static Sprite tricksterVentButtonSprite;

    public static Sprite getPlaceBoxButtonSprite()
    {
        if (placeBoxButtonSprite) return placeBoxButtonSprite;
        placeBoxButtonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.PlaceJackInTheBoxButton.png", 115f);
        return placeBoxButtonSprite;
    }

    public static Sprite getLightsOutButtonSprite()
    {
        if (lightOutButtonSprite) return lightOutButtonSprite;
        lightOutButtonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.LightsOutButton.png", 115f);
        return lightOutButtonSprite;
    }

    public static Sprite getTricksterVentButtonSprite()
    {
        if (tricksterVentButtonSprite) return tricksterVentButtonSprite;
        tricksterVentButtonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.TricksterVentButton.png", 115f);
        return tricksterVentButtonSprite;
    }

    public static void clearAndReload()
    {
        trickster = null;
        lightsOutTimer = 0f;
        placeBoxCooldown = CustomOptionHolder.tricksterPlaceBoxCooldown.getFloat();
        lightsOutCooldown = CustomOptionHolder.tricksterLightsOutCooldown.getFloat();
        lightsOutDuration = CustomOptionHolder.tricksterLightsOutDuration.getFloat();
        JackInTheBox.UpdateStates(); // if the role is erased, we might have to update the state of the created objects
    }
}