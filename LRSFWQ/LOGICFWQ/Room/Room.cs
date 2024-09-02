using System;
using System.Collections.Generic;
using System.Text;
#nullable disable

public class Room
{
    public int id = 0;
    public int maxPlayer = 999;
    public List<uint> playerIds = new List<uint>();

    public LockStepManager lockStepManager = new LockStepManager();
    public Dictionary<uint, int> playerUnsyncOpt = new Dictionary<uint, int>();

    public Thread thread = null;
    public void StartLockStep()
    {
        // 检查线程是否已经在运行  
        if (thread != null && thread.IsAlive)
        {
            Console.WriteLine("线程已经在运行中，不会重复启动。");
            return;
        }
        thread = new Thread(lockStepManager.Run);
        thread.Start();
    }

    public bool AddPlayer(uint id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if(player == null)
        {
            Console.WriteLine("AddPlayer失败，玩家为空");
            return false;
        }
        if(playerIds.Count>=maxPlayer)
        {
            Console.WriteLine("AddPlayer失败，房间满了");
            return false;
        }
        if(playerIds.Contains(id))
        {
            Console.WriteLine("AddPlayer失败，玩家已经在房间了");
            return false;
        }

        playerIds.Add(id);
        player.roomId = this.id;
        playerUnsyncOpt.Add(id, 0);
        return true;
    }

    public void TcpBroadcast(MsgBase msg)
    {
        for(int i=0;i<playerIds.Count;i++)
        {
            PlayerManager.GetPlayer(playerIds[i]).Send(msg);
        }
    }

    public void UdpBroadcast(MsgBase msg)
    {
        for (int i = 0; i < playerIds.Count; i++)
        {
            PlayerManager.GetPlayer(playerIds[i]).SendTo(msg);
        }
    }

    public void RemovePlayer(uint guid)
    {
        // 检查玩家是否在房间中
        if (!playerIds.Contains(guid))
        {
            Console.WriteLine("RemovePlayer失败，玩家不在房间中");
            return;
        }

        // 移除玩家
        playerIds.Remove(guid);
        playerUnsyncOpt.Remove(guid);

        // 清理玩家的房间 ID
        Player player = PlayerManager.GetPlayer(guid);
        if (player != null)
        {
            player.roomId = -1; // 或者设置为其他合适的值
        }
        // 清理玩家
        PlayerManager.RemovePlayer(guid);

        if (PlayerManager.players.Count <= 1)
        {
            lockStepManager.isStarted = false;
        }

        Console.WriteLine("玩家已从房间中移除: " + guid);
    }

}