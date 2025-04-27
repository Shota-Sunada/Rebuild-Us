using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RebuildUs.Roles.RoleBase;
using UnityEngine;

namespace RebuildUs.Roles;

internal enum RoleType
{
    Crewmate = 0,
    Impostor = 1,
    Neutral = 2,
    Ignore = 3,
}

internal enum RoleId : byte
{
    // Among Us Roles
    Crewmate = 0,
    Impostor = 1,

    // Special Roles
    GM = 10,

    // Add new roles here
    Sheriff = 30,
}

internal static class RolesManager
{
    internal static RoleInfoAttribute Crewmate = new("Crewmate", "CrewmateIntro", "CrewmateShort", "CrewmateFull", RoleType.Crewmate, RoleId.Crewmate);
    internal static RoleInfoAttribute Impostor = new("Impostor", "ImpostorIntro", "ImpostorShort", "ImpostorFull", RoleType.Impostor, RoleId.Impostor);

    internal static Dictionary<RoleId, RoleInfoAttribute> AllRoles { get; } = [];

    internal static void RegisterRoles()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var roleInfo = type.GetCustomAttribute<RoleInfoAttribute>();
            if (roleInfo == null) continue;

            AllRoles.Add(roleInfo.RoleId, roleInfo);
            Plugin.Instance.Logger.LogMessage($"Registering role: {roleInfo.NameKey}");
        }
    }

    internal static ModRole CreateRoleInstance(RoleId roleId, PlayerControl player)
    {
        if (AllRoles.TryGetValue(roleId, out var roleInfo))
        {
            var type = roleInfo.GetType();
            if (type != null && Activator.CreateInstance(type) is ModRole role)
            {
                return ModRole.AssignRole(role, player);
            }
        }
        return null;
    }

    internal static List<RoleInfoAttribute> GetRoleInfoForPlayer(PlayerControl player)
    {
        var infos = new List<RoleInfoAttribute>();
        if (player == null) return infos;

        foreach (var (roleId, roleInfo) in AllRoles)
        {
            if (ModRole.HasRole(player, roleId))
            {
                infos.Add(roleInfo);
            }
        }

        if (infos.Count == 0)
        {
            if (player.Data.Role.IsImpostor)
            {
                infos.Add(Impostor);
            }
            else
            {
                infos.Add(Crewmate);
            }
        }

        return infos;
    }

    // internal static RoleId GetRoleId<T>() where T : ModRole
    // {
    //     return AllRoles.FirstOrDefault(r => r.Value.NameKey == typeof(T).Name).Key;
    // }
}

[AttributeUsage(AttributeTargets.Class)]
internal class RoleInfoAttribute : Attribute
{
    internal string NameKey { get; }
    internal string IntroDescKey { get; }
    internal string ShortDescKey { get; }
    internal string FullDescKey { get; }
    internal RoleType RoleType { get; }
    internal RoleId RoleId { get; }
    internal Color Color { get; }

    internal RoleInfoAttribute(
        string nameKey,
        string introDescKey,
        string shortDescKey,
        string fullDescKey,
        RoleType roleType,
        RoleId roleId)
    {
        NameKey = nameKey;
        IntroDescKey = introDescKey;
        ShortDescKey = shortDescKey;
        FullDescKey = fullDescKey;
        RoleType = roleType;
        RoleId = roleId;
        Color = Color.white;
    }

    internal RoleInfoAttribute(
        string nameKey,
        string introDescKey,
        string shortDescKey,
        string fullDescKey,
        RoleType roleType,
        RoleId roleId,
        byte colorR, byte colorG, byte colorB)
    {
        NameKey = nameKey;
        IntroDescKey = introDescKey;
        ShortDescKey = shortDescKey;
        FullDescKey = fullDescKey;
        RoleType = roleType;
        RoleId = roleId;
        Color = new Color32(colorR, colorG, colorB, byte.MaxValue);
    }
}