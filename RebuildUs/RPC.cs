using System;
using Hazel;
using HarmonyLib;

namespace RebuildUs;

public enum CustomRPC : byte
{
    ShareOptions = 80,
    SetRole,
}

public static class RPCProcedure
{
    public static RPCSender SendRPC(uint netId, CustomRPC callId, int targetId = -1)
    {
        return new RPCSender(netId, callId, targetId);
    }

    public static RPCSender SendRPC(CustomRPC callId, int targetId = -1)
    {
        return new RPCSender(PlayerControl.LocalPlayer.NetId, callId, targetId);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class HandleRpcPatch
{
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (__instance == null) return;
        if (reader == null) return;

        // アモアスが0から65まで使用 => 将来追加されることを見込んで80番から使用すること
        // Submergedが210から214まで使用するので重複厳禁
        // 最大値は 255(=byte.MaxValue)
        switch ((CustomRPC)callId)
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
public class RPCSender(uint netId, CustomRPC callId, int targetId = -1) : IDisposable
{
    // Send RPC to player with netId
    private readonly MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(netId, (byte)callId, SendOption.Reliable, targetId);

    public void Dispose()
    {
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public void Write(bool value)
    {
        writer.Write(value);
    }

    public void Write(byte value)
    {
        writer.Write(value);
    }

    public void Write(uint value, bool isPacked = false)
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

    public void Write(int value, bool isPacked = false)
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

    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(string value)
    {
        writer.Write(value);
    }
}