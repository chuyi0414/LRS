using System;
using System.Collections.Generic;
using System.Text;


public class Player
{
    public uint guid;
    public int roomId = -1;

    public Player(uint guid)
    {
        this.guid = guid;
    }

    public void Send(MsgBase msg)
    {
        NetManager.Send(msg,guid);
    }

    public void SendTo(MsgBase msg)
    {
        NetManager.SendTo(msg, guid);
    }
}