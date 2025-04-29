using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RebuildUs.Roles;

public static class RoleHelpers
{
    public static bool IsRole(this PlayerControl player, RoleId role)
    {
        foreach (var t in RoleData.AllRoleTypes)
        {
            if (role == t.Key)
            {
                return (bool)t.Value.GetMethod("IsRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
            }
        }

        return false;
    }

    public static void SetRole(this PlayerControl player, RoleId role)
    {
        foreach (var t in RoleData.AllRoleTypes)
        {
            if (role == t.Key)
            {
                t.Value.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
                return;
            }
        }
    }

    public static void EraseRole(this PlayerControl player, RoleId role)
    {
        if (IsRole(player, role))
        {
            foreach (var t in RoleData.AllRoleTypes)
            {
                if (role == t.Key)
                {
                    t.Value.GetMethod("EraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
                    return;
                }
            }
            Plugin.Instance.Logger.LogError($"EraseRole: no method found for role type {role}");
        }
    }

    public static void EraseAllRoles(this PlayerControl player)
    {
        foreach (var t in RoleData.AllRoleTypes)
        {
            t.Value.GetMethod("EraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
        }
    }

    public static void SwapRoles(this PlayerControl player, PlayerControl target)
    {
        foreach (var t in RoleData.AllRoleTypes)
        {
            if (player.IsRole(t.Key))
            {
                t.Value.GetMethod("swapRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player, target]);
            }
        }
    }

    public static string ModifyNameText(this PlayerControl player, string nameText)
    {
        if (player == null || player.Data.Disconnected) return nameText;

        foreach (var role in Role.AllRoles)
        {
            if (role.player == player)
            {
                nameText = role.ModifyNameText(nameText);
            }
        }

        foreach (var mod in Modifier.AllModifiers)
        {
            if (mod.player == player)
            {
                nameText = mod.ModifyNameText(nameText);
            }
        }

        return nameText;
    }

    public static string ModifyRoleText(this PlayerControl player, string roleText, List<RoleInfo> roleInfo, bool useColors = true, bool includeHidden = false)
    {
        foreach (var mod in Modifier.AllModifiers)
        {
            if (mod.player == player)
            {
                roleText = mod.ModifyRoleText(roleText, roleInfo, useColors, includeHidden);
            }
        }
        return roleText;
    }

    public static string MeetingInfoText(this PlayerControl player)
    {
        var text = "";
        var lines = new StringBuilder();
        foreach (var role in Role.AllRoles.Where(x => x.player == player))
        {
            text = role.MeetingInfoText();
            if (text != "") lines.AppendLine(text);
        }

        foreach (var mod in Modifier.AllModifiers.Where(x => x.player == player))
        {
            text = mod.MeetingInfoText();
            if (text != "") lines.AppendLine(text);
        }

        return lines.ToString();
    }

    public static void OnKill(this PlayerControl player, PlayerControl target)
    {
        Role.AllRoles.DoIf(x => x.player == player, x => x.OnKill(target));
        Modifier.AllModifiers.DoIf(x => x.player == player, x => x.OnKill(target));
    }

    public static void OnDeath(this PlayerControl player, PlayerControl killer)
    {
        Role.AllRoles.DoIf(x => x.player == player, x => x.OnDeath(killer));
        Modifier.AllModifiers.DoIf(x => x.player == player, x => x.OnDeath(killer));

        RPCProcedure.UpdateMeeting(player.PlayerId, true);
    }
}