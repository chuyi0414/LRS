using System;

public class MsgLockStep : MsgBase
{
    public int turn;

    public Opts[] opts;
}

public class MsgMove : MsgBase
{
    public Opts opts;
}

//一帧的操作
[Serializable]
public class Opts
{
    public uint guid;

    public Operation operation;

    public Fixed64[] param;
}