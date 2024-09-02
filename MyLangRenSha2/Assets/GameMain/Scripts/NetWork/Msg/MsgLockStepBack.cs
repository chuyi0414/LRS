//服务端返回给客户端
using System;

public class MsgLockStepBack :MsgBase
{
    public int turn;

    public UnsyncOpts[] unsyncOpts;
}

public class MsgMoveBack : MsgBase
{
    public Opts opts;
}

//未同步的操作
[Serializable]
public class UnsyncOpts
{
    public int turn;

    public Opts[] opts;
}