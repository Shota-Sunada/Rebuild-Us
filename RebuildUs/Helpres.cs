using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace RebuildUs;

public static class Helpers
{
    public static bool IsRoleEnabled()
    {
        return true;
    }

    public static Dictionary<string, Sprite> CachedSprites = [];

    public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit, bool cache = true)
    {
        try
        {
            if (cache && CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            var texture = LoadTextureFromResources(path);
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

    public static unsafe Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
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

    public static Texture2D LoadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            }
        }
        catch
        {
            Plugin.Instance.Logger.LogError("Error loading texture from disk: " + path);
        }
        return null;
    }

    public static void Destroy(this UnityEngine.Object obj)
    {
        UnityEngine.Object.Destroy(obj);
    }

    public static bool IsCrewmate(this PlayerControl player)
    {
        return player != null && !player.IsImpostor() && !player.IsNeutral();
    }

    public static bool IsImpostor(this PlayerControl player)
    {
        return player != null && player.Data.Role.IsImpostor;
    }

    public static bool IsNeutral(this PlayerControl player)
    {
        return false;
    }

    public static bool IsDead(this PlayerControl player)
    {
        return
            player == null ||
            player.Data.IsDead ||
            player.Data.Disconnected;
    }

    public static bool IsAlive(this PlayerControl player)
    {
        return !IsDead(player);
    }

    public static string Cs(this string str, Color c)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), str);
    }

    public static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    public static bool HasFakeTasks(this PlayerControl player)
    {
        return false;
    }

    public static object TryCast(this Il2CppObjectBase self, Type type)
    {
        return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, []);
    }

    public static void RpcRepairSystem(this ShipStatus shipStatus, SystemTypes systemType, byte amount)
    {
        shipStatus.RpcUpdateSystem(systemType, amount);
    }

    public static PlayerControl PlayerById(byte id)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == id)
            {
                return player;
            }
        }
        return null;
    }

    public static void ClearAllTasks(this PlayerControl player)
    {
        if (player == null) return;
        for (int i = 0; i < player.myTasks.Count; i++)
        {
            var playerTask = player.myTasks[i];
            playerTask.OnRemove();
            UnityEngine.Object.Destroy(playerTask.gameObject);
        }
        player.myTasks.Clear();

        if (player.Data != null && player.Data.Tasks != null)
        {
            player.Data.Tasks.Clear();
        }
    }
}