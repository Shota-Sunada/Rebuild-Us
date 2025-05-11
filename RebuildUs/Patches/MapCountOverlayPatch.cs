using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class MapCountOverlayPatch
{
    public static Dictionary<SystemTypes, List<Color>> players = [];

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    public static bool Prefix(MapCountOverlay __instance)
    {
        // Save colors for the Hacker
        __instance.timer += Time.deltaTime;
        if (__instance.timer < 0.1f)
        {
            return false;
        }
        __instance.timer = 0f;
        players = [];
        bool commsActive = false;
        foreach (var task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
        {
            if (task.TaskType == TaskTypes.FixComms)
            {
                commsActive = true;
            }
        }

        if (!__instance.isSab && commsActive)
        {
            __instance.isSab = true;
            __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
            __instance.SabotageText.gameObject.SetActive(true);
            return false;
        }
        if (__instance.isSab && !commsActive)
        {
            __instance.isSab = false;
            __instance.BackgroundColor.SetColor(Color.green);
            __instance.SabotageText.gameObject.SetActive(false);
        }

        for (int i = 0; i < __instance.CountAreas.Length; i++)
        {
            var counterArea = __instance.CountAreas[i];
            List<Color> roomColors = [];
            players.Add(counterArea.RoomType, roomColors);

            if (!commsActive)
            {
                var plainShipRoom = MapUtilities.CachedShipStatus.FastRooms[counterArea.RoomType];
                if (plainShipRoom != null && plainShipRoom.roomArea)
                {
                    HashSet<int> hashSet = [];
                    int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                    int num2 = 0;
                    for (int j = 0; j < num; j++)
                    {
                        var collider2D = __instance.buffer[j];
                        if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies)
                        {
                            num2++;
                            var bodyComponent = collider2D.GetComponent<DeadBody>();
                            if (bodyComponent)
                            {
                                var playerInfo = GameData.Instance.GetPlayerById(bodyComponent.ParentId);
                                if (playerInfo != null)
                                {
                                    var color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                                    if (Hacker.onlyColorType)
                                    {
                                        color = Helpers.isD(playerInfo.PlayerId) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];
                                    }
                                    roomColors.Add(color);
                                }
                            }
                        }
                        else if (!collider2D.isTrigger)
                        {
                            var component = collider2D.GetComponent<PlayerControl>();
                            if (component && component.Data != null && !component.Data.Disconnected && !component.Data.IsDead && (__instance.showLivePlayerPosition || !component.AmOwner) && hashSet.Add((int)component.PlayerId))
                            {
                                num2++;
                                if (component?.cosmetics?.currentBodySprite?.BodySprite?.material != null)
                                {
                                    var color = component.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
                                    if (Hacker.onlyColorType)
                                    {
                                        color = Helpers.isLighterColor(component) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];
                                    }
                                    roomColors.Add(color);
                                }
                            }
                        }
                    }

                    counterArea.UpdateCount(num2);
                }
                else
                {
                    Debug.LogWarning("Couldn't find counter for:" + counterArea.RoomType);
                }
            }
            else
            {
                counterArea.UpdateCount(0);
            }
        }
        return false;
    }
}