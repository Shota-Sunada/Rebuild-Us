using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace RebuildUs.Roles;

public enum ModifierId
{
    Madmate = 0,
    AkujoHonmei,
    AkujoKeep,

    // don't put anything below this
    NoModifier = int.MaxValue
}

[HarmonyPatch]
public static class ModifierData
{
    public static Dictionary<ModifierId, Type> allModTypes = [];
}

public abstract class Modifier
{
    public static List<Modifier> allModifiers = [];
    public PlayerControl player;
    public ModifierId modifierId;

    public abstract void OnMeetingStart();
    public abstract void OnMeetingEnd();
    public abstract void FixedUpdate();
    public abstract void OnKill(PlayerControl target);
    public abstract void OnDeath(PlayerControl killer = null);
    public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    public abstract void Clear();
    public virtual void ResetModifier() { }

    public virtual string modifyNameText(string nameText) { return nameText; }
    public virtual string modifyRoleText(string roleText, List<RoleInfo> roleInfo, bool useColors = true, bool includeHidden = false) { return roleText; }
    public virtual string meetingInfoText() { return ""; }

    public static void ClearAll()
    {
        allModifiers = [];
    }
}

[HarmonyPatch]
public abstract class ModifierBase<T> : Modifier where T : ModifierBase<T>, new()
{
    public static List<T> players = [];
    public static ModifierId baseModifierId;
    public static List<RoleId> persistRoleChange = [];

    public void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        allModifiers.Add(this);
    }

    public static T local
    {
        get
        {
            return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
        }
    }

    public static List<PlayerControl> allPlayers
    {
        get
        {
            return players.Select(x => x.player).ToList();
        }
    }

    public static List<PlayerControl> livingPlayers
    {
        get
        {
            return players.Select(x => x.player).Where(x => x.isAlive()).ToList();
        }
    }

    public static List<PlayerControl> deadPlayers
    {
        get
        {
            return players.Select(x => x.player).Where(x => !x.isAlive()).ToList();
        }
    }

    public static bool exists
    {
        get { return Helpers.RolesEnabled && players.Count > 0; }
    }

    public static T getModifier(PlayerControl player = null)
    {
        player = player ?? PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }

    public static bool hasModifier(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }

    public static T addModifier(PlayerControl player)
    {
        T mod = new();
        mod.Init(player);
        return mod;
    }

    public static void eraseModifier(PlayerControl player, RoleId newRole = RoleId.NoRole)
    {
        List<T> toRemove = [];

        foreach (var p in players)
        {
            if (p.player == player && p.modifierId == baseModifierId && !persistRoleChange.Contains(newRole))
                toRemove.Add(p);
        }
        players.RemoveAll(x => toRemove.Contains(x));
        allModifiers.RemoveAll(x => toRemove.Contains(x));
    }

    public static void swapModifier(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            players[index].player = p2;
        }
    }
}


public static class ModifierHelpers
{
    public static bool hasModifier(this PlayerControl player, ModifierId mod)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (mod == t.Key)
            {
                return (bool)t.Value.GetMethod("hasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }
        }
        return false;
    }

    public static void addModifier(this PlayerControl player, ModifierId mod)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (mod == t.Key)
            {
                t.Value.GetMethod("addModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                return;
            }
        }
    }

    public static void eraseModifier(this PlayerControl player, ModifierId mod, RoleId newRole = RoleId.NoRole)
    {
        if (hasModifier(player, mod))
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (mod == t.Key)
                {
                    t.Value.GetMethod("eraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, newRole });
                    return;
                }
            }
            RebuildUs.Instance.Logger.LogError($"eraseRole: no method found for role type {mod}");
        }
    }

    public static void eraseAllModifiers(this PlayerControl player, RoleId newRole = RoleId.NoRole)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            t.Value.GetMethod("eraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, newRole });
        }

        if (Bait.bait.Any(x => x.PlayerId == player.PlayerId)) Bait.bait.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Bloody.bloody.Any(x => x.PlayerId == player.PlayerId)) Bloody.bloody.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == player.PlayerId)) AntiTeleport.antiTeleport.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Sunglasses.sunglasses.Any(x => x.PlayerId == player.PlayerId)) Sunglasses.sunglasses.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (player == Tiebreaker.tiebreaker) Tiebreaker.clearAndReload();
        if (player == Mini.mini) Mini.clearAndReload();
        if (Vip.vip.Any(x => x.PlayerId == player.PlayerId)) Vip.vip.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Invert.invert.Any(x => x.PlayerId == player.PlayerId)) Invert.invert.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (Chameleon.chameleon.Any(x => x.PlayerId == player.PlayerId)) Chameleon.chameleon.RemoveAll(x => x.PlayerId == player.PlayerId);
        if (player == Armored.armored) Armored.clearAndReload();
    }

    public static void swapModifiers(this PlayerControl player, PlayerControl target)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (player.hasModifier(t.Key))
            {
                t.Value.GetMethod("swapModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, target });
            }
        }
    }
}