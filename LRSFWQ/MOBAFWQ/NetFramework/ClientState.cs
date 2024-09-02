using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
#nullable disable

public class ClientState
{
    public uint guid;

    public Socket socket;

    public ByteArray readBuffer=new ByteArray();

    public long lastPingTime=0;
}