using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace RebuildUs.Patches;

[HarmonyPatch]
public static class VentPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static bool CanUsePrefix(Vent __instance, ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode is GameModes.HideNSeek) return true;

        var num = float.MaxValue;
        var @object = pc.Object;
        var roleCouldUse = @object.roleCanUseVents();

        if (__instance.name.StartsWith("SealedVent_"))
        {
            canUse = couldUse = false;
            __result = num;
            return false;
        }

        // Submerged Compatibility if needed:
        if (SubmergedCompatibility.IsSubmerged)
        {
            if (SubmergedCompatibility.getInTransition())
            {
                __result = float.MaxValue;
                return canUse = couldUse = false;
            }

            switch (__instance.Id)
            {
                case 9: // Cannot enter vent 9 (Engine Room Exit Only Vent)!
                    if (PlayerControl.LocalPlayer.inVent) break;
                    __result = float.MaxValue;
                    return canUse = couldUse = false;
                case 14: // Lower Central
                    __result = float.MaxValue;
                    couldUse = roleCouldUse && !pc.IsDead && (@object.CanMove || @object.inVent);
                    canUse = couldUse;
                    if (canUse)
                    {
                        var center = @object.Collider.bounds.center;
                        var position = __instance.transform.position;
                        __result = Vector2.Distance(center, position);
                        canUse &= __result <= __instance.UsableDistance;
                    }
                    return false;
            }
        }

        var usableDistance = __instance.UsableDistance;
        if (__instance.name.StartsWith("JackInTheBoxVent_"))
        {
            if (Trickster.trickster != PlayerControl.LocalPlayer)
            {
                // Only the Trickster can use the Jack-In-The-Boxes!
                canUse = false;
                couldUse = false;
                __result = num;
                return false;
            }
            else
            {
                // Reduce the usable distance to reduce the risk of getting stuck while trying to jump into the box if it's placed near objects
                usableDistance = 0.4f;
            }
        }

        couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
        canUse = couldUse;
        if (canUse)
        {
            Vector3 center = @object.Collider.bounds.center;
            Vector3 position = __instance.transform.position;
            num = Vector2.Distance(center, position);
            canUse &= num <= usableDistance && (!PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false) || __instance.name.StartsWith("JackInTheBoxVent_"));
        }
        __result = num;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
    public static bool UsePrefix(Vent __instance)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;

        if (Trapper.playersOnMap.Contains(PlayerControl.LocalPlayer.PlayerId)) return false;

        __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);
        bool canMoveInVents = PlayerControl.LocalPlayer != Spy.spy && !Trapper.playersOnMap.Contains(PlayerControl.LocalPlayer.PlayerId);
        if (!canUse) return false; // No need to execute the native method as using is disallowed anyways

        bool isEnter = !PlayerControl.LocalPlayer.inVent;

        if (__instance.name.StartsWith("JackInTheBoxVent_"))
        {
            __instance.SetButtons(isEnter && canMoveInVents);
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UseUncheckedVent, Hazel.SendOption.Reliable);
            writer.WritePacked(__instance.Id);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(isEnter ? byte.MaxValue : (byte)0);
            writer.EndMessage();
            RPCProcedure.useUncheckedVent(__instance.Id, PlayerControl.LocalPlayer.PlayerId, isEnter ? byte.MaxValue : (byte)0);
            return false;
        }

        if (isEnter)
        {
            PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);
        }
        else
        {
            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);
        }
        __instance.SetButtons(isEnter && canMoveInVents);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Vent), nameof(Vent.TryMoveToVent))]
    public static bool TryMoveToVentPrefix(Vent otherVent)
    {
        return !Trapper.playersOnMap.Contains(PlayerControl.LocalPlayer.PlayerId);
    }
}