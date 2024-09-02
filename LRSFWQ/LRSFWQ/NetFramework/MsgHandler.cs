using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;


public class MsgHandler
{
    public static void MsgPing(ClientState state,MsgBase msgBase)
    {
        Console.WriteLine("测试");
    }

    /*public static void MsgPing(ClientState state, IExtensible msgBase)
    {
        Console.WriteLine("MsgPing:" + state.socket.RemoteEndPoint);
        state.lastPingTime = NetManager.GetTimeStamp();
        proto.MsgPong.MsgPong msg = new MsgPong();
        NetManagerProto.Send(state, msg);
    }*/
}