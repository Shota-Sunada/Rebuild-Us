using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace RebuildUs.Roles;

public enum RoleType
{
    Crewmate = 0,
    Impostor = 1,
    Neutral = 2,
}

public enum RoleId : byte
{
    // Among Us Roles
    Crewmate = 0,
    Impostor = 1,

    // Special Roles
    GM = 10,

    // Add new roles here
    Sheriff = 30,

    NoRole = byte.MaxValue
}

public static class RoleData
{
    public static Dictionary<RoleId, Type> AllRoleTypes = [

    ];

    public static void ClearAndReloadRoles()
    {
        Role.ClearAll();
    }

    public static void FixedUpdate(PlayerControl player)
    {
        Role.AllRoles.DoIf(x => x.player == player, x => x.FixedUpdate());
        Modifier.AllModifiers.DoIf(x => x.player == player, x => x.FixedUpdate());
    }

    public static void OnMeetingStart()
    {
        Role.AllRoles.Do(x => x.OnMeetingStart());
        Modifier.AllModifiers.Do(x => x.OnMeetingStart());
    }

    public static void OnMeetingEnd()
    {
        Role.AllRoles.Do(x => x.OnMeetingEnd());
        Modifier.AllModifiers.Do(x => x.OnMeetingEnd());
    }

    public static void MakeButtons(HudManager hm)
    {
        Role.AllRoles.Do(x => x.MakeButtons(hm));
    }

    public static void SetButtonCooldowns()
    {
        Role.AllRoles.Do(x => x.SetButtonCooldowns());
    }
}

public abstract class Role
{
    public static List<Role> AllRoles = [];
    public PlayerControl player;
    public RoleId roleId;

    public abstract void OnMeetingStart();
    public abstract void OnMeetingEnd();
    public abstract void FixedUpdate();
    public abstract void OnKill(PlayerControl target);
    public abstract void OnDeath(PlayerControl killer = null);
    public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    public abstract void OnRoleReset();
    public abstract void MakeButtons(HudManager hm);
    public abstract void SetButtonCooldowns();
    public virtual void PostInit() { }
    public virtual string ModifyNameText(string nameText) { return nameText; }
    public virtual string MeetingInfoText() { return ""; }

    public static void ClearAll()
    {
        AllRoles = [];
    }
}

public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
{
    public static List<T> players = [];
    public static RoleId baseRoleId;

    public RoleBase(RoleId roleId)
    {
        base.roleId = baseRoleId = roleId;
    }

    public void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        AllRoles.Add(this);
        PostInit();
    }

    public static T Local
    {
        get
        {
            return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
        }
    }

    public static List<PlayerControl> AllPlayers
    {
        get
        {
            return [.. players.Select(x => x.player)];
        }
    }

    public static List<PlayerControl> LivingPlayers
    {
        get
        {
            return [.. players.Select(x => x.player).Where(x => x.IsAlive())];
        }
    }

    public static List<PlayerControl> DeadPlayers
    {
        get
        {
            return [.. players.Select(x => x.player).Where(x => !x.IsAlive())];
        }
    }

    public static bool Exists
    {
        get { return Helpers.IsRoleEnabled() && players.Count > 0; }
    }

    public static T GetRole(PlayerControl player = null)
    {
        player ??= PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }

    public static bool IsRole(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }

    public static T SetRole(PlayerControl player)
    {
        if (!IsRole(player))
        {
            var role = new T();
            role.Init(player);
            return role;
        }
        return null;
    }

    public static void EraseRole(PlayerControl player)
    {
        players.DoIf(x => x.player == player, x => x.OnRoleReset());
        players.RemoveAll(x => x.player == player && x.roleId == baseRoleId);
        AllRoles.RemoveAll(x => x.player == player && x.roleId == baseRoleId);
    }

    public static void SwapRole(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            players[index].player = p2;
        }
    }
}