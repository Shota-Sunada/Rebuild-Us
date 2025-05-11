using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RebuildUs.Localization;
using static RebuildUs.RebuildUs;

namespace RebuildUs.Roles;

[HarmonyPatch]
public static class RoleData
{
    public static readonly (RoleId id, Type type)[] ALL_ROLE_TYPES = [
        // CREWMATES
        (RoleId.Detective, typeof(Detective)),
        (RoleId.Engineer, typeof(Engineer)),
        (RoleId.Hacker, typeof(Hacker)),
        (RoleId.Lighter, typeof(Lighter)),
        (RoleId.Mayor, typeof(Mayor)),
        (RoleId.Medic, typeof(Medic)),
        (RoleId.Seer, typeof(Seer)),
        (RoleId.Sheriff, typeof(Sheriff)),
        (RoleId.TimeMaster, typeof(TimeMaster)),
        (RoleId.Tracker, typeof(Tracker)),

        // IMPOSTORS
        (RoleId.Eraser, typeof(Eraser)),

        // NEUTRALS
        (RoleId.Arsonist, typeof(Arsonist)),
        (RoleId.Jester, typeof(Jester)),
    ];
}

public abstract class Role
{
    public static List<Role> allRoles = [];
    public PlayerControl player;
    public RoleId roleId;

    public abstract void OnMeetingStart();
    public abstract void OnMeetingEnd();
    public abstract void FixedUpdate();
    public abstract void HudUpdate();
    public abstract void OnKill(PlayerControl target);
    public abstract void OnDeath(PlayerControl killer = null);
    public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    public abstract void MakeButtons(HudManager hm);
    public abstract void SetButtonCooldowns();
    public abstract void Clear();
    public virtual void ResetRole() { }
    public virtual void PostInit() { }
    public virtual string modifyNameText(string nameText) { return nameText; }
    public virtual string meetingInfoText() { return ""; }

    public static void ClearAll()
    {
        allRoles = [];
    }
}

[HarmonyPatch]
public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
{
    public static List<T> players = [];
    public static RoleId baseRoleId;

    public void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        allRoles.Add(this);
        PostInit();
    }

    public static T local => players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);

    public static List<PlayerControl> allPlayers => [.. players.Select(x => x.player)];

    public static List<PlayerControl> livingPlayers => [.. players.Select(x => x.player).Where(x => x.isAlive())];

    public static List<PlayerControl> deadPlayers => [.. players.Select(x => x.player).Where(x => !x.isAlive())];

    public static bool exists => Helpers.RolesEnabled && players.Count > 0;

    public static T getRole(PlayerControl player = null)
    {
        player ??= PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }

    public static bool isRole(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }

    public static T setRole(PlayerControl player)
    {
        if (!isRole(player))
        {
            T role = new();
            role.Init(player);
            return role;
        }
        return null;
    }

    public static void eraseRole(PlayerControl player)
    {
        players.DoIf(x => x.player == player, x => x.ResetRole());
        players.RemoveAll(x => x.player == player && x.roleId == baseRoleId);
        allRoles.RemoveAll(x => x.player == player && x.roleId == baseRoleId);
    }

    public static void swapRole(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            players[index].player = p2;
        }
    }
}

public static class RoleHelpers
{
    public static bool isRole(this PlayerControl player, RoleId role)
    {
        foreach (var (id, type) in RoleData.ALL_ROLE_TYPES)
        {
            if (role == id)
            {
                return (bool)type.GetMethod("isRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
            }
        }

        switch (role)
        {
            case RoleId.Godfather:
                return Mafia.Godfather.godfather == player;
            case RoleId.Mafioso:
                return Mafia.Mafioso.mafioso == player;
            case RoleId.Janitor:
                return Mafia.Janitor.janitor == player;
            case RoleId.Shifter:
                return Shifter.shifter == player;
            case RoleId.Swapper:
                return Swapper.swapper == player;
            case RoleId.Morphing:
                return Morphing.morphing == player;
            case RoleId.Camouflager:
                return Camouflager.camouflager == player;
            case RoleId.Mini:
                return Mini.mini == player;
            case RoleId.Vampire:
                return Vampire.vampire == player;
            case RoleId.Snitch:
                return Snitch.snitch == player;
            case RoleId.Spy:
                return Spy.spy == player;
            case RoleId.Trickster:
                return Trickster.trickster == player;
            case RoleId.Cleaner:
                return Cleaner.cleaner == player;
            case RoleId.Warlock:
                return Warlock.warlock == player;
            case RoleId.SecurityGuard:
                return SecurityGuard.securityGuard == player;
            case RoleId.EvilGuesser:
                return Guesser.evilGuesser == player;
            case RoleId.NiceGuesser:
                return Guesser.niceGuesser == player;
            case RoleId.BountyHunter:
                return BountyHunter.bountyHunter == player;
            case RoleId.Vulture:
                return Vulture.vulture == player;
            case RoleId.Medium:
                return Medium.medium == player;
            case RoleId.Witch:
                return Witch.witch == player;
            case RoleId.Lawyer:
                return Lawyer.lawyer == player;
            case RoleId.Pursuer:
                return Pursuer.pursuer == player;
            default:
                Instance.Logger.LogError($"isRole: no method found for role type {role}");
                break;
        }

        return false;
    }

    public static void setRole(this PlayerControl player, RoleId role)
    {
        foreach (var (id, type) in RoleData.ALL_ROLE_TYPES)
        {
            if (role == id)
            {
                type.GetMethod("setRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
                return;
            }
        }

        switch (role)
        {
            case RoleId.Godfather:
                Mafia.Godfather.godfather = player;
                break;
            case RoleId.Mafioso:
                Mafia.Mafioso.mafioso = player;
                break;
            case RoleId.Janitor:
                Mafia.Janitor.janitor = player;
                break;
            case RoleId.Shifter:
                Shifter.shifter = player;
                break;
            case RoleId.Swapper:
                Swapper.swapper = player;
                break;
            case RoleId.Morphing:
                Morphing.morphing = player;
                break;
            case RoleId.Camouflager:
                Camouflager.camouflager = player;
                break;
            case RoleId.Mini:
                Mini.mini = player;
                break;
            case RoleId.Vampire:
                Vampire.vampire = player;
                break;
            case RoleId.Snitch:
                Snitch.snitch = player;
                break;
            case RoleId.Spy:
                Spy.spy = player;
                break;
            case RoleId.Trickster:
                Trickster.trickster = player;
                break;
            case RoleId.Cleaner:
                Cleaner.cleaner = player;
                break;
            case RoleId.Warlock:
                Warlock.warlock = player;
                break;
            case RoleId.SecurityGuard:
                SecurityGuard.securityGuard = player;
                break;
            case RoleId.EvilGuesser:
                Guesser.evilGuesser = player;
                break;
            case RoleId.NiceGuesser:
                Guesser.niceGuesser = player;
                break;
            case RoleId.BountyHunter:
                BountyHunter.bountyHunter = player;
                break;
            case RoleId.Vulture:
                Vulture.vulture = player;
                break;
            case RoleId.Medium:
                Medium.medium = player;
                break;
            case RoleId.Witch:
                Witch.witch = player;
                break;
            case RoleId.Lawyer:
                Lawyer.lawyer = player;
                break;
            case RoleId.Pursuer:
                Pursuer.pursuer = player;
                break;
            default:
                Instance.Logger.LogError($"setRole: no method found for role type {role}");
                return;
        }
    }

    public static void eraseRole(this PlayerControl player, RoleId role)
    {
        if (isRole(player, role))
        {
            foreach (var (id, type) in RoleData.ALL_ROLE_TYPES)
            {
                if (role == id)
                {
                    type.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
                    return;
                }
            }
            Instance.Logger.LogError($"eraseRole: no method found for role type {role}");
        }
    }

    public static void eraseAllRoles(this PlayerControl player)
    {
        foreach (var (_, type) in RoleData.ALL_ROLE_TYPES)
        {
            type.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
        }

        // Crewmate roles
        if (player.isRole(RoleId.Shifter)) Shifter.clearAndReload();
        if (player.isRole(RoleId.Snitch)) Snitch.clearAndReload();
        if (player.isRole(RoleId.Swapper)) Swapper.clearAndReload();
        if (player.isRole(RoleId.Spy)) Spy.clearAndReload();
        if (player.isRole(RoleId.SecurityGuard)) SecurityGuard.clearAndReload();
        if (player.isRole(RoleId.Medium)) Medium.clearAndReload();
        if (player.isRole(RoleId.Trapper)) Trapper.clearAndReload();

        // Impostor roles
        if (player.isRole(RoleId.Morphing)) Morphing.clearAndReload();
        if (player.isRole(RoleId.Camouflager)) Camouflager.clearAndReload();
        if (player.isRole(RoleId.Godfather)) Mafia.Godfather.clearAndReload();
        if (player.isRole(RoleId.Mafioso)) Mafia.Mafioso.clearAndReload();
        if (player.isRole(RoleId.Janitor)) Mafia.Janitor.clearAndReload();
        if (player.isRole(RoleId.Vampire)) Vampire.clearAndReload();
        if (player.isRole(RoleId.Trickster)) Trickster.clearAndReload();
        if (player.isRole(RoleId.Cleaner)) Cleaner.clearAndReload();
        if (player.isRole(RoleId.Warlock)) Warlock.clearAndReload();
        if (player.isRole(RoleId.Witch)) Witch.clearAndReload();
        if (player.isRole(RoleId.Ninja)) Ninja.clearAndReload();
        if (player.isRole(RoleId.Bomber)) Bomber.clearAndReload();
        if (player.isRole(RoleId.Yoyo)) Yoyo.clearAndReload();

        // Other roles
        if (Guesser.isGuesser(player.PlayerId)) Guesser.clear(player.PlayerId);
        if (player.isRole(RoleId.Jackal))
        {
            // Promote Sidekick and hence override the the Jackal or erase Jackal
            if (TeamJackal.Sidekick.promotesToJackal && TeamJackal.Sidekick.sidekick != null && !TeamJackal.Sidekick.sidekick.isDead())
            {
                RPCProcedure.sidekickPromotes();
            }
            else
            {
                TeamJackal.Jackal.ClearAndReload();
            }
        }
        if (player.isRole(RoleId.Sidekick)) TeamJackal.Sidekick.ClearAndReload();
        if (player == BountyHunter.bountyHunter) BountyHunter.clearAndReload();
        if (player == Vulture.vulture) Vulture.clearAndReload();
        if (player == Lawyer.lawyer) Lawyer.clearAndReload();
        if (player == Pursuer.pursuer) Pursuer.clearAndReload();
        if (player == Thief.thief) Thief.clearAndReload();
    }

    public static void swapRoles(this PlayerControl player, PlayerControl target)
    {
        foreach (var (id, type) in RoleData.ALL_ROLE_TYPES)
        {
            if (player.isRole(id))
            {
                type.GetMethod("swapRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player, target]);
            }
        }

        if (player.isRole(RoleId.Swapper)) Swapper.swapper = target;
        if (player.isRole(RoleId.Snitch)) Snitch.snitch = target;
        if (player.isRole(RoleId.Spy)) Spy.spy = target;
        if (player.isRole(RoleId.SecurityGuard)) SecurityGuard.securityGuard = target;
        if (player.isRole(RoleId.Medium)) Medium.medium = target;
        if (player.isRole(RoleId.Godfather)) Mafia.Godfather.godfather = target;
        if (player.isRole(RoleId.Mafioso)) Mafia.Mafioso.mafioso = target;
        if (player.isRole(RoleId.Janitor)) Mafia.Janitor.janitor = target;
        if (player.isRole(RoleId.Morphing)) Morphing.morphing = target;
        if (player.isRole(RoleId.Camouflager)) Camouflager.camouflager = target;
        if (player.isRole(RoleId.Vampire)) Vampire.vampire = target;
        if (player.isRole(RoleId.Trickster)) Trickster.trickster = target;
        if (player.isRole(RoleId.Cleaner)) Cleaner.cleaner = target;
        if (player.isRole(RoleId.Warlock)) Warlock.warlock = target;
        if (player.isRole(RoleId.BountyHunter)) BountyHunter.bountyHunter = target;
        if (player.isRole(RoleId.Witch)) Witch.witch = target;
        if (player.isRole(RoleId.Mini)) Mini.mini = target;
        if (player.isRole(RoleId.EvilGuesser)) Guesser.evilGuesser = target;
        if (player.isRole(RoleId.NiceGuesser)) Guesser.niceGuesser = target;
        if (player.isRole(RoleId.Jackal)) TeamJackal.Jackal.jackal = target;
        if (player.isRole(RoleId.Sidekick)) TeamJackal.Sidekick.sidekick = target;
        if (player.isRole(RoleId.Vulture)) Vulture.vulture = target;
        if (player.isRole(RoleId.Lawyer)) Lawyer.lawyer = target;
        if (player.isRole(RoleId.Pursuer)) Pursuer.pursuer = target;
    }

    public static string modifyNameText(this PlayerControl player, string nameText)
    {
        if (player == null || player.Data.Disconnected) return nameText;

        foreach (var role in Role.allRoles)
        {
            if (role.player == player)
            {
                nameText = role.modifyNameText(nameText);
            }
        }

        foreach (var mod in Modifier.allModifiers)
        {
            if (mod.player == player)
            {
                nameText = mod.modifyNameText(nameText);
            }
        }

        nameText += Lovers.getIcon(player);

        return nameText;
    }

    public static string modifyRoleText(this PlayerControl player, string roleText, List<RoleInfo> roleInfo, bool useColors = true, bool includeHidden = false)
    {
        foreach (var mod in Modifier.allModifiers)
        {
            if (mod.player == player)
            {
                roleText = mod.modifyRoleText(roleText, roleInfo, useColors, includeHidden);
            }
        }
        return roleText;
    }

    public static string meetingInfoText(this PlayerControl player)
    {
        var text = "";
        var lines = new StringBuilder();
        foreach (var role in Role.allRoles.Where(x => x.player == player))
        {
            text = role.meetingInfoText();
            if (text != "") lines.AppendLine(text);
        }

        foreach (var mod in Modifier.allModifiers.Where(x => x.player == player))
        {
            text = mod.meetingInfoText();
            if (text != "") lines.AppendLine(text);
        }

        if (player.isRole(RoleId.Swapper) && Swapper.numSwaps > 0 && player.isAlive())
        {
            text = string.Format(Tr.Get("swapperSwapsLeft"), Swapper.numSwaps);
            if (text != "") lines.AppendLine(text);
        }

        var numGuesses = Guesser.remainingShots(player.PlayerId);
        if (Guesser.isGuesser(player.PlayerId) && player.isAlive() && numGuesses > 0)
        {
            text = string.Format(Tr.Get("guesserGuessesLeft"), numGuesses);
            if (text != "") lines.AppendLine(text);
        }

        if (player.isRole(RoleId.Shifter) && Shifter.futureShift != null)
        {
            text = string.Format(Tr.Get("shifterTargetInfo"), Shifter.futureShift.Data.PlayerName);
            if (text != "") lines.AppendLine(text);
        }

        return lines.ToString();
    }

    public static void OnKill(this PlayerControl player, PlayerControl target)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnKill(target));
        Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnKill(target));
    }

    public static void OnDeath(this PlayerControl player, PlayerControl killer)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnDeath(killer));
        Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnDeath(killer));

        // Lover suicide trigger on exile/death
        if (player.isLovers())
        {
            Lovers.killLovers(player, killer);
        }

        RPCProcedure.updateMeeting(player.PlayerId, true);
    }
}