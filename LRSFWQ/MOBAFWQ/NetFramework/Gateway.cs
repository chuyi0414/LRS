using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Channels;
#nullable disable
public static class Gateway
{
    public enum ServerType
    {
        Gateway,//网关服务器
        Fighter,//战斗服务器
    }
    /// <summary>
    /// 用于连接客户端的服务端socket
    /// </summary>
    public static Socket listenfd;
    /// <summary>
    /// 用于连接其他服务端的Socket
    /// </summary>
    public static Socket gateway;
    /// <summary>
    /// 客户端字典
    /// </summary>
    public static Dictionary<Socket, ClientState> clientStates = new Dictionary<Socket, ClientState>();
    /// <summary>
    /// 其他服务端字典
    /// </summary>
    public static Dictionary<Socket, ServerState> serverStates = new Dictionary<Socket, ServerState>();

    /// <summary>
    /// 服务器类型和服务器的映射
    /// </summary>
    public static Dictionary<ServerType, ServerState> type2ss = new Dictionary<ServerType, ServerState>();
    /// <summary>
    /// 通过id找到相应客户端的字典
    /// </summary>
    public static Dictionary<uint, ClientState> id2cs = new Dictionary<uint, ClientState>();
    /// <summary>
    /// 用于检测的列表
    /// </summary>
    public static List<Socket> sockets = new List<Socket>();
    private static float pingInterval = 2;

    /// <summary>
    /// 接收客户端的udp
    /// </summary>
    private static UdpClient receiveClientUdp;
    /// <summary>
    /// 接收服务端的udp
    /// </summary>
    private static UdpClient receiveServerUdp;

    private static readonly object udpLock = new object();
    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip">ip地址</param>
    /// <param name="port">端口号</param>
    public static void Connect(string ip, int port)
    {
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress iPAddress = IPAddress.Parse(ip);
        IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
        listenfd.Bind(iPEndPoint);
        listenfd.Listen(0);


        receiveClientUdp = new UdpClient((IPEndPoint)listenfd.LocalEndPoint);

        Console.WriteLine("网关服务器启动成功");
        while (true)
        {
            sockets.Clear();
            //放服务端的socket
            sockets.Add(listenfd);
            //放客户端的Socket
            foreach (Socket socket in clientStates.Keys)
            {
                sockets.Add(socket);
            }
            Socket.Select(sockets, null, null, 1000);
            for (int i = 0; i < sockets.Count; i++)
            {
                Socket s = sockets[i];
                if (s == listenfd)
                {
                    //有客户端要连接
                    Accept(s);
                }
                else
                {
                    //客户端发消息过来了
                    Receive(s);
                }
            }
            //CheckPing();
        }
    }

    /// <summary>
    /// 连接其他服务器
    /// </summary>
    /// <param name="ip">ip地址</param>
    /// <param name="port">端口号</param>
    public static ServerState ConnectServer(string ip, int port)
    {
        ServerState serverState = new ServerState();
        gateway = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port);
        gateway.Bind(iPEndPoint);
        gateway.Listen(0);
        Console.WriteLine("战斗服务器等待连接");
        gateway.BeginAccept(AcceptServerCallback, serverState);
        return serverState;
    }
    /// <summary>
    /// 接收其他服务端连接的回调
    /// </summary>
    /// <param name="ar"></param>
    private static void AcceptServerCallback(IAsyncResult ar)
    {
        //封装连接过来的服务端对象
        ServerState serverState = (ServerState)ar.AsyncState;
        Socket socket = gateway.EndAccept(ar);
        Console.WriteLine("连接成功");
        serverState.socket = socket;

        serverStates.Add(socket, serverState);


        //udp
        receiveServerUdp = new UdpClient((IPEndPoint)gateway.LocalEndPoint);
        receiveServerUdp.BeginReceive(ReceiveUdpServerCallback, serverState);

        //接收消息
        ByteArray byteArray = serverState.readBuffer;
        socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveServerCallback, serverState);
    }
    /// <summary>
    /// 接收其他服务端发过来的消息回调
    /// </summary>
    /// <param name="ar"></param>
    private static void ReceiveServerCallback(IAsyncResult ar)
    {
        ServerState serverState = (ServerState)ar.AsyncState;
        int count = 0;
        Socket server = serverState.socket;


        ByteArray byteArray = serverState.readBuffer;
        if (byteArray.Remain <= 0)
        {
            byteArray.MoveBytes();
        }
        if (byteArray.Remain <= 0)
        {
            Console.WriteLine("Receive fail :数组长度不足");
            //关闭服务端
            //Close();
            return;
        }
        try
        {
            count = server.EndReceive(ar);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Receive fail:" + e.Message);
            //关闭服务端
            Close(server);
            return;
        }
        if (count <= 0)
        {
            //Console.WriteLine("Socket Close:" + serverState.socket.RemoteEndPoint.ToString());
            //关闭服务端
            //Close();
            return;
        }

        //处理接收过来的消息
        byteArray.writeIndex += count;
        OnReceiveData(serverState);
        byteArray.MoveBytes();

        server.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveServerCallback, serverState);
    }
    /// <summary>
    /// 处理服务端发过来的消息
    /// </summary>
    /// <param name="serverState"></param>
    private static void OnReceiveData(ServerState serverState)
    {
        ByteArray byteArray = serverState.readBuffer;
        byte[] bytes = byteArray.bytes;

        if (byteArray.Length <= 2)
        {
            return;
        }
        //解析长度
        short length = (short)(bytes[byteArray.readIndex + 1] * 256 + bytes[byteArray.readIndex]);

        if (byteArray.Length < length + 2)
        {
            return;
        }

        uint guid = (uint)(bytes[byteArray.readIndex + 2] << 24 |
                    bytes[byteArray.readIndex + 3] << 16 |
                    bytes[byteArray.readIndex + 4] << 8 |
                    bytes[byteArray.readIndex + 5]);
        byteArray.readIndex += 6;

        try
        {
            int msgLength = length - 4;
            //发送给客户端的数组
            byte[] sendBytes = new byte[msgLength + 2];
            //打包长度
            sendBytes[0] = (byte)(msgLength % 256);
            sendBytes[1] = (byte)(msgLength / 256);

            Array.Copy(bytes, byteArray.readIndex, sendBytes, 2, msgLength);


            id2cs[guid].socket.Send(sendBytes, 0);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.Message);
        }
        byteArray.readIndex += length - 4;

        //继续处理
        if (byteArray.Length > 2)
        {
            OnReceiveData(serverState);
        }
    }
    /// <summary>
    /// 接收客户端的连接
    /// </summary>
    /// <param name="listenfd">服务端的socket</param>
    private static void Accept(Socket listenfd)
    {
        try
        {
            Socket socket = listenfd.Accept();
            // 创建描述客户端的对象
            ClientState state = new ClientState
            {
                socket = socket,
                guid = MyGuid.GetGuid(),
                lastPingTime = GetTimeStamp()
            };
            // 直接传递 state 对象作为回调的状态
            receiveClientUdp.BeginReceive(ReceiveUdpClientCallback, state);
            // 保存客户端状态
            id2cs.Add(state.guid, state);
            clientStates.Add(socket, state);
            Console.WriteLine("获取Accept的guid：" + state.guid + "|ip：" + socket.RemoteEndPoint);

            
        }
        catch (SocketException e)
        {
            Console.WriteLine("Accept 失败" + e.Message);
        }
    }
    /// <summary>
    /// 接收客户端发过来的消息
    /// </summary>
    /// <param name="socket">客户端的socket</param>
    private static void Receive(Socket socket)
    {
        ClientState state = clientStates[socket];
        ByteArray readBuffer = state.readBuffer;

        if (readBuffer.Remain <= 0)
        {
            readBuffer.MoveBytes();
        }
        if (readBuffer.Remain <= 0)
        {
            Console.WriteLine("Receive 失败,数组不够大");
            return;
        }
        int count = 0;
        try
        {
            count = socket.Receive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.Remain, 0);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Receive 失败," + e.Message);
            Close(socket); // 关闭客户端
            return;
        }
        //客户端主动关闭
        if (count <= 0)
        {
            Console.WriteLine("Socket Close :" + socket.RemoteEndPoint.ToString());
            Close(socket); // 关闭客户端
            return;
        }
        readBuffer.writeIndex += count;
        //处理消息
        OnReceiveData(state);
        readBuffer.MoveBytes();
    }
    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="state">客户端对象</param>
    private static void OnReceiveData(ClientState state)
    {
        ByteArray readBuffer = state.readBuffer;
        byte[] bytes = readBuffer.bytes;
        int readIndex = readBuffer.readIndex;

        if (readBuffer.Length <= 2)
            return;
        //解析总长度
        short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        //收到的消息没有解析出来的多
        if (readBuffer.Length < length + 2)
            return;

        ServerType serverType = (ServerType)bytes[readIndex + 2];
        readBuffer.readIndex += 3;


        try
        {
            //减去一个字节的服务器号，留四位作为id 得到发送出去消息的长度
            int sendLength = length - 1 + 4;
            byte[] sendBytes = new byte[sendLength + 2];
            sendBytes[0] = (byte)(sendLength % 256);
            sendBytes[1] = (byte)(sendLength / 256);

            sendBytes[2] = (byte)(state.guid >> 24);
            sendBytes[3] = (byte)((state.guid >> 16) & 0xff);
            sendBytes[4] = (byte)((state.guid >> 8) & 0xff);
            sendBytes[5] = (byte)(state.guid & 0xff);

            Array.Copy(bytes, readBuffer.readIndex, sendBytes, 6, sendLength - 4);
            type2ss[serverType].socket.Send(sendBytes, 0, sendLength + 2, 0);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.Message);
        }

        readBuffer.readIndex += length - 1;
        readBuffer.MoveBytes();

        //继续处理
        if (readBuffer.Length > 2)
        {
            OnReceiveData(state);
        }
    }
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="state">客户端对象</param>
    /// <param name="msgBase">消息</param>
    public static void Send(ClientState state, MsgBase msgBase)
    {
        if (state == null || !state.socket.Connected)
            return;

        //编码
        byte[] nameBytes = MsgBase.EncodeName(msgBase);
        byte[] bodyBytes = MsgBase.Encode(msgBase);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[len + 2];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        try
        {
            state.socket.Send(sendBytes, 0, sendBytes.Length, 0);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Send 失败" + e.Message);
        }
    }
    /// <summary>
    /// 关闭对应的客户端
    /// </summary>
    /// <param name="state">客户端</param>
    private static void Close(Socket socket)
    {
        if (serverStates.ContainsKey(socket))
        {
            
        }
        else if (clientStates.ContainsKey(socket))
        {
            Console.WriteLine("客户端已断开连接: " + socket.RemoteEndPoint);
            // 处理客户端断开连接
            ClientState clientState = clientStates[socket];
            // 通知所有服务器
            foreach (var serverEntry in type2ss)
            {
                SendUdpToServer(clientState, serverEntry.Key, new MsgStop());
            }
            id2cs.Remove(clientState.guid);
            clientStates.Remove(socket);
            clientState.socket.Close();
        }

    }
    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <returns>时间戳</returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
    private static void CheckPing()
    {
        foreach (ClientState state in clientStates.Values)
        {
            if (GetTimeStamp() - state.lastPingTime > pingInterval * 4)
            {
                Console.WriteLine("心跳机制，断开连接:", state.socket.RemoteEndPoint);
                //关闭客户端
                //Close(state);
                return;
            }
        }
    }

    #region Udp

    /// <summary>
    /// 向服务器发送UDP消息
    /// </summary>
    /// <param name="serverType">要发送的服务器</param>
    /// <param name="message">要发送的消息</param>
    public static void SendUdpToServer(ClientState state , ServerType serverType, MsgBase message)
    {
        // 1. 获取目标服务器状态
        if (!type2ss.TryGetValue(serverType, out ServerState serverState))
        {
            Console.WriteLine($"未找到类型为 {serverType} 的服务器状态");
            return;
        }

        // 2. 检查服务器是否连接并具有有效的 UDP 端点
        IPEndPoint serverIpendPoint = serverState.socket?.RemoteEndPoint as IPEndPoint;
        if (serverState == null || serverIpendPoint == null)
        {
            Console.WriteLine($"服务器类型 {serverType} 未连接或无效的 UDP 端点");
            return;
        }

        try
        {
            // 3. 准备要发送的字节数组
            byte[] nameBytes = MsgBase.EncodeName(message);
            byte[] bodyBytes = MsgBase.Encode(message);
            int len = nameBytes.Length + bodyBytes.Length + 1;
            byte[] sendBytes = new byte[len];
            sendBytes[0] = (byte)serverType;
            Array.Copy(nameBytes, 0, sendBytes, 1, nameBytes.Length);
            Array.Copy(bodyBytes, 0, sendBytes, 1 + nameBytes.Length, bodyBytes.Length);

            // 4. 将 GUID 信息写入消息的前4个字节
            // 5. 将编码后的消息数据复制到发送数组中
            byte[] sendBytes1 = new byte[sendBytes.Length + 3];
            sendBytes1[0] = (byte)(state.guid >> 24);
            sendBytes1[1] = (byte)((state.guid >> 16) & 0xff);
            sendBytes1[2] = (byte)((state.guid >> 8) & 0xff);
            sendBytes1[3] = (byte)(state.guid & 0xff);
            Array.Copy(sendBytes, 1, sendBytes1, 4, sendBytes.Length - 1);

            // 6. 使用 UdpClient 发送消息
            receiveServerUdp.Send(sendBytes1, sendBytes1.Length, serverIpendPoint);

            Console.WriteLine($"关闭消息已发送到服务器类型 {serverType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送 UDP 消息到服务器类型 {serverType} 失败: {ex.Message}");
        }
    }


    private static void ReceiveUdpClientCallback(IAsyncResult ar)
    {
        lock (udpLock)
        {
            IPEndPoint iPEndPoint = null;
            byte[] receiveBuf = null;
            try
            {
                // 使用 IPEndPoint 进行正确的 state 查找
                iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                receiveBuf = receiveClientUdp.EndReceive(ar, ref iPEndPoint);
            }catch (Exception ex)
            {
                return;
            }
            

            // 查找正确的 ClientState 对象
            ClientState state = clientStates.Values
                .FirstOrDefault(cs => ((IPEndPoint)cs.socket.RemoteEndPoint).Address.Equals(iPEndPoint.Address)
                                       && ((IPEndPoint)cs.socket.RemoteEndPoint).Port.Equals(iPEndPoint.Port));

            if (state == null)
            {
                Console.WriteLine("未找到客户端状态");
                return;
            }
           
            ServerType serverType = (ServerType)receiveBuf[0];

            IPEndPoint serverIpendPoint = (IPEndPoint)type2ss[serverType].socket.RemoteEndPoint;
            byte[] sendBytes = new byte[receiveBuf.Length + 3];

            sendBytes[0] = (byte)(state.guid >> 24);
            sendBytes[1] = (byte)((state.guid >> 16) & 0xff);
            sendBytes[2] = (byte)((state.guid >> 8) & 0xff);
            sendBytes[3] = (byte)(state.guid & 0xff);

            Array.Copy(receiveBuf, 1, sendBytes, 4, receiveBuf.Length - 1);

            receiveServerUdp.Send(sendBytes, sendBytes.Length, serverIpendPoint);

            receiveClientUdp.BeginReceive(ReceiveUdpClientCallback, state);
        }
    }
    private static void ReceiveUdpServerCallback(IAsyncResult ar)
    {
        ServerState state = (ServerState)ar.AsyncState;

        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receiveBuf = receiveServerUdp.EndReceive(ar, ref iPEndPoint);

        uint guid = (uint)(receiveBuf[0] << 24 |
                    receiveBuf[1] << 16 |
                    receiveBuf[2] << 8 |
                    receiveBuf[3]);

        if(!id2cs.ContainsKey(guid))
            return;
        IPEndPoint clientIpEndPoint = (IPEndPoint)id2cs[guid].socket.RemoteEndPoint;

        Array.Copy(receiveBuf, 4, receiveBuf, 0, receiveBuf.Length - 4);

        receiveClientUdp.Send(receiveBuf, receiveBuf.Length - 4, clientIpEndPoint);

        receiveServerUdp.BeginReceive(ReceiveUdpServerCallback, state);

    }
    #endregion
}
