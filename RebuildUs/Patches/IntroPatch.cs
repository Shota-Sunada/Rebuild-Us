using HarmonyLib;
using System;
using static RebuildUs.RebuildUs;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RebuildUs.Patches;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
class IntroCutsceneOnDestroyPatch
{
    public static PoolablePlayer playerPrefab;
    public static Vector3 bottomLeft;
    public static void Prefix(IntroCutscene __instance)
    {
        // Generate and initialize player icons
        int playerCounter = 0;
        if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
        {
            float aspect = Camera.main.aspect;
            float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
            float xpos = 1.75f - safeOrthographicSize * aspect * 1.70f;
            float ypos = 0.15f - safeOrthographicSize * 1.7f;
            bottomLeft = new Vector3(xpos / 2, ypos / 2, -61f);

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                NetworkedPlayerInfo data = p.Data;
                PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                playerPrefab = __instance.PlayerPrefab;
                p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                // PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                player.cosmetics.nameText.text = data.PlayerName;
                player.SetFlipX(true);
                MapOptions.playerIcons[p.PlayerId] = player;
                player.gameObject.SetActive(false);

                if (PlayerControl.LocalPlayer.isRole(RoleId.Arsonist) && !p.isRole(RoleId.Arsonist))
                {
                    player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                    player.transform.localScale = Vector3.one * 0.2f;
                    player.setSemiTransparent(true);
                    player.gameObject.SetActive(true);
                }
                else
                {   //  This can be done for all players not just for the bounty hunter as it was before. Allows the thief to have the correct position and scaling
                    player.transform.localPosition = bottomLeft;
                    player.transform.localScale = Vector3.one * 0.4f;
                    player.gameObject.SetActive(false);
                }
            }
        }

        // Force Bounty Hunter to load a new Bounty when the Intro is over
        if (BountyHunter.bounty != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter)
        {
            BountyHunter.bountyUpdateTimer = 0f;
            if (FastDestroyableSingleton<HudManager>.Instance != null)
            {
                BountyHunter.cooldownText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                BountyHunter.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -0.35f, -62f);
                BountyHunter.cooldownText.transform.localScale = Vector3.one * 0.4f;
                BountyHunter.cooldownText.gameObject.SetActive(true);
            }
        }

        // First kill
        if (AmongUsClient.Instance.AmHost && MapOptions.shieldFirstKill && MapOptions.firstKillName != "")
        {
            PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(MapOptions.firstKillName));
            if (target != null)
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.SetFirstKill);
                writer.Write(target.PlayerId);
                RPCProcedure.setFirstKill(target.PlayerId);
            }
        }
        MapOptions.firstKillName = "";

        EventUtility.gameStartsUpdate();
    }
}

[HarmonyPatch]
class IntroPatch
{
    public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        // Intro solo teams
        if (Helpers.isNeutral(PlayerControl.LocalPlayer))
        {
            var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            soloTeam.Add(PlayerControl.LocalPlayer);
            yourTeam = soloTeam;
        }

        // Add the Spy to the Impostor team (for the Impostors)
        if (Spy.spy != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            List<PlayerControl> players = [.. PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid())];
            var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
            fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
            foreach (PlayerControl p in players)
            {
                if (PlayerControl.LocalPlayer != p && (p == Spy.spy || p.Data.Role.IsImpostor))
                    fakeImpostorTeam.Add(p);
            }
            yourTeam = fakeImpostorTeam;
        }

        // Role draft: If spy is enabled, don't show the team
        if (CustomOptionHolder.spySpawnRate.getSelection() > 0 && PlayerControl.AllPlayerControls.ToArray().ToList().Where(x => x.Data.Role.IsImpostor).Count() > 1)
        {
            var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
            fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
            yourTeam = fakeImpostorTeam;
        }
    }

    public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
        var roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
        var neutralColor = new Color32(76, 84, 78, 255);
        if (roleInfo == null || roleInfo == RoleInfo.crewmate)
        {
            return;
        }

        if (roleInfo.roleType is RoleType.Neutral)
        {
            __instance.BackgroundBar.material.color = neutralColor;
            __instance.TeamTitle.text = "Neutral";
            __instance.TeamTitle.color = neutralColor;
        }
    }

    public static IEnumerator<WaitForSeconds> EndShowRole(IntroCutscene __instance)
    {
        yield return new WaitForSeconds(5f);
        __instance.YouAreText.gameObject.SetActive(false);
        __instance.RoleText.gameObject.SetActive(false);
        __instance.RoleBlurbText.gameObject.SetActive(false);
        __instance.ourCrewmate.gameObject.SetActive(false);

    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static void Postfix(IntroCutscene __instance, bool impostorPositioning, ref PoolablePlayer __result)
        {
            if (impostorPositioning) __result.SetNameColor(Palette.ImpostorRed);
        }
    }


    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    class SetUpRoleTextPatch
    {
        static int seed = 0;
        static public void SetRoleTexts(IntroCutscene __instance)
        {
            // Don't override the intro of the vanilla roles
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
            RoleInfo modifierInfo = infos.Where(info => info.isModifier).FirstOrDefault();

            if (EventUtility.isEnabled)
            {
                var roleInfos = RoleInfo.allRoleInfos.Where(x => !x.isModifier).ToList();
                if (roleInfo.roleType is RoleType.Neutral) roleInfos.RemoveAll(x => x.roleType is not RoleType.Neutral);
                if (roleInfo.color == Palette.ImpostorRed) roleInfos.RemoveAll(x => x.color != Palette.ImpostorRed);
                if (roleInfo.roleType is not RoleType.Neutral && roleInfo.color != Palette.ImpostorRed) roleInfos.RemoveAll(x => x.color == Palette.ImpostorRed || x.roleType is RoleType.Neutral);
                var rnd = new System.Random(seed);
                roleInfo = roleInfos[rnd.Next(roleInfos.Count)];
            }

            __instance.RoleBlurbText.text = "";
            if (roleInfo != null)
            {
                __instance.RoleText.text = roleInfo.name;
                __instance.RoleText.color = roleInfo.color;
                __instance.RoleBlurbText.text = roleInfo.introDescription;
                __instance.RoleBlurbText.color = roleInfo.color;
            }
            if (modifierInfo != null)
            {
                if (PlayerControl.LocalPlayer.isLovers())
                {
                    var partner = Lovers.getPartner(PlayerControl.LocalPlayer);
                    __instance.RoleBlurbText.text += Helpers.cs(Lovers.Color, $"\n♥ You are in love with {partner?.Data?.PlayerName ?? ""} ♥");
                }
            }
        }
        public static bool Prefix(IntroCutscene __instance)
        {
            seed = rnd.Next(5000);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
            {
                SetRoleTexts(__instance);
            })));
            return true;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            setupIntroTeamIcons(__instance, ref teamToDisplay);
        }

        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            setupIntroTeam(__instance, ref teamToDisplay);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class BeginImpostorPatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            setupIntroTeamIcons(__instance, ref yourTeam);
        }

        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            setupIntroTeam(__instance, ref yourTeam);
        }
    }
}

/* Horses are broken since 2024.3.5 - keeping this code in case they return.
 * [HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldHorseAround))]
public static class ShouldAlwaysHorseAround {
    public static bool Prefix(ref bool __result) {
        __result = EventUtility.isEnabled && !EventUtility.disableEventMode;
        return false;
    }
}*/

[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldShowAprilFoolsToggle))]
public static class ShouldShowAprilFoolsToggle
{
    public static void Postfix(ref bool __result)
    {
        __result = __result || EventUtility.isEventDate || EventUtility.canBeEnabled;  // Extend it to a 7 day window instead of just 1st day of the Month
    }
}