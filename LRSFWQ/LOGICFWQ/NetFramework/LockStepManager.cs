using System;
using System.Collections.Generic;
using System.Text;
#nullable disable

public class LockStepManager
{
    private long lastTime;

    private long currTime;

    private long timeInterval = (long)33.33333333;

    public bool isStarted;

    public int turn = 0;

    public Room room;

    public Dictionary<int, List<Opts>> allOpt = new Dictionary<int, List<Opts>>();
    public void Run()
    {
        if (isStarted)
            return;

        isStarted = true;
        lastTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        while (true)
        {
            currTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;

            long time = currTime - lastTime;
            if(time>timeInterval)
            {
                lastTime = currTime;
                turn++;

                MsgLockStepBack msgLockStepBack = new MsgLockStepBack();
                msgLockStepBack.turn = turn;

                foreach (var player in room.playerUnsyncOpt)
                {
                    List<UnsyncOpts> unsyncOpts = new List<UnsyncOpts>();
                    //player.Value当前玩家同步到的帧数 turn表示服务器帧数 
                    //player.Value到turn之间表示当前客户端没有同步的帧数
                    if (player.Value < turn)
                    {
                        UnsyncOpts unsyncOpt = new UnsyncOpts();
                        for (int i = player.Value; i < turn; i++)
                        {
                            unsyncOpt.turn = i;
                            if (allOpt.ContainsKey(i))
                            {
                                unsyncOpt.opts = allOpt[i].ToArray();
                            }
                        }
                        unsyncOpts.Add(unsyncOpt);
                    }
                    msgLockStepBack.unsyncOpts = unsyncOpts.ToArray();
                    NetManager.SendTo(msgLockStepBack, player.Key);
                }
            }
        }
    }
}