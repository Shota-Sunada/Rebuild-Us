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
    internal static Dictionary<RoleId, List<ModRole>> RoleInstances = [];

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
        RoleInstances.Clear();
    }

    internal static ModRole AssignRole(ModRole role, PlayerControl player)
    {
        role.player = player;
        if (!RoleInstances.ContainsKey(role.roleId))
        {
            RoleInstances[role.roleId] = [];
        }
        RoleInstances[role.roleId].Add(role);
        return role;
    }

    internal static T AssignRole<T>(PlayerControl player) where T : ModRole, new()
    {
        var roleId = RolesManager.AllRoles.FirstOrDefault(r => r.Value.NameKey == typeof(T).Name).Key;
        if (roleId == default)
        {
            throw new InvalidOperationException($"RoleId for type {typeof(T).Name} not found.");
        }

        var role = new T { player = player, roleId = roleId };
        return (T)AssignRole(role, player);
    }

    internal static void RemoveRole(PlayerControl player, RoleId roleId)
    {
        if (RoleInstances.TryGetValue(roleId, out var roles))
        {
            roles.RemoveAll(r => r.player.PlayerId == player.PlayerId);
            if (roles.Count == 0) RoleInstances.Remove(roleId);
        }
    }

    internal static ModRole GetRole(PlayerControl player, RoleId roleId)
    {
        return RoleInstances.TryGetValue(roleId, out var roles)
            ? roles.FirstOrDefault(r => r.player.PlayerId == player.PlayerId)
            : null;
    }

    internal static bool HasRole(PlayerControl player, RoleId roleId)
    {
        return GetRole(player, roleId) != null;
    }

    internal static IEnumerable<PlayerControl> GetPlayersWithRole(RoleId roleId)
    {
        return RoleInstances.TryGetValue(roleId, out var roles)
            ? roles.Select(r => r.player)
            : [];
    }

    internal static void SetAllRolesButtonCooldowns()
    {
        foreach (var roleList in RoleInstances.Values)
        {
            foreach (var role in roleList)
            {
                role.SetButtonCooldowns();
            }
        }
    }

    internal static void MakeAllRolesButtons(HudManager hm)
    {
        foreach (var roleList in RoleInstances.Values)
        {
            foreach (var role in roleList)
            {
                role.MakeButtons(hm);
            }
        }
    }

    internal static void OnKill(PlayerControl killer, PlayerControl target)
    {
        foreach (var roleList in RoleInstances.Values)
        {
            foreach (var role in roleList)
            {
                if (role.player.PlayerId == killer.PlayerId)
                {
                    role.OnKill(target);
                }
            }
        }
    }

    internal static void OnDeath(PlayerControl player, PlayerControl killer = null)
    {
        foreach (var roleList in RoleInstances.Values)
        {
            foreach (var role in roleList)
            {
                if (role.player.PlayerId == player.PlayerId)
                {
                    role.OnDeath(killer);
                }
            }
        }
    }
}