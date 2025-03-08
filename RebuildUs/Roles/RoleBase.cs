using System.Collections.Generic;
using System.Linq;

namespace RebuildUs.Roles;

internal abstract class Role
{
    internal static readonly List<Role> AllRoles = [];
    internal PlayerControl Player;
    internal RoleId RoleId;

    internal abstract void OnMeetingStart();
    internal abstract void OnMeetingEnd();
    internal abstract void OnGameStart();
    internal abstract void OnGameEnd();
    internal abstract void OnDisconnected(PlayerControl player, DisconnectReasons reason);
    internal abstract void OnKill(PlayerControl target);
    internal abstract void OnDeath(PlayerControl killer = null);
    internal abstract void FixedUpdate();

    internal static void ClearAll()
    {
        AllRoles.Clear();
    }
}

internal abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
{
    internal static List<T> Players = [];

    internal RoleBase(PlayerControl Player)
    {
        this.Player = Player;
        Players.Add((T)this);
        AllRoles.Add(this);
    }

    internal static T GetLocal
    {
        get
        {
            return Players.Find(x => x.Player == PlayerControl.LocalPlayer);
        }
    }

    internal static List<PlayerControl> AllPlayers
    {
        get
        {
            return [.. Players.Select(x => x.Player)];
        }
    }
}