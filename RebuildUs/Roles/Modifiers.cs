using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RebuildUs.Roles;

public enum ModifierId : byte
{
    Madmate = 0,

    NoModifier = byte.MaxValue
}

[HarmonyPatch]
public static class ModifierData
{
    public static Dictionary<ModifierId, Type> AllModifierTypes = [

    ];
}

public abstract class Modifier
{
    public static List<Modifier> AllModifiers = [];
    public PlayerControl player;
    public ModifierId modifierId;

    public abstract void OnMeetingStart();
    public abstract void OnMeetingEnd();
    public abstract void FixedUpdate();
    public abstract void OnKill(PlayerControl target);
    public abstract void OnDeath(PlayerControl killer = null);
    public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    public virtual void ResetModifier() { }

    public virtual string ModifyNameText(string nameText) { return nameText; }
    public virtual string ModifyRoleText(string roleText, List<RoleInfo> roleInfo, bool useColors = true, bool includeHidden = false) { return roleText; }
    public virtual string MeetingInfoText() { return ""; }

    public static void ClearAll()
    {
        AllModifiers = [];
    }
}

public abstract class ModifierBase<T> : Modifier where T : ModifierBase<T>, new()
{
    public static List<T> Players = [];
    public static ModifierId baseModifierId;
    public static List<RoleId> persistRoleChange = [];

    public void Init(PlayerControl player)
    {
        this.player = player;
        Players.Add((T)this);
        AllModifiers.Add(this);
    }

    public static T Local
    {
        get
        {
            return Players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
        }
    }

    public static List<PlayerControl> AllPlayers
    {
        get
        {
            return [.. Players.Select(x => x.player)];
        }
    }

    public static List<PlayerControl> LivingPlayers
    {
        get
        {
            return [.. Players.Select(x => x.player).Where(x => x.IsAlive())];
        }
    }

    public static List<PlayerControl> DeadPlayers
    {
        get
        {
            return [.. Players.Select(x => x.player).Where(x => !x.IsAlive())];
        }
    }

    public static bool Exists
    {
        get { return Helpers.IsRoleEnabled() && Players.Count > 0; }
    }

    public static T GetModifier(PlayerControl player = null)
    {
        player ??= PlayerControl.LocalPlayer;
        return Players.FirstOrDefault(x => x.player == player);
    }

    public static bool HasModifier(PlayerControl player)
    {
        return Players.Any(x => x.player == player);
    }

    public static T AddModifier(PlayerControl player)
    {
        var mod = new T();
        mod.Init(player);
        return mod;
    }

    public static void EraseModifier(PlayerControl player, RoleId newRole = RoleId.NoRole)
    {
        var toRemove = new List<T>();

        foreach (var p in Players)
        {
            if (p.player == player && p.modifierId == baseModifierId && !persistRoleChange.Contains(newRole))
            {
                toRemove.Add(p);
            }
        }
        Players.RemoveAll(toRemove.Contains);
        AllModifiers.RemoveAll(x => toRemove.Contains(x));
    }

    public static void SwapModifier(PlayerControl p1, PlayerControl p2)
    {
        var index = Players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            Players[index].player = p2;
        }
    }
}