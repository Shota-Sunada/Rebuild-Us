using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using System.Reflection;
using System;
using Rewired;

namespace RebuildUs.Roles.RoleBase;

internal abstract class ModRole
{
    internal static List<ModRole> AllRoles = [];

    internal PlayerControl player;
    internal RoleId roleId;

    internal abstract void OnMeetingStart();
    internal abstract void OnMeetingEnd();
    internal abstract void FixedUpdate();
    internal abstract void OnKill(PlayerControl target);
    internal abstract void OnDeath(PlayerControl killer = null);
    internal abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    internal abstract void OnRoleReset();
    internal abstract void MakeButtons(HudManager hm);
    internal abstract void SetButtonCooldowns();

    internal static void Clear()
    {
        AllRoles = [];
    }
}

[HarmonyPatch]
internal abstract class ModRoleBase<T> : ModRole where T : ModRoleBase<T>, new()
{
    internal static List<T> players = [];

    internal void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        AllRoles.Add(this);
    }

    internal static T Local
    {
        get
        {
            return players.FirstOrDefault(x => x.player.PlayerId == PlayerControl.LocalPlayer.PlayerId);
        }
    }

    internal static PlayerControl[] AllPlayers
    {
        get
        {
            return [.. players.Select(x => x.player)];
        }
    }

    internal static PlayerControl[] AlivePlayers
    {
        get
        {
            return [.. AllPlayers.Where(x => x.IsAlive())];
        }
    }

    internal static PlayerControl[] DeadPlayers
    {
        get
        {
            return [.. AllPlayers.Where(x => x.IsDead())];
        }
    }

    internal static bool Exists
    {
        get
        {
            return Helpers.IsRoleEnabled() && players.Count() > 0;
        }
    }

    internal static T GetRole(PlayerControl player = null)
    {
        player ??= PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player.PlayerId == player.PlayerId);
    }

    internal static bool IsRole(PlayerControl player)
    {
        return players.Any(x => x.player.PlayerId == player.PlayerId);
    }

    internal static T SetRole(PlayerControl player)
    {
        if (!IsRole(player))
        {
            var role = new T();
            role.Init(player);
            return role;
        }

        return null;
    }

    internal static void EraseRole(PlayerControl player, RoleId roleId)
    {
        players.DoIf(x => x.player.PlayerId == player.PlayerId, x => x.OnRoleReset());
        players.RemoveAll(x => x.player.PlayerId == player.PlayerId && x.roleId == roleId);
        AllRoles.RemoveAll(x => x.player.PlayerId == player.PlayerId && x.roleId == roleId);
    }

    internal static void SwapRole(PlayerControl player1, PlayerControl player2)
    {
        var index = players.FindIndex(x => x.player.PlayerId == player1.PlayerId);
        if (index >= 0)
        {
            players[index].player.PlayerId = player2.PlayerId;
        }
    }
}

internal static class RoleHelpers
{
    internal static bool IsRole<T>(this PlayerControl player) where T : ModRoleBase<T>, new()
    {
        return ModRoleBase<T>.IsRole(player);
    }

    internal static void SetRole<T>(this PlayerControl player) where T : ModRoleBase<T>, new()
    {
        ModRoleBase<T>.SetRole(player);
        return;
    }

    internal static void EraseRole<T>(this PlayerControl player, RoleId roleId) where T : ModRoleBase<T>, new()
    {
        ModRoleBase<T>.EraseRole(player, roleId);
    }

    internal static void EraseAllRoles<T>(this PlayerControl player) where T : ModRoleBase<T>, new()
    {
        foreach (var roleId in Enum.GetValues<RoleId>())
        {
            ModRoleBase<T>.EraseRole(player, roleId);
        }
    }

    internal static void SwapRoles<T>(this PlayerControl player1, PlayerControl player2) where T : ModRoleBase<T>, new()
    {
        if (player1.IsRole<T>())
        {
            ModRoleBase<T>.SwapRole(player1, player2);
        }
    }

    internal static void OnKill(this PlayerControl player, PlayerControl target)
    {
        ModRole.AllRoles.DoIf(x => x.player.PlayerId == player.PlayerId, x => x.OnKill(target));
    }

    internal static void OnDeath(this PlayerControl player, PlayerControl killer)
    {
        ModRole.AllRoles.DoIf(x => x.player.PlayerId == player.PlayerId, x => x.OnDeath(killer));

        RPCProcedure.UpdateMeeting(player.PlayerId, true);
    }

    internal static void MakeButtons(HudManager hm)
    {
        ModRole.AllRoles.Do(x => x.MakeButtons(hm));
    }

    internal static void SetButtonCooldowns()
    {
        ModRole.AllRoles.Do(x => x.SetButtonCooldowns());
    }
}