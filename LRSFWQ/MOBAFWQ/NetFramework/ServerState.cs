using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
#nullable disable

public class ServerState
{
    public Socket socket;

    public ByteArray readBuffer=new ByteArray();
    public Gateway.ServerType serverType;
}