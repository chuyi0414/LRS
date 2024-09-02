using GameFramework.Network;
using System;
using System.IO;
using System.Text;

internal class NetworkChannelHelper : INetworkChannelHelper
{
    // 包头长度固定为 4 字节，用于表示整个包的长度
    public int PacketHeaderLength => sizeof(int);

    public void Initialize(INetworkChannel networkChannel)
    {
        Console.WriteLine("NetworkChannel initialized.");
    }

    public void PrepareForConnecting()
    {
        Console.WriteLine("Preparing for connecting.");
    }

    public bool SendHeartBeat()
    {
        Console.WriteLine("Heartbeat sent.");
        return true;
    }

    public void Shutdown()
    {
        Console.WriteLine("NetworkChannel shutting down.");
    }

    public bool Serialize<T>(T packet, Stream destination) where T : Packet
    {
        try
        {
            using (BinaryWriter writer = new BinaryWriter(destination, Encoding.UTF8, true))
            {
                // 先写入包头
                writer.Write(packet.Id);

                // 假设包体是一个字符串
                string packetData = packet.ToString();
                writer.Write(packetData);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Serialize error: {ex.Message}");
            return false;
        }
    }

    public IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData)
    {
        customErrorData = null;

        try
        {
            using (BinaryReader reader = new BinaryReader(source, Encoding.UTF8, true))
            {
                int packetLength = reader.ReadInt32();
                return new DefaultPacketHeader(packetLength);
            }
        }
        catch (Exception ex)
        {
            customErrorData = ex;
            Console.WriteLine($"DeserializePacketHeader error: {ex.Message}");
            return null;
        }
    }

    public Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData)
    {
        customErrorData = null;

        try
        {
            using (BinaryReader reader = new BinaryReader(source, Encoding.UTF8, true))
            {
                string packetData = reader.ReadString();
                return new ExamplePacket(packetHeader.PacketLength, packetData); // 假设你有一个 ExamplePacket 类
            }
        }
        catch (Exception ex)
        {
            customErrorData = ex;
            Console.WriteLine($"DeserializePacket error: {ex.Message}");
            return null;
        }
    }
}

// 包头的默认实现
internal class DefaultPacketHeader : IPacketHeader
{
    public int PacketLength { get; }

    public DefaultPacketHeader(int packetLength)
    {
        PacketLength = packetLength;
    }
}