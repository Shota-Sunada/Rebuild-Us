using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using static RebuildUs.RebuildUs;
using HarmonyLib;
using Hazel;
using AmongUs.GameOptions;
using RebuildUs.Patches;
using static RebuildUs.GameHistory;
using RebuildUs.Localization;
using RebuildUs.Extensions;

namespace RebuildUs;

public static class Helpers
{
    public static bool ShowButtons => !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                    !MeetingHud.Instance &&
                    !ExileController.Instance;

    public static bool ShowMeetingText => MeetingHud.Instance != null && (MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Discussion);

    public static bool GameStarted => AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started;

    public static bool RolesEnabled => CustomOptionHolder.activateRoles.getBool();

    public static bool RefundVotes => CustomOptionHolder.refundVotesOnDeath.getBool();

    public static List<byte> generateTasks(int numCommon, int numShort, int numLong)
    {
        if (numCommon + numShort + numLong <= 0)
        {
            numShort = 1;
        }

        var tasks = new Il2CppSystem.Collections.Generic.List<byte>();
        var hashSet = new Il2CppSystem.Collections.Generic.HashSet<TaskTypes>();

        var commonTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
        foreach (var task in ShipStatus.Instance.CommonTasks.OrderBy(x => rnd.Next())) commonTasks.Add(task);

        var shortTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
        foreach (var task in ShipStatus.Instance.ShortTasks.OrderBy(x => rnd.Next())) shortTasks.Add(task);

        var longTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
        foreach (var task in ShipStatus.Instance.LongTasks.OrderBy(x => rnd.Next())) longTasks.Add(task);

        int start = 0;
        ShipStatus.Instance.AddTasksFromList(ref start, numCommon, tasks, hashSet, commonTasks);

        start = 0;
        ShipStatus.Instance.AddTasksFromList(ref start, numShort, tasks, hashSet, shortTasks);

        start = 0;
        ShipStatus.Instance.AddTasksFromList(ref start, numLong, tasks, hashSet, longTasks);

        return tasks.ToArray().ToList();
    }

    public static void generateAndAssignTasks(this PlayerControl player, int numCommon, int numShort, int numLong)
    {
        if (player == null) return;

        List<byte> taskTypeIds = generateTasks(numCommon, numShort, numLong);

        var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedSetTasks);
        writer.Write(player.PlayerId);
        writer.WriteBytesAndSize(taskTypeIds.ToArray());
        RPCProcedure.uncheckedSetTasks(player.PlayerId, [.. taskTypeIds]);
    }

    public static void setSkinWithAnim(PlayerPhysics playerPhysics, string SkinId, int ColorId)
    {
        SkinViewData nextSkin;
        try { nextSkin = ShipStatus.Instance.CosmeticsCache.GetSkin(SkinId); } catch { return; }
        var spriteAnim = playerPhysics.Animations.Animator;
        var anim = spriteAnim.m_animator;

        var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();
        AnimationClip clip;
        if (currentPhysicsAnim == playerPhysics.Animations.group.RunAnim)
        {
            clip = nextSkin.RunAnim;
        }
        else if (currentPhysicsAnim == playerPhysics.Animations.group.SpawnAnim)
        {
            clip = nextSkin.SpawnAnim;
        }
        else if (currentPhysicsAnim == playerPhysics.Animations.group.EnterVentAnim)
        {
            clip = nextSkin.EnterVentAnim;
        }
        else if (currentPhysicsAnim == playerPhysics.Animations.group.ExitVentAnim)
        {
            clip = nextSkin.ExitVentAnim;
        }
        else
        {
            clip = nextSkin.IdleAnim;
        }

        float progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        playerPhysics.myPlayer.RawSetSkin(SkinId, ColorId);

        spriteAnim.Play(clip, 1f);
        anim.Play("a", 0, progress % 1);
        anim.Update(0f);
    }

    public static string previousEndGameSummary = "";
    public static Dictionary<string, Sprite> CachedSprites = [];

    public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit, bool cache = true)
    {
        try
        {
            if (cache && CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = loadTextureFromResources(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            if (cache) sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            if (!cache) return sprite;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
        }
        return null;
    }

    public static unsafe Texture2D loadTextureFromResources(string path)
    {
        try
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var length = stream.Length;
            var byteTexture = new Il2CppStructArray<byte>(length);
            stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
            if (path.Contains("HorseHats"))
            {
                byteTexture = new Il2CppStructArray<byte>([.. byteTexture.Reverse()]);
            }
            ImageConversion.LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from resources: " + path);
        }
        return null;
    }

    public static Texture2D loadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            }
        }
        catch
        {
            RebuildUs.Instance.Logger.LogError("Error loading texture from disk: " + path);
        }
        return null;
    }

    /* This function has been removed from TOR because we switched to assetbundles for compressed audio. leaving it here for reference - Gendelo
    public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP") {

        // must be "raw (headerless) 2-channel signed 32 bit pcm (le) 48kHz" (can e.g. use Audacity® to export )
        try {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var byteAudio = new byte[stream.Length];
            _ = stream.Read(byteAudio, 0, (int)stream.Length);
            float[] samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
            int offset;
            for (int i = 0; i < samples.Length; i++) {
                offset = i * 4;
                samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / Int32.MaxValue;
            }
            int channels = 2;
            int sampleRate = 48000;
            AudioClip audioClip = AudioClip.Create(clipName, samples.Length / 2, channels, sampleRate, false);
            audioClip.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            audioClip.SetData(samples, 0);
            return audioClip;
        } catch {
            System.Console.WriteLine("Error loading AudioClip from resources: " + path);
        }
        return null;

        // Usage example:
        //AudioClip exampleClip = Helpers.loadAudioClipFromResources("RebuildUs.Resources.exampleClip.raw");
        //if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);

    }*/

    public static string readTextFromResources(string path)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Stream stream = assembly.GetManifestResourceStream(path);
        StreamReader textStreamReader = new(stream);
        return textStreamReader.ReadToEnd();
    }

    public static string readTextFromFile(string path)
    {
        Stream stream = File.OpenRead(path);
        StreamReader textStreamReader = new(stream);
        return textStreamReader.ReadToEnd();
    }

    public static PlayerControl playerById(byte id)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == id)
                return player;
        }

        return null;
    }

    public static Dictionary<byte, PlayerControl> allPlayersById()
    {
        Dictionary<byte, PlayerControl> res = [];
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            res.Add(player.PlayerId, player);
        return res;
    }

    public static void handleVampireBiteOnBodyReport()
    {
        // Murder the bitten player and reset bitten (regardless whether the kill was successful or not)
        checkMurderAttemptAndKill(Vampire.vampire, Vampire.bitten, true, false);
        using var writer = RPCProcedure.SendRPC(CustomRPC.VampireSetBitten);
        writer.Write(byte.MaxValue);
        writer.Write(byte.MaxValue);
        RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
    }

    public static void refreshRoleDescription(PlayerControl player)
    {
        if (player == null) return;

        var infos = RoleInfo.getRoleInfoForPlayer(player);
        var taskTexts = new List<string>(infos.Count);

        foreach (var roleInfo in infos)
        {
            taskTexts.Add(getRoleString(roleInfo));
        }

        var toRemove = new List<PlayerTask>();
        foreach (var t in player.myTasks.GetFastEnumerator())
        {
            var textTask = t.TryCast<ImportantTextTask>();
            if (textTask == null) continue;

            var currentText = textTask.Text;

            if (taskTexts.Contains(currentText)) taskTexts.Remove(currentText); // TextTask for this RoleInfo does not have to be added, as it already exists
            else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
        }

        foreach (PlayerTask t in toRemove)
        {
            t.OnRemove();
            player.myTasks.Remove(t);
            UnityEngine.Object.Destroy(t.gameObject);
        }

        // Add TextTask for remaining RoleInfos
        foreach (string title in taskTexts)
        {
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);
            task.Text = title;
            player.myTasks.Insert(0, task);
        }

        if (player.hasModifier(ModifierId.Madmate))
        {
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);
            task.Text = cs(Madmate.Color, $"{Tr.Get("Madmate")}: " + Tr.Get("madmateShortDesc"));
            player.myTasks.Insert(0, task);
        }
    }

    internal static string getRoleString(RoleInfo roleInfo)
    {
        if (roleInfo.name == "Jackal")
        {
            var getSidekickText = TeamJackal.Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
            return cs(roleInfo.color, $"{roleInfo.name}: Kill everyone{getSidekickText}");
        }

        if (roleInfo.name == "Invert")
        {
            return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription} ({Invert.meetings})");
        }

        return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");
    }

    public static bool isD(byte playerId)
    {
        return playerId % 2 == 0;
    }

    public static bool isLighterColor(PlayerControl target)
    {
        return isD(target.PlayerId);
    }

    public static bool isCustomServer()
    {
        if (FastDestroyableSingleton<ServerManager>.Instance == null) return false;
        StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
        return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
    }

    public static bool isDead(this PlayerControl player)
    {
        return player == null || player?.Data?.IsDead == true || player?.Data?.Disconnected == true ||
                (finalStatuses != null && finalStatuses.ContainsKey(player.PlayerId) && finalStatuses[player.PlayerId] != FinalStatus.Alive);
    }

    public static bool isAlive(this PlayerControl player)
    {
        return !isDead(player);
    }

    public static bool isNeutral(this PlayerControl player)
    {
        var roleInfo = RoleInfo.getRoleInfoForPlayer(player).FirstOrDefault();
        return roleInfo != null && roleInfo.roleType is RoleType.Neutral;
    }

    public static bool isCrewmate(this PlayerControl player)
    {
        return player != null && !player.isImpostor() && !player.isNeutral();
    }

    public static bool isImpostor(this PlayerControl player)
    {
        return player != null && player.Data.Role.IsImpostor;
    }

    public static bool hasFakeTasks(this PlayerControl player)
    {
        return player.isNeutral() && !player.neutralHasTasks() ||
            (player.isLovers() && Lovers.separateTeam && !Lovers.tasksCount);
    }

    public static bool neutralHasTasks(this PlayerControl player)
    {
        return player.isNeutral();
        // return player.isNeutral() &&
        //     (player.isRole(RoleType.Lawyer) ||
        //      player.isRole(RoleType.Pursuer) ||
        //      player.isRole(RoleType.Shifter) ||
        //      player.isRole(RoleType.Fox));
    }

    public static bool isLovers(this PlayerControl player)
    {
        return player != null && Lovers.isLovers(player);
    }

    public static PlayerControl getPartner(this PlayerControl player)
    {
        return Lovers.getPartner(player);
    }

    public static bool canBeErased(this PlayerControl player)
    {
        return !player.isRole(RoleId.Jackal) && !player.isRole(RoleId.Sidekick) && !TeamJackal.formerJackals.Any(x => x == player);
    }

    public static void clearAllTasks(this PlayerControl player)
    {
        if (player == null) return;
        for (int i = 0; i < player.myTasks.Count; i++)
        {
            PlayerTask playerTask = player.myTasks[i];
            playerTask.OnRemove();
            UnityEngine.Object.Destroy(playerTask.gameObject);
        }
        player.myTasks.Clear();

        if (player.Data != null && player.Data.Tasks != null)
            player.Data.Tasks.Clear();
    }

    public static void setSemiTransparent(this PoolablePlayer player, bool value)
    {
        float alpha = value ? 0.25f : 1f;
        foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
        }
        player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r, player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
    }

    public static string GetString(this TranslationController t, StringNames key, params Il2CppSystem.Object[] parts)
    {
        return t.GetString(key, parts);
    }

    public static bool shouldShowGhostInfo()
    {
        return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead && MapOptions.ghostsSeeInformation || AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended;
    }

    public static void MurderPlayer(this PlayerControl player, PlayerControl target)
    {
        player.MurderPlayer(target, MurderResultFlags.Succeeded);
    }

    public static void RpcRepairSystem(this ShipStatus shipStatus, SystemTypes systemType, byte amount)
    {
        shipStatus.RpcUpdateSystem(systemType, amount);
    }

    public static bool isMira()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 1;
    }

    public static bool isAirship()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 4;
    }
    public static bool isSkeld()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 0;
    }
    public static bool isPolus()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 2;
    }

    public static bool isFungle()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 5;
    }

    public static bool MushroomSabotageActive()
    {
        return PlayerControl.LocalPlayer.myTasks.ToArray().Any((x) => x.TaskType == TaskTypes.MushroomMixupSabotage);
    }

    public static bool sabotageActive()
    {
        var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
        return sabSystem.AnyActive;
    }

    public static float sabotageTimer()
    {
        var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
        return sabSystem.Timer;
    }

    public static bool canUseSabotage()
    {
        var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
        ISystemType systemType;
        IActivatable doors = null;
        if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Doors, out systemType))
        {
            doors = systemType.CastFast<IActivatable>();
        }
        return GameManager.Instance.SabotagesEnabled() && sabSystem.Timer <= 0f && !sabSystem.AnyActive && !(doors != null && doors.IsActive);
    }

    public static string cs(Color c, string s)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
    }

    public static int lineCount(string text)
    {
        return text.Count(c => c == '\n');
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
    {
        tie = true;
        KeyValuePair<byte, int> result = new(byte.MaxValue, int.MinValue);
        foreach (KeyValuePair<byte, int> keyValuePair in self)
        {
            if (keyValuePair.Value > result.Value)
            {
                result = keyValuePair;
                tie = false;
            }
            else if (keyValuePair.Value == result.Value)
            {
                tie = true;
            }
        }
        return result;
    }

    public static bool hidePlayerName(PlayerControl target)
    {
        return hidePlayerName(PlayerControl.LocalPlayer, target);
    }

    public static bool hidePlayerName(PlayerControl source, PlayerControl target)
    {
        if (source == target) return false;
        if (source == null || target == null) return true;
        if (source.isDead()) return false;
        if (target.isDead()) return true;
        if (Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive()) return true;
        if (MinigameExtensions.nightVisionIsActive) return true;
        if (Ninja.isInvisible && Ninja.ninja == target) return true;
        if (!MapOptions.hidePlayerNames) return false; // All names are visible
        if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target == Spy.spy || target.isRole(RoleId.Sidekick) && TeamJackal.Sidekick.wasTeamRed || target.isRole(RoleId.Jackal) && TeamJackal.Jackal.wasTeamRed)) return false; // Members of team Impostors see the names of Impostors/Spies
        if (source.isLovers() && target.isLovers()) return false; // Members of team Lovers see the names of each other
        if ((source.isRole(RoleId.Jackal) || source.isRole(RoleId.Sidekick)) && (target.isRole(RoleId.Jackal) || target.isRole(RoleId.Sidekick) || target == TeamJackal.Jackal.fakeSidekick)) return false; // Members of team Jackal see the names of each other
        return true;
    }

    public static void setDefaultLook(this PlayerControl target, bool enforceNightVisionUpdate = true)
    {
        if (Helpers.MushroomSabotageActive())
        {
            var instance = ShipStatus.Instance.CastFast<FungleShipStatus>().specialSabotage;
            MushroomMixupSabotageSystem.CondensedOutfit condensedOutfit = instance.currentMixups[target.PlayerId];
            NetworkedPlayerInfo.PlayerOutfit playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
            target.MixUpOutfit(playerOutfit);
        }
        else
        {
            target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId, enforceNightVisionUpdate);
        }
    }

    public static void setLook(this PlayerControl target, String playerName, int colorId, string hatId, string visorId, string skinId, string petId, bool enforceNightVisionUpdate = true)
    {
        target.RawSetColor(colorId);
        target.RawSetVisor(visorId, colorId);
        target.RawSetHat(hatId, colorId);
        target.RawSetName(hidePlayerName(PlayerControl.LocalPlayer, target) ? "" : playerName);

        SkinViewData nextSkin;
        try { nextSkin = ShipStatus.Instance.CosmeticsCache.GetSkin(skinId); } catch { return; }
        ;

        PlayerPhysics playerPhysics = target.MyPhysics;
        var spriteAnim = playerPhysics.myPlayer.cosmetics.skin.animator;
        var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();

        AnimationClip clip;
        if (currentPhysicsAnim == playerPhysics.Animations.group.RunAnim) clip = nextSkin.RunAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.SpawnAnim) clip = nextSkin.SpawnAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.EnterVentAnim) clip = nextSkin.EnterVentAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.ExitVentAnim) clip = nextSkin.ExitVentAnim;
        else if (currentPhysicsAnim == playerPhysics.Animations.group.IdleAnim) clip = nextSkin.IdleAnim;
        else clip = nextSkin.IdleAnim;
        float progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        playerPhysics.myPlayer.cosmetics.skin.skin = nextSkin;
        playerPhysics.myPlayer.cosmetics.skin.UpdateMaterial();

        spriteAnim.Play(clip, 1f);
        spriteAnim.m_animator.Play("a", 0, progress % 1);
        spriteAnim.m_animator.Update(0f);

        target.RawSetPet(petId, colorId);

        if (enforceNightVisionUpdate) MinigameExtensions.enforceNightVision(target);
        Chameleon.update();  // so that morphling and camo wont make the chameleons visible
    }

    public static void showFlash(Color color, float duration = 1f, string message = "")
    {
        if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        // Message Text
        TMPro.TextMeshPro messageText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
        messageText.text = message;
        messageText.enableWordWrapping = false;
        messageText.transform.localScale = Vector3.one * 0.5f;
        messageText.transform.localPosition += new Vector3(0f, 2f, -69f);
        messageText.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
        {
            var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

            if (p < 0.5)
            {
                if (renderer != null)
                    renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
            }
            else
            {
                if (renderer != null)
                    renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
            }
            if (p == 1f && renderer != null) renderer.enabled = false;
            if (p == 1f) messageText.gameObject.Destroy();
        })));
    }

    public static bool roleCanUseVents(this PlayerControl player)
    {
        bool roleCouldUse = false;
        if (Engineer.exists && Engineer.allPlayers.Contains(player))
        {
            roleCouldUse = true;
        }
        else if (TeamJackal.Jackal.canUseVents && player.isRole(RoleId.Jackal))
        {
            roleCouldUse = true;
        }
        else if (TeamJackal.Sidekick.canUseVents && player.isRole(RoleId.Sidekick))
        {
            roleCouldUse = true;
        }
        else if (Spy.canEnterVents && Spy.spy != null && Spy.spy == player)
        {
            roleCouldUse = true;
        }
        else if (Vulture.canUseVents && Vulture.vulture != null && Vulture.vulture == player)
        {
            roleCouldUse = true;
        }
        else if (Thief.canUseVents && Thief.thief != null && Thief.thief == player)
        {
            roleCouldUse = true;
        }
        else if (player.Data?.Role != null && player.Data.Role.CanVent)
        {
            if (Mafia.Janitor.janitor != null && PlayerControl.LocalPlayer.isRole(RoleId.Janitor))
            {
                roleCouldUse = false;
            }
            else if (Mafia.Mafioso.mafioso != null && PlayerControl.LocalPlayer.isRole(RoleId.Mafioso) && Mafia.Godfather.godfather != null && !Mafia.Godfather.godfather.Data.IsDead)
            {
                roleCouldUse = false;
            }
            else
            {
                roleCouldUse = true;
            }
        }
        return roleCouldUse;
    }

    public static bool roleCanSabotage(this PlayerControl player)
    {
        bool roleCouldUse = false;
        if (Jester.canSabotage && player.isRole(RoleId.Jester))
            roleCouldUse = true;
        // else if (Madmate.canSabotage && player.hasModifier(ModifierId.Madmate))
        //     roleCouldUse = true;
        // else if (!Mafioso.canSabotage && player.isRole(RoleId.Mafioso))
        //     roleCouldUse = false;
        // else if (!Janitor.canSabotage && player.isRole(RoleId.Janitor))
        //     roleCouldUse = false;
        // else if (player.Data?.Role != null && player.Data.Role.IsImpostor)
        //     roleCouldUse = true;

        return roleCouldUse;
    }

    public static bool checkArmored(PlayerControl target, bool breakShield, bool showShield, bool additionalCondition = true)
    {
        if (target != null && Armored.armored != null && Armored.armored == target && !Armored.isBrokenArmor && additionalCondition)
        {
            if (breakShield)
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.BreakArmor);
                RPCProcedure.breakArmor();
            }
            if (showShield)
            {
                target.ShowFailedMurder();
            }
            return true;
        }
        return false;
    }

    public static MurderAttemptResult checkMurderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false, bool ignoreMedic = false)
    {
        var targetRole = RoleInfo.getRoleInfoForPlayer(target).FirstOrDefault();
        // Modified vanilla checks
        if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
        if (killer == null || killer.Data == null || (killer.Data.IsDead && !ignoreIfKillerIsDead) || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return MurderAttemptResult.PerformKill;

        // Handle first kill attempt
        if (MapOptions.shieldFirstKill && MapOptions.firstKillPlayer == target) return MurderAttemptResult.SuppressKill;

        // Handle blank shot
        if (!ignoreBlank && Pursuer.blankedList.Any(x => x.PlayerId == killer.PlayerId))
        {
            using var writer = RPCProcedure.SendRPC(CustomRPC.SetBlanked);
            writer.Write(killer.PlayerId);
            writer.Write((byte)0);
            RPCProcedure.setBlanked(killer.PlayerId, 0);

            return MurderAttemptResult.BlankKill;
        }

        // Block impostor shielded kill
        foreach (var medic in Medic.players)
        {
            if (!ignoreMedic && medic.shielded != null && medic.shielded == target)
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.ShieldedMurderAttempt);
                RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                return MurderAttemptResult.SuppressKill;
            }
        }

        // Block impostor not fully grown mini kill
        if (Mini.mini != null && target == Mini.mini && !Mini.isGrownUp())
        {
            return MurderAttemptResult.SuppressKill;
        }

        // Block Time Master with time shield kill
        else if (TimeMaster.shieldActive && target.isRole(RoleId.TimeMaster))
        {
            if (!blockRewind)
            { // Only rewind the attempt was not called because a meeting started
                using var writer = RPCProcedure.SendRPC(CustomRPC.TimeMasterRewindTime);
                RPCProcedure.timeMasterRewindTime();
            }
            return MurderAttemptResult.SuppressKill;
        }

        // Thief if hit crew only kill if setting says so, but also kill the thief.
        else if (Thief.isFailedThiefKill(target, killer, targetRole))
        {
            if (!checkArmored(killer, true, true))
                Thief.suicideFlag = true;
            return MurderAttemptResult.SuppressKill;
        }

        // Block Armored with armor kill

        else if (checkArmored(target, true, killer == PlayerControl.LocalPlayer, Sheriff.exists || killer.isRole(RoleId.Sheriff) || isEvil(target) && Sheriff.canKillNeutrals || isKiller(target)))
        {
            return MurderAttemptResult.BlankKill;
        }

        if (TransportationToolPatches.isUsingTransportation(target) && !blockRewind && killer == Vampire.vampire)
        {
            return MurderAttemptResult.DelayVampireKill;
        }
        else if (TransportationToolPatches.isUsingTransportation(target))
        {
            return MurderAttemptResult.SuppressKill;
        }

        return MurderAttemptResult.PerformKill;
    }

    public static void MurderPlayer(PlayerControl killer, PlayerControl target, bool showAnimation)
    {
        using var writer = RPCProcedure.SendRPC(CustomRPC.UncheckedMurderPlayer);
        writer.Write(killer.PlayerId);
        writer.Write(target.PlayerId);
        writer.Write(showAnimation ? Byte.MaxValue : 0);
        RPCProcedure.uncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
    }

    public static MurderAttemptResult checkMurderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false)
    {
        // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
        // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible
        MurderAttemptResult murder = checkMurderAttempt(killer, target, isMeetingStart, ignoreBlank, ignoreIfKillerIsDead);

        if (murder == MurderAttemptResult.PerformKill)
        {
            MurderPlayer(killer, target, showAnimation);
        }
        else if (murder == MurderAttemptResult.DelayVampireKill)
        {
            HudManager.Instance.StartCoroutine(Effects.Lerp(10f, new Action<float>((p) =>
            {
                if (!TransportationToolPatches.isUsingTransportation(target) && Vampire.bitten != null)
                {
                    using var writer = RPCProcedure.SendRPC(CustomRPC.VampireSetBitten);
                    writer.Write(byte.MaxValue);
                    writer.Write(byte.MaxValue);
                    RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
                    MurderPlayer(killer, target, showAnimation);
                }
            })));
        }
        return murder;
    }

    public static void shareGameVersion()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
        writer.Write((byte)RebuildUs.Instance.Version.Major);
        writer.Write((byte)RebuildUs.Instance.Version.Minor);
        writer.Write((byte)RebuildUs.Instance.Version.Build);
        writer.Write(AmongUsClient.Instance.AmHost ? LobbyExtensions.timer : -1f);
        writer.WritePacked(AmongUsClient.Instance.ClientId);
        writer.Write((byte)(RebuildUs.Instance.Version.Revision < 0 ? 0xFF : RebuildUs.Instance.Version.Revision));
        writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.versionHandshake(RebuildUs.Instance.Version.Major, RebuildUs.Instance.Version.Minor, RebuildUs.Instance.Version.Build, RebuildUs.Instance.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
    }

    public static List<PlayerControl> getKillerTeamMembers(PlayerControl player)
    {
        List<PlayerControl> team = [];
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Role.IsImpostor && p.Data.Role.IsImpostor && player.PlayerId != p.PlayerId && team.All(x => x.PlayerId != p.PlayerId)) team.Add(p);
            else if (player.isRole(RoleId.Jackal) && p.isRole(RoleId.Sidekick)) team.Add(p);
            else if (player.isRole(RoleId.Sidekick) && p.isRole(RoleId.Jackal)) team.Add(p);
        }

        return team;
    }

    public static bool isKiller(PlayerControl player)
    {
        return player.Data.Role.IsImpostor ||
            (isNeutral(player) &&
            !player.isRole(RoleId.Jester) &&
            !player.isRole(RoleId.Arsonist) &&
            player != Vulture.vulture &&
            player != Lawyer.lawyer &&
            player != Pursuer.pursuer);
    }

    public static bool isEvil(PlayerControl player)
    {
        return player.Data.Role.IsImpostor || isNeutral(player);
    }

    public static bool zoomOutStatus = false;
    public static void toggleZoom(bool reset = false)
    {
        float orthographicSize = reset || zoomOutStatus ? 3f : 12f;

        zoomOutStatus = !zoomOutStatus && !reset;
        Camera.main.orthographicSize = orthographicSize;
        foreach (var cam in Camera.allCameras)
        {
            if (cam != null && cam.gameObject.name == "UI Camera") cam.orthographicSize = orthographicSize;  // The UI is scaled too, else we cant click the buttons. Downside: map is super small.
        }

        var tzGO = GameObject.Find("TOGGLEZOOMBUTTON");
        if (tzGO != null)
        {
            var rend = tzGO.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            var rendActive = tzGO.transform.Find("Active").GetComponent<SpriteRenderer>();
            rend.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("RebuildUs.Resources.Plus_Button.png", 100f) : Helpers.loadSpriteFromResources("RebuildUs.Resources.Minus_Button.png", 100f);
            rendActive.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("RebuildUs.Resources.Plus_ButtonActive.png", 100f) : Helpers.loadSpriteFromResources("RebuildUs.Resources.Minus_ButtonActive.png", 100f);
            tzGO.transform.localScale = new Vector3(1.2f, 1.2f, 1f) * (zoomOutStatus ? 4 : 1);
        }

        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen); // This will move button positions to the correct position.
    }

    public static bool hasImpVision(NetworkedPlayerInfo player)
    {
        return player.Role.IsImpostor
            || ((player.Object.isRole(RoleId.Jackal) || TeamJackal.formerJackals.Any(x => x.PlayerId == player.PlayerId)) && TeamJackal.Jackal.hasImpostorVision)
            || (player.Object.isRole(RoleId.Sidekick) && TeamJackal.Sidekick.hasImpostorVision)
            || (Spy.spy != null && Spy.spy.PlayerId == player.PlayerId && Spy.hasImpostorVision)
            || (player.Object.isRole(RoleId.Jester) && Jester.hasImpostorVision)
            || (Thief.thief != null && Thief.thief.PlayerId == player.PlayerId && Thief.hasImpostorVision);
    }

    public static object TryCast(this Il2CppObjectBase self, Type type)
    {
        return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
    }

    public static void Destroy(this UnityEngine.Object obj)
    {
        UnityEngine.Object.Destroy(obj);
    }
}