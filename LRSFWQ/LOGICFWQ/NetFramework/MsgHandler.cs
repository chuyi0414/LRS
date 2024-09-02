
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
#nullable enable

public class MsgHandler
{
    public static void MsgTest(uint guid, MsgBase msgBase)
    {
        Console.WriteLine("测试MsgTest");
        NetManager.SendTo(msgBase, guid);

    }

    public static void MsgStart(uint guid, MsgBase msgBase)
    {
        Console.WriteLine($"MsgStart|guid:{guid}");
        MsgStart msgStart = (MsgStart)msgBase;
        
        Player player = new Player(guid);
        PlayerManager.AddPlayer(guid, player);

        if (PlayerManager.players.Count == 1)
        {
            Room room = RoomManager.GetRoom();

            room.AddPlayer(guid);
        }
        else
        {
            Room room = RoomManager.GetRoom();
            room.AddPlayer(guid);

            room.StartLockStep();

            msgStart.res = true;

            msgStart.guid = new uint[room.playerIds.Count];
            for (int i = 0; i < room.playerIds.Count; i++)
            {
                msgStart.guid[i] = room.playerIds[i];
            }

            room.UdpBroadcast(msgStart);
        }

    }

    public static void MsgGetInfo(uint guid, MsgBase msgBase)
    {
        Console.WriteLine($"MsgGetInfo|guid:{guid}");
        MsgGetInfo msg = (MsgGetInfo)msgBase;
        msg.guid = guid;

        NetManager.SendTo(msg, guid);
    }

    public static void MsgLockStep(uint guid, MsgBase msgBase)
    {
        MsgLockStep msg = (MsgLockStep)msgBase;
        Player player = PlayerManager.GetPlayer(guid);

        if (player == null)
            return;
        Room room = RoomManager.GetRoom();
        if (room == null)
            return;

        LockStepManager lockStepManager = room.lockStepManager;
        lock (lockStepManager)
        {
            if (!lockStepManager.allOpt.ContainsKey(msg.turn))
            {
                lockStepManager.allOpt.Add(msg.turn, new List<Opts>());
            }
            for (int i = 0; i < msg.opts.Length; i++)
            {
                lockStepManager.allOpt[msg.turn].Add(msg.opts[i]);
            }
            room.playerUnsyncOpt[guid] = msg.turn - 1;
        }
    }

    public static void MsgStop(uint guid, MsgBase msgBase)
    {
        Console.WriteLine($"MsgStop|guid:{guid}");
        // 假设 MsgStop 类中有一个需要的字段，如 stopReason
        MsgStop msgStop = (MsgStop)msgBase;

        // 获取玩家对象
        Player player = PlayerManager.GetPlayer(guid);
        if (player == null)
        {
            Console.WriteLine("玩家不存在: " + guid);
            return;
        }

        // 获取玩家所在的房间
        Room room = RoomManager.GetRoom();
        if (room == null)
        {
            Console.WriteLine("房间不存在: " + player.roomId);
            return;
        }

        // 移除玩家
        room.RemovePlayer(guid);


        // 广播停止消息给所有玩家
        MsgStop broadcastMsg = new MsgStop
        {
            guid = guid,
        };

        room.UdpBroadcast(broadcastMsg);
    }

    public static void MsgMove(uint guid, MsgBase msgBase)
    {
        Console.WriteLine($"MsgMove|guid:{guid}");
        MsgMove msgMove = (MsgMove)msgBase;

        MsgMoveBack msgMoveBack = new MsgMoveBack();
        msgMoveBack.opts = msgMove.opts;

        Room room = RoomManager.GetRoom();

        for(int i = 0;i< room.playerIds.Count;i++)
        {
            NetManager.SendTo(msgMoveBack, room.playerIds[i]);
        }
    }
}