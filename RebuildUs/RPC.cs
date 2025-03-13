using System;
using Hazel;
using HarmonyLib;
using RebuildUs.Modules;
using System.Linq;

namespace RebuildUs;

internal enum CustomRPC : byte
{
    ShareOptions = 80,
}

internal static class RPCProcedure
{
    internal static RPCSender SendRPC(uint netId, CustomRPC callId, int targetId = -1)
    {
        return new RPCSender(netId, callId, targetId);
    }

    internal static RPCSender SendRPC(CustomRPC callId, int targetId = -1)
    {
        return new RPCSender(PlayerControl.LocalPlayer.NetId, callId, targetId);
    }

    internal static void HandleShareOptions(byte optionsCount, MessageReader reader)
    {
        try
        {
            for (int i = 0; i < optionsCount; i++)
            {
                int id = reader.ReadPackedInt32();
                int selectedIndex = reader.ReadPackedInt32();
                var option = CustomOption.AllOptions[id];
                option.UpdateSelection(selectedIndex, i == optionsCount - 1);
            }
        }
        catch (Exception e)
        {
            RebuildUsPlugin.Instance.Logger.LogError($"Error while deserializing options: {e.Message}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal static class HandleRpcPatch
{
    internal static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (__instance == null) return;
        if (reader == null) return;

        // アモアスが0から65まで使用 => 将来追加されることを見込んで80番から使用すること
        // Submergedが210から214まで使用するので重複厳禁
        // 最大値は 255(=byte.MaxValue)
        switch ((CustomRPC)callId)
        {
            case CustomRPC.ShareOptions:
                RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                break;

            default:
                break;
        }
    }
}

// RPCの送信を行うクラス
// このクラスはIDisposableを実装しているため、usingステートメントを使って使い捨てることができます
// 例:
// using (var rpc = new RPCSender(0, 0))
// {
//     rpc.Write(0);
//     rpc.Write(1);
// } // Disposeが呼ばれる
// ↑っていうのをCopilotが生成してくれた
internal class RPCSender(uint netId, CustomRPC callId, int targetId = -1) : IDisposable
{
    // Send RPC to player with netId
    private readonly MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(netId, (byte)callId, SendOption.Reliable, targetId);

    public void Dispose()
    {
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    internal void Write(bool value)
    {
        writer.Write(value);
    }

    internal void Write(byte value)
    {
        writer.Write(value);
    }

    internal void Write(uint value, bool isPacked = false)
    {
        if (isPacked)
        {
            writer.WritePacked(value);
        }
        else
        {
            writer.Write(value);
        }
    }

    internal void Write(int value, bool isPacked = false)
    {
        if (isPacked)
        {
            writer.WritePacked(value);
        }
        else
        {
            writer.Write(value);
        }
    }

    internal void Write(float value)
    {
        writer.Write(value);
    }

    internal void Write(string value)
    {
        writer.Write(value);
    }
}