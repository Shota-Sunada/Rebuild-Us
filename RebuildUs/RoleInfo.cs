using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RebuildUs.Localization;
using RebuildUs.Modules;
using RebuildUs.Roles;
using UnityEngine;

namespace RebuildUs;

public class RoleInfo
{
    public static List<RoleInfo> AllRoleInfos = [];

    public Color color;
    public virtual string Name => Tr.Get(nameKey);
    public virtual string NameColored => Name.Cs(color);
    public virtual string IntroDescription => Tr.Get(nameKey + "IntroDesc");
    public virtual string ShortDescription => Tr.Get(nameKey + "ShortDesc");
    public virtual string FullDescription => Tr.Get(nameKey + "FullDesc");
    public virtual string Blurb => Tr.Get(nameKey + "Blurb");

    public bool Enabled { get { return Helpers.IsRoleEnabled() && (this == crewmate || this == impostor || (baseOption != null && baseOption.Enabled)); } }
    public RoleId roleId;

    private readonly string nameKey;
    private readonly CustomOption baseOption;

    public RoleInfo(string nameKey, Color color, CustomOption baseOption, RoleId roleId)
    {
        this.color = color;
        this.nameKey = nameKey;
        this.baseOption = baseOption;
        this.roleId = roleId;
    }

    public static RoleInfo impostor = new("Impostor", Palette.ImpostorRed, null, RoleId.Impostor);
    public static RoleInfo crewmate = new("Crewmate", Palette.CrewmateBlue, null, RoleId.Crewmate);

    public static List<RoleInfo> GetRoleInfoForPlayer(PlayerControl player, RoleId[] excludeRoles = null)
    {
        var result = new List<RoleInfo>();

        if (player == null) return result;

        // ここに追記

        // Default roles
        if (result.Count == 0 && player.Data.Role.IsImpostor) result.Add(impostor); // Just Impostor
        if (result.Count == 0 && !player.Data.Role.IsImpostor) result.Add(crewmate); // Just Crewmate

        if (excludeRoles != null)
        {
            result.RemoveAll(x => excludeRoles.Contains(x.roleId));
        }

        return result;
    }

    public static string GetRolesString(PlayerControl player, bool useColors, RoleId[] excludeRoles = null)
    {
        if (player?.Data?.Disconnected != false) return "";

        var roleInfo = GetRoleInfoForPlayer(player, excludeRoles);
        var roleText = new StringBuilder(" ").Append(roleInfo.Select(x => useColors ? x.Name.Color(x.color) : x.Name).ToArray()).ToString();
        roleText = player.ModifyRoleText(roleText, roleInfo, useColors);

        return roleText;
    }
}