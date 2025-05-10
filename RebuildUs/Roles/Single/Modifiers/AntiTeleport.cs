using System.Collections.Generic;
using UnityEngine;

namespace RebuildUs.Roles;

public static class AntiTeleport
{
    public static List<PlayerControl> antiTeleport = [];
    public static Vector3 position;

    public static void clearAndReload()
    {
        antiTeleport = [];
        position = Vector3.zero;
    }

    public static void setPosition()
    {
        if (position == Vector3.zero) return;  // Check if this has been set, otherwise first spawn on submerged will fail
        if (antiTeleport.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count > 0)
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
            if (SubmergedCompatibility.IsSubmerged)
            {
                SubmergedCompatibility.ChangeFloor(position.y > -7);
            }
        }
    }
}