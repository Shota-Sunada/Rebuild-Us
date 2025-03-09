using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RebuildUs.Roles;

internal enum RoleType
{
    Crewmate = 0,
    Impostor = 1,
    Neutral = 2,
    Ignore = 3,
}

internal enum RoleId
{
    None = -1,

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
    internal static List<RoleInfoAttribute> AllRoles { get; } = [];

    internal static void RegisterRoles()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var roleInfo = type.GetCustomAttribute<RoleInfoAttribute>();
            if (roleInfo == null) continue;

            AllRoles.Add(roleInfo);
            RebuildUsPlugin.Instance.Logger.LogMessage($"Registering role: {nameof(type)}");
        }
    }


}

[AttributeUsage(AttributeTargets.Class)]
internal class RoleInfoAttribute : Attribute
{
    internal string NameKey { get; }
    internal Color Color { get; }
    internal string IntroDescKey { get; }
    internal string ShortDescKey { get; }
    internal string FullDescKey { get; }
    internal RoleType RoleType { get; }
    internal RoleId RoleId { get; }

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
    }
}