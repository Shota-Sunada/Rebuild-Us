using System;
using Hazel;

namespace RebuildUs;

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