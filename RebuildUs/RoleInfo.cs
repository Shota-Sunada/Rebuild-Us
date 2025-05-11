using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http;
using RebuildUs.Localization;

namespace RebuildUs;

public class RoleInfo
{
    public Color color;
    public string name => Tr.Get(nameKey);
    public string nameColored => Helpers.cs(color, name);
    public string introDescription => Tr.Get($"{nameKey}IntroDesc");
    public string shortDescription => Tr.Get($"{nameKey}ShortDesc");
    public string fullDescription => Tr.Get($"{nameKey}FullDesc");
    public string roleOptions => GameOptionsDataPatch.optionsToString(baseOption, true);
    public bool enabled => Helpers.RolesEnabled && (this == crewmate || this == impostor || (baseOption != null && baseOption.Enabled));

    public RoleId roleId;
    public RoleType roleType;
    public bool isModifier = false;

    private string nameKey;
    private CustomOption baseOption;

    public RoleInfo(string nameKey, Color color, CustomOption baseOption, RoleId roleId, RoleType roleType)
    {
        this.color = color;
        this.nameKey = nameKey;
        this.roleId = roleId;
        this.roleType = roleType;
        this.baseOption = baseOption;
    }

    public RoleInfo(string nameKey, Color color, CustomOption baseOption, RoleId roleId, bool isModifier)
    {
        this.color = color;
        this.nameKey = nameKey;
        this.roleId = roleId;
        roleType = RoleType.NONE;
        this.isModifier = isModifier;
        this.baseOption = baseOption;
    }

    public static RoleInfo jester = new("Jester", Jester.Color, CustomOptionHolder.jesterSpawnRate, RoleId.Jester, RoleType.Neutral);
    public static RoleInfo mayor = new("Mayor", Mayor.Color, CustomOptionHolder.mayorSpawnRate, RoleId.Mayor, RoleType.Crewmate);
    public static RoleInfo portalmaker = new("Portalmaker", Portalmaker.Color, CustomOptionHolder.portalmakerSpawnRate, RoleId.Portalmaker, RoleType.Crewmate);
    public static RoleInfo engineer = new("Engineer", Engineer.Color, CustomOptionHolder.engineerSpawnRate, RoleId.Engineer, RoleType.Crewmate);
    public static RoleInfo sheriff = new("Sheriff", Sheriff.Color, CustomOptionHolder.sheriffSpawnRate, RoleId.Sheriff, RoleType.Crewmate);
    public static RoleInfo lighter = new("Lighter", Lighter.Color, CustomOptionHolder.lighterSpawnRate, RoleId.Lighter, RoleType.Crewmate);
    public static RoleInfo godfather = new("Godfather", Mafia.Color, CustomOptionHolder.mafiaSpawnRate, RoleId.Godfather, RoleType.Impostor);
    public static RoleInfo mafioso = new("Mafioso", Mafia.Color, CustomOptionHolder.mafiaSpawnRate, RoleId.Mafioso, RoleType.Impostor);
    public static RoleInfo janitor = new("Janitor", Mafia.Color, CustomOptionHolder.mafiaSpawnRate, RoleId.Janitor, RoleType.Impostor);
    public static RoleInfo morphing = new("Morphing", Morphing.color, CustomOptionHolder.morphingSpawnRate, RoleId.Morphing, RoleType.Impostor);
    public static RoleInfo camouflager = new("Camouflager", Camouflager.color, CustomOptionHolder.camouflagerSpawnRate, RoleId.Camouflager, RoleType.Impostor);
    public static RoleInfo vampire = new("Vampire", Vampire.color, CustomOptionHolder.vampireSpawnRate, RoleId.Vampire, RoleType.Impostor);
    public static RoleInfo eraser = new("Eraser", Eraser.Color, CustomOptionHolder.eraserSpawnRate, RoleId.Eraser, RoleType.Impostor);
    public static RoleInfo trickster = new("Trickster", Trickster.Color, CustomOptionHolder.tricksterSpawnRate, RoleId.Trickster, RoleType.Impostor);
    public static RoleInfo cleaner = new("Cleaner", Cleaner.color, CustomOptionHolder.cleanerSpawnRate, RoleId.Cleaner, RoleType.Impostor);
    public static RoleInfo warlock = new("Warlock", Warlock.color, CustomOptionHolder.warlockSpawnRate, RoleId.Warlock, RoleType.Impostor);
    public static RoleInfo bountyHunter = new("Bounty Hunter", BountyHunter.color, CustomOptionHolder.bountyHunterSpawnRate, RoleId.BountyHunter, RoleType.Impostor);
    public static RoleInfo detective = new("Detective", Detective.Color, CustomOptionHolder.detectiveSpawnRate, RoleId.Detective, RoleType.Crewmate);
    public static RoleInfo timeMaster = new("Time Master", TimeMaster.Color, CustomOptionHolder.timeMasterSpawnRate, RoleId.TimeMaster, RoleType.Crewmate);
    public static RoleInfo medic = new("Medic", Medic.Color, CustomOptionHolder.medicSpawnRate, RoleId.Medic, RoleType.Crewmate);
    public static RoleInfo niceSwapper = new("NiceSwapper", Swapper.Color, CustomOptionHolder.swapperSpawnRate, RoleId.Swapper, RoleType.Crewmate);
    public static RoleInfo evilSwapper = new("EvilSwapper", Palette.ImpostorRed, CustomOptionHolder.swapperSpawnRate, RoleId.Swapper, RoleType.Impostor);
    public static RoleInfo seer = new("Seer", Seer.Color, CustomOptionHolder.seerSpawnRate, RoleId.Seer, RoleType.Crewmate);
    public static RoleInfo hacker = new("Hacker", Hacker.Color, CustomOptionHolder.hackerSpawnRate, RoleId.Hacker, RoleType.Crewmate);
    public static RoleInfo tracker = new("Tracker", Tracker.Color, CustomOptionHolder.trackerSpawnRate, RoleId.Tracker, RoleType.Crewmate);
    public static RoleInfo snitch = new("Snitch", Snitch.color, CustomOptionHolder.snitchSpawnRate, RoleId.Snitch, RoleType.Crewmate);
    public static RoleInfo jackal = new("Jackal", TeamJackal.Color, CustomOptionHolder.jackalSpawnRate, RoleId.Jackal, RoleType.Neutral);
    public static RoleInfo sidekick = new("Sidekick", TeamJackal.Color, CustomOptionHolder.jackalSpawnRate, RoleId.Sidekick, RoleType.Neutral);
    public static RoleInfo spy = new("Spy", Spy.color, CustomOptionHolder.spySpawnRate, RoleId.Spy, RoleType.Crewmate);
    public static RoleInfo securityGuard = new("Security Guard", SecurityGuard.color, CustomOptionHolder.securityGuardSpawnRate, RoleId.SecurityGuard, RoleType.Crewmate);
    public static RoleInfo arsonist = new("Arsonist", Arsonist.Color, CustomOptionHolder.arsonistSpawnRate, RoleId.Arsonist, RoleType.Neutral);
    public static RoleInfo goodGuesser = new("Nice Guesser", Guesser.color, CustomOptionHolder.guesserSpawnRate, RoleId.NiceGuesser, RoleType.Crewmate);
    public static RoleInfo badGuesser = new("Evil Guesser", Palette.ImpostorRed, CustomOptionHolder.guesserSpawnRate, RoleId.EvilGuesser, RoleType.Impostor);
    public static RoleInfo vulture = new("Vulture", Vulture.color, CustomOptionHolder.vultureSpawnRate, RoleId.Vulture, RoleType.Neutral);
    public static RoleInfo medium = new("Medium", Medium.color, CustomOptionHolder.mediumSpawnRate, RoleId.Medium, RoleType.Crewmate);
    public static RoleInfo trapper = new("Trapper", Trapper.color, CustomOptionHolder.trapperSpawnRate, RoleId.Trapper, RoleType.Crewmate);
    public static RoleInfo lawyer = new("Lawyer", Lawyer.color, CustomOptionHolder.lawyerSpawnRate, RoleId.Lawyer, RoleType.Neutral);
    public static RoleInfo prosecutor = new("Prosecutor", Lawyer.color, CustomOptionHolder.lawyerSpawnRate, RoleId.Prosecutor, RoleType.Neutral);
    public static RoleInfo pursuer = new("Pursuer", Pursuer.color, CustomOptionHolder.lawyerSpawnRate, RoleId.Pursuer, false);
    public static RoleInfo impostor = new("Impostor", Palette.ImpostorRed, null, RoleId.Impostor, RoleType.Impostor);
    public static RoleInfo crewmate = new("Crewmate", Palette.CrewmateBlue, null, RoleId.Crewmate, RoleType.Crewmate);
    public static RoleInfo witch = new("Witch", Witch.color, CustomOptionHolder.witchSpawnRate, RoleId.Witch, RoleType.Impostor);
    public static RoleInfo ninja = new("Ninja", Ninja.color, CustomOptionHolder.ninjaSpawnRate, RoleId.Ninja, RoleType.Impostor);
    public static RoleInfo thief = new("Thief", Thief.color, CustomOptionHolder.thiefSpawnRate, RoleId.Thief, RoleType.Neutral);
    public static RoleInfo bomber = new("Bomber", Bomber.color, CustomOptionHolder.bomberSpawnRate, RoleId.Bomber, RoleType.Impostor);
    public static RoleInfo yoyo = new("Yo-Yo", Yoyo.color, CustomOptionHolder.yoyoSpawnRate, RoleId.Yoyo, RoleType.Impostor);
    public static RoleInfo shifter = new("Shifter", Shifter.Color, CustomOptionHolder.shifterSpawnRate, RoleId.Shifter, RoleType.Crewmate);
    public static RoleInfo mini = new("Mini", Color.yellow, CustomOptionHolder.modifierMini, RoleId.Mini, false);

    public static List<RoleInfo> allRoleInfos = [
        impostor,
        godfather,
        mafioso,
        janitor,
        morphing,
        camouflager,
        vampire,
        eraser,
        trickster,
        cleaner,
        warlock,
        bountyHunter,
        witch,
        ninja,
        bomber,
        yoyo,
        goodGuesser,
        badGuesser,
        jester,
        arsonist,
        jackal,
        sidekick,
        vulture,
        pursuer,
        lawyer,
        thief,
        prosecutor,
        crewmate,
        mayor,
        portalmaker,
        engineer,
        sheriff,
        lighter,
        detective,
        timeMaster,
        medic,
        niceSwapper,
        evilSwapper,
        seer,
        hacker,
        tracker,
        snitch,
        spy,
        securityGuard,
        medium,
        trapper,
        mini,
        shifter
    ];

    public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p)
    {
        List<RoleInfo> infos = [];
        if (p == null) return infos;

        // Special roles
        if (p.isRole(RoleId.Jester)) infos.Add(jester);
        if (p.isRole(RoleId.Mayor)) infos.Add(mayor);
        if (p.isRole(RoleId.Portalmaker)) infos.Add(portalmaker);
        if (p.isRole(RoleId.Engineer)) infos.Add(engineer);
        if (p.isRole(RoleId.Sheriff)) infos.Add(sheriff);
        if (p.isRole(RoleId.Lighter)) infos.Add(lighter);
        if (p.isRole(RoleId.Godfather)) infos.Add(godfather);
        if (p.isRole(RoleId.Mafioso)) infos.Add(mafioso);
        if (p.isRole(RoleId.Janitor)) infos.Add(janitor);
        if (p == Morphing.morphing) infos.Add(morphing);
        if (p == Camouflager.camouflager) infos.Add(camouflager);
        if (p == Vampire.vampire) infos.Add(vampire);
        if (p.isRole(RoleId.Eraser)) infos.Add(eraser);
        if (p == Trickster.trickster) infos.Add(trickster);
        if (p == Cleaner.cleaner) infos.Add(cleaner);
        if (p == Warlock.warlock) infos.Add(warlock);
        if (p == Witch.witch) infos.Add(witch);
        if (p == Ninja.ninja) infos.Add(ninja);
        if (p == Bomber.bomber) infos.Add(bomber);
        if (p == Yoyo.yoyo) infos.Add(yoyo);
        if (p.isRole(RoleId.Detective)) infos.Add(detective);
        if (p.isRole(RoleId.TimeMaster)) infos.Add(timeMaster);
        if (p.isRole(RoleId.Medic)) infos.Add(medic);
        if (p.isRole(RoleId.Swapper)) infos.Add(p.isImpostor() ? evilSwapper : niceSwapper);
        if (p.isRole(RoleId.Seer)) infos.Add(seer);
        if (p.isRole(RoleId.Hacker)) infos.Add(hacker);
        if (p.isRole(RoleId.Tracker)) infos.Add(tracker);
        if (p == Snitch.snitch) infos.Add(snitch);
        if (p.isRole(RoleId.Jackal) || (TeamJackal.formerJackals != null && TeamJackal.formerJackals.Any(x => x.PlayerId == p.PlayerId))) infos.Add(jackal);
        if (p.isRole(RoleId.Sidekick)) infos.Add(sidekick);
        if (p == Spy.spy) infos.Add(spy);
        if (p == SecurityGuard.securityGuard) infos.Add(securityGuard);
        if (p.isRole(RoleId.Arsonist)) infos.Add(arsonist);
        if (p == Guesser.niceGuesser) infos.Add(goodGuesser);
        if (p == Guesser.evilGuesser) infos.Add(badGuesser);
        if (p == BountyHunter.bountyHunter) infos.Add(bountyHunter);
        if (p == Vulture.vulture) infos.Add(vulture);
        if (p == Medium.medium) infos.Add(medium);
        if (p == Lawyer.lawyer && !Lawyer.isProsecutor) infos.Add(lawyer);
        if (p == Lawyer.lawyer && Lawyer.isProsecutor) infos.Add(prosecutor);
        if (p == Trapper.trapper) infos.Add(trapper);
        if (p == Pursuer.pursuer) infos.Add(pursuer);
        if (p == Thief.thief) infos.Add(thief);

        if (infos.Count == 0)
        {
            if (p.isImpostor())
            {
                infos.Add(impostor);
            }
            else
            {
                infos.Add(crewmate);
            }
        }

        return infos;
    }

    public static string GetRolesString(PlayerControl p, bool useColors)
    {
        if (p?.Data?.Disconnected != false) return "";

        var roleInfo = getRoleInfoForPlayer(p);
        var roleText = string.Join(" ", [.. roleInfo.Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name)]);
        if (Lawyer.target != null && p?.PlayerId == Lawyer.target.PlayerId && PlayerControl.LocalPlayer != Lawyer.target)
        {
            roleText += useColors ? Helpers.cs(Pursuer.color, " §") : " §";
        }
        roleText = p.modifyRoleText(roleText, roleInfo, useColors);

        // if (!suppressGhostInfo && p != null)
        // {
        //     if (p == Shifter.shifter && (PlayerControl.LocalPlayer == Shifter.shifter || Helpers.shouldShowGhostInfo()) && Shifter.futureShift != null)
        //         roleName += Helpers.cs(Color.yellow, " ← " + Shifter.futureShift.Data.PlayerName);
        //     if (p == Vulture.vulture && (PlayerControl.LocalPlayer == Vulture.vulture || Helpers.shouldShowGhostInfo()))
        //         roleName = roleName + Helpers.cs(Vulture.color, $" ({Vulture.vultureNumberToWin - Vulture.eatenBodies} left)");
        //     if (Helpers.shouldShowGhostInfo())
        //     {
        //         if (Eraser.futureErased.Contains(p))
        //             roleName = Helpers.cs(Color.gray, "(erased) ") + roleName;
        //         if (Vampire.vampire != null && !Vampire.vampire.Data.IsDead && Vampire.bitten == p && !p.Data.IsDead)
        //             roleName = Helpers.cs(Vampire.color, $"(bitten {(int)HudManagerStartPatch.vampireKillButton.Timer + 1}) ") + roleName;
        //         if (p == Warlock.curseVictim)
        //             roleName = Helpers.cs(Warlock.color, "(cursed) ") + roleName;
        //         if (p == Ninja.ninjaMarked)
        //             roleName = Helpers.cs(Ninja.color, "(marked) ") + roleName;
        //         if (Pursuer.blankedList.Contains(p) && !p.Data.IsDead)
        //             roleName = Helpers.cs(Pursuer.color, "(blanked) ") + roleName;
        //         if (Witch.futureSpelled.Contains(p) && !MeetingHud.Instance) // This is already displayed in meetings!
        //             roleName = Helpers.cs(Witch.color, "☆ ") + roleName;
        //         if (BountyHunter.bounty == p)
        //             roleName = Helpers.cs(BountyHunter.color, "(bounty) ") + roleName;
        //         if (Arsonist.dousedPlayers.Contains(p))
        //             roleName = Helpers.cs(Arsonist.Color, "♨ ") + roleName;
        //         if (p.isRole(RoleId.Arsonist))
        //             roleName += Helpers.cs(Arsonist.Color, $" ({PlayerControl.AllPlayerControls.ToArray().Count(x => { return !x.isRole(RoleId.Arsonist) && !x.isDead() && !Arsonist.dousedPlayers.Any(y => y.PlayerId == x.PlayerId); })} left)");
        //         if (p == TeamJackal.Jackal.fakeSidekick)
        //             roleName = Helpers.cs(TeamJackal.Color, $" (fake SK)") + roleName;

        //         // Death Reason on Ghosts
        //         if (p.Data.IsDead)
        //         {
        //             string deathReasonString = "";
        //             var deadPlayer = GameHistory.deadPlayers.FirstOrDefault(x => x.player.PlayerId == p.PlayerId);

        //             Color killerColor = new();
        //             if (deadPlayer != null && deadPlayer.killerIfExisting != null)
        //             {
        //                 killerColor = RoleInfo.getRoleInfoForPlayer(deadPlayer.killerIfExisting, false).FirstOrDefault().color;
        //             }

        //             if (deadPlayer != null)
        //             {
        //                 switch (deadPlayer.deathReason)
        //                 {
        //                     case CustomDeathReason.Disconnect:
        //                         deathReasonString = " - disconnected";
        //                         break;
        //                     case CustomDeathReason.Exile:
        //                         deathReasonString = " - voted out";
        //                         break;
        //                     case CustomDeathReason.Kill:
        //                         deathReasonString = $" - killed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
        //                         break;
        //                     case CustomDeathReason.Guess:
        //                         if (deadPlayer.killerIfExisting.Data.PlayerName == p.Data.PlayerName)
        //                             deathReasonString = $" - failed guess";
        //                         else
        //                             deathReasonString = $" - guessed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
        //                         break;
        //                     case CustomDeathReason.Shift:
        //                         deathReasonString = $" - {Helpers.cs(Color.yellow, "shifted")} {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
        //                         break;
        //                     case CustomDeathReason.WitchExile:
        //                         deathReasonString = $" - {Helpers.cs(Witch.color, "witched")} by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
        //                         break;
        //                     case CustomDeathReason.LoverSuicide:
        //                         deathReasonString = $" - {Helpers.cs(Lovers.Color, "lover died")}";
        //                         break;
        //                     case CustomDeathReason.LawyerSuicide:
        //                         deathReasonString = $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}";
        //                         break;
        //                     case CustomDeathReason.Bomb:
        //                         deathReasonString = $" - bombed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
        //                         break;
        //                     case CustomDeathReason.Arson:
        //                         deathReasonString = $" - burnt by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
        //                         break;
        //                 }
        //                 roleName = roleName + deathReasonString;
        //             }
        //         }
        //     }
        // }
        return roleText;
    }


    static string ReadmePage = "";
    public static async Task loadReadme()
    {
        if (ReadmePage == "")
        {
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync("https://raw.githubusercontent.com/RebuildUsAU/RebuildUs/main/README.md");
            response.EnsureSuccessStatusCode();
            string httpres = await response.Content.ReadAsStringAsync();
            ReadmePage = httpres;
        }
    }
    public static string GetRoleDescription(RoleInfo roleInfo)
    {
        while (ReadmePage == "")
        {
        }

        int index = ReadmePage.IndexOf($"## {roleInfo.name}");
        int endindex = ReadmePage.Substring(index).IndexOf("### Game Options");
        return ReadmePage.Substring(index, endindex);

    }
}