using System;
using Hazel;
using HarmonyLib;

namespace RebuildUs;

internal static class RPCProcedure
{
    internal static RPCSender SendRPC(uint netId, byte callId)
    {
        return new RPCSender(netId, callId);
    }

    internal static RPCSender SendRPC(byte callId)
    {
        return new RPCSender(PlayerControl.LocalPlayer.NetId, callId);
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
        switch (callId)
        {
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
internal class RPCSender(uint netId, byte callId) : IDisposable
{
    // Send RPC to player with netId
    private readonly MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(netId, callId, SendOption.Reliable);

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