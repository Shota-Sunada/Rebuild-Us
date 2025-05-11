using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class CounterAreaPatch
{
    private static Material defaultMat;
    private static Material newMat;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
    public static void UpdateCountPostfix(CounterArea __instance)
    {
        // Hacker display saved colors on the admin panel
        bool showHackerInfo = Hacker.exists && PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && Hacker.getRole().hackerTimer > 0;
        if (MapCountOverlayPatch.players.ContainsKey(__instance.RoomType))
        {
            var colors = MapCountOverlayPatch.players[__instance.RoomType];
            int i = -1;
            foreach (var icon in __instance.myIcons.GetFastEnumerator())
            {
                i += 1;
                var renderer = icon.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    if (defaultMat == null) defaultMat = renderer.material;
                    if (newMat == null) newMat = UnityEngine.Object.Instantiate<Material>(defaultMat);
                    if (showHackerInfo && colors.Count > i)
                    {
                        renderer.material = newMat;
                        var color = colors[i];
                        renderer.material.SetColor("_BodyColor", color);
                        var id = Palette.PlayerColors.IndexOf(color);
                        if (id < 0)
                        {
                            renderer.material.SetColor("_BackColor", color);
                        }
                        else
                        {
                            renderer.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                        }
                        renderer.material.SetColor("_VisorColor", Palette.VisorColor);
                    }
                    else
                    {
                        renderer.material = defaultMat;
                    }
                }
            }
        }
    }
}