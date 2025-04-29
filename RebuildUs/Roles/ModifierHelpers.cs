using System.Reflection;

namespace RebuildUs.Roles;

public static class ModifierHelpers
{
    public static bool HasModifier(this PlayerControl player, ModifierId mod)
    {
        foreach (var t in ModifierData.AllModifierTypes)
        {
            if (mod == t.Key)
            {
                return (bool)t.Value.GetMethod("HasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
            }
        }
        return false;
    }

    public static void AddModifier(this PlayerControl player, ModifierId mod)
    {
        foreach (var t in ModifierData.AllModifierTypes)
        {
            if (mod == t.Key)
            {
                t.Value.GetMethod("AddModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
                return;
            }
        }
    }

    public static void EraseModifier(this PlayerControl player, ModifierId mod, RoleId newRole = RoleId.NoRole)
    {
        if (HasModifier(player, mod))
        {
            foreach (var t in ModifierData.AllModifierTypes)
            {
                if (mod == t.Key)
                {
                    t.Value.GetMethod("EraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player, newRole]);
                    return;
                }
            }
            Plugin.Instance.Logger.LogError($"EraseModifier: no method found for role id {mod}");
        }
    }

    public static void EraseAllModifiers(this PlayerControl player, RoleId newRole = RoleId.NoRole)
    {
        foreach (var t in ModifierData.AllModifierTypes)
        {
            t.Value.GetMethod("EraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player, newRole]);
        }
    }

    public static void SwapModifiers(this PlayerControl player, PlayerControl target)
    {
        foreach (var t in ModifierData.AllModifierTypes)
        {
            if (player.HasModifier(t.Key))
            {
                t.Value.GetMethod("SwapModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player, target]);
            }
        }
    }
}