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
        Gateway,//���ط�����
        Fighter,//ս��������
    }
    /// <summary>
    /// �������ӿͻ��˵ķ����socket
    /// </summary>
    public static Socket listenfd;
    /// <summary>
    /// ����������������˵�Socket
    /// </summary>
    public static Socket gateway;
    /// <summary>
    /// �ͻ����ֵ�
    /// </summary>
    public static Dictionary<Socket, ClientState> clientStates = new Dictionary<Socket, ClientState>();
    /// <summary>
    /// ����������ֵ�
    /// </summary>
    public static Dictionary<Socket, ServerState> serverStates = new Dictionary<Socket, ServerState>();

    /// <summary>
    /// ���������ͺͷ�������ӳ��
    /// </summary>
    public static Dictionary<ServerType, ServerState> type2ss = new Dictionary<ServerType, ServerState>();
    /// <summary>
    /// ͨ��id�ҵ���Ӧ�ͻ��˵��ֵ�
    /// </summary>
    public static Dictionary<uint, ClientState> id2cs = new Dictionary<uint, ClientState>();
    /// <summary>
    /// ���ڼ����б�
    /// </summary>
    public static List<Socket> sockets = new List<Socket>();
    private static float pingInterval = 2;

    /// <summary>
    /// ���տͻ��˵�udp
    /// </summary>
    private static UdpClient receiveClientUdp;
    /// <summary>
    /// ���շ���˵�udp
    /// </summary>
    private static UdpClient receiveServerUdp;

    private static readonly object udpLock = new object();
    /// <summary>
    /// ���ӷ�����
    /// </summary>
    /// <param name="ip">ip��ַ</param>
    /// <param name="port">�˿ں�</param>
    public static void Connect(string ip, int port)
    {
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress iPAddress = IPAddress.Parse(ip);
        IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
        listenfd.Bind(iPEndPoint);
        listenfd.Listen(0);


        receiveClientUdp = new UdpClient((IPEndPoint)listenfd.LocalEndPoint);

        Console.WriteLine("���ط����������ɹ�");
        while (true)
        {
            sockets.Clear();
            //�ŷ���˵�socket
            sockets.Add(listenfd);
            //�ſͻ��˵�Socket
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
                    //�пͻ���Ҫ����
                    Accept(s);
                }
                else
                {
                    //�ͻ��˷���Ϣ������
                    Receive(s);
                }
            }
            //CheckPing();
        }
    }

    /// <summary>
    /// ��������������
    /// </summary>
    /// <param name="ip">ip��ַ</param>
    /// <param name="port">�˿ں�</param>
    public static ServerState ConnectServer(string ip, int port)
    {
        ServerState serverState = new ServerState();
        gateway = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port);
        gateway.Bind(iPEndPoint);
        gateway.Listen(0);
        Console.WriteLine("ս���������ȴ�����");
        gateway.BeginAccept(AcceptServerCallback, serverState);
        return serverState;
    }
    /// <summary>
    /// ����������������ӵĻص�
    /// </summary>
    /// <param name="ar"></param>
    private static void AcceptServerCallback(IAsyncResult ar)
    {
        //��װ���ӹ����ķ���˶���
        ServerState serverState = (ServerState)ar.AsyncState;
        Socket socket = gateway.EndAccept(ar);
        Console.WriteLine("���ӳɹ�");
        serverState.socket = socket;

        serverStates.Add(socket, serverState);


        //udp
        receiveServerUdp = new UdpClient((IPEndPoint)gateway.LocalEndPoint);
        receiveServerUdp.BeginReceive(ReceiveUdpServerCallback, serverState);

        //������Ϣ
        ByteArray byteArray = serverState.readBuffer;
        socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveServerCallback, serverState);
    }
    /// <summary>
    /// ������������˷���������Ϣ�ص�
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
            Console.WriteLine("Receive fail :���鳤�Ȳ���");
            //�رշ����
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
            //�رշ����
            Close(server);
            return;
        }
        if (count <= 0)
        {
            //Console.WriteLine("Socket Close:" + serverState.socket.RemoteEndPoint.ToString());
            //�رշ����
            //Close();
            return;
        }

        //������չ�������Ϣ
        byteArray.writeIndex += count;
        OnReceiveData(serverState);
        byteArray.MoveBytes();

        server.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveServerCallback, serverState);
    }
    /// <summary>
    /// �������˷���������Ϣ
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
        //��������
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
            //���͸��ͻ��˵�����
            byte[] sendBytes = new byte[msgLength + 2];
            //�������
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

        //��������
        if (byteArray.Length > 2)
        {
            OnReceiveData(serverState);
        }
    }
    /// <summary>
    /// ���տͻ��˵�����
    /// </summary>
    /// <param name="listenfd">����˵�socket</param>
    private static void Accept(Socket listenfd)
    {
        try
        {
            Socket socket = listenfd.Accept();
            // ���������ͻ��˵Ķ���
            ClientState state = new ClientState
            {
                socket = socket,
                guid = MyGuid.GetGuid(),
                lastPingTime = GetTimeStamp()
            };
            // ֱ�Ӵ��� state ������Ϊ�ص���״̬
            receiveClientUdp.BeginReceive(ReceiveUdpClientCallback, state);
            // ����ͻ���״̬
            id2cs.Add(state.guid, state);
            clientStates.Add(socket, state);
            Console.WriteLine("��ȡAccept��guid��" + state.guid + "|ip��" + socket.RemoteEndPoint);

            
        }
        catch (SocketException e)
        {
            Console.WriteLine("Accept ʧ��" + e.Message);
        }
    }
    /// <summary>
    /// ���տͻ��˷���������Ϣ
    /// </summary>
    /// <param name="socket">�ͻ��˵�socket</param>
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
            Console.WriteLine("Receive ʧ��,���鲻����");
            return;
        }
        int count = 0;
        try
        {
            count = socket.Receive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.Remain, 0);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Receive ʧ��," + e.Message);
            Close(socket); // �رտͻ���
            return;
        }
        //�ͻ��������ر�
        if (count <= 0)
        {
            Console.WriteLine("Socket Close :" + socket.RemoteEndPoint.ToString());
            Close(socket); // �رտͻ���
            return;
        }
        readBuffer.writeIndex += count;
        //������Ϣ
        OnReceiveData(state);
        readBuffer.MoveBytes();
    }
    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="state">�ͻ��˶���</param>
    private static void OnReceiveData(ClientState state)
    {
        ByteArray readBuffer = state.readBuffer;
        byte[] bytes = readBuffer.bytes;
        int readIndex = readBuffer.readIndex;

        if (readBuffer.Length <= 2)
            return;
        //�����ܳ���
        short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        //�յ�����Ϣû�н��������Ķ�
        if (readBuffer.Length < length + 2)
            return;

        ServerType serverType = (ServerType)bytes[readIndex + 2];
        readBuffer.readIndex += 3;


        try
        {
            //��ȥһ���ֽڵķ������ţ�����λ��Ϊid �õ����ͳ�ȥ��Ϣ�ĳ���
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

        //��������
        if (readBuffer.Length > 2)
        {
            OnReceiveData(state);
        }
    }
    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="state">�ͻ��˶���</param>
    /// <param name="msgBase">��Ϣ</param>
    public static void Send(ClientState state, MsgBase msgBase)
    {
        if (state == null || !state.socket.Connected)
            return;

        //����
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
            Console.WriteLine("Send ʧ��" + e.Message);
        }
    }
    /// <summary>
    /// �رն�Ӧ�Ŀͻ���
    /// </summary>
    /// <param name="state">�ͻ���</param>
    private static void Close(Socket socket)
    {
        if (serverStates.ContainsKey(socket))
        {
            
        }
        else if (clientStates.ContainsKey(socket))
        {
            Console.WriteLine("�ͻ����ѶϿ�����: " + socket.RemoteEndPoint);
            // ����ͻ��˶Ͽ�����
            ClientState clientState = clientStates[socket];
            // ֪ͨ���з�����
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
    /// ��ȡʱ���
    /// </summary>
    /// <returns>ʱ���</returns>
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
                Console.WriteLine("�������ƣ��Ͽ�����:", state.socket.RemoteEndPoint);
                //�رտͻ���
                //Close(state);
                return;
            }
        }
    }

    #region Udp

    /// <summary>
    /// �����������UDP��Ϣ
    /// </summary>
    /// <param name="serverType">Ҫ���͵ķ�����</param>
    /// <param name="message">Ҫ���͵���Ϣ</param>
    public static void SendUdpToServer(ClientState state , ServerType serverType, MsgBase message)
    {
        // 1. ��ȡĿ�������״̬
        if (!type2ss.TryGetValue(serverType, out ServerState serverState))
        {
            Console.WriteLine($"δ�ҵ�����Ϊ {serverType} �ķ�����״̬");
            return;
        }

        // 2. ���������Ƿ����Ӳ�������Ч�� UDP �˵�
        IPEndPoint serverIpendPoint = serverState.socket?.RemoteEndPoint as IPEndPoint;
        if (serverState == null || serverIpendPoint == null)
        {
            Console.WriteLine($"���������� {serverType} δ���ӻ���Ч�� UDP �˵�");
            return;
        }

        try
        {
            // 3. ׼��Ҫ���͵��ֽ�����
            byte[] nameBytes = MsgBase.EncodeName(message);
            byte[] bodyBytes = MsgBase.Encode(message);
            int len = nameBytes.Length + bodyBytes.Length + 1;
            byte[] sendBytes = new byte[len];
            sendBytes[0] = (byte)serverType;
            Array.Copy(nameBytes, 0, sendBytes, 1, nameBytes.Length);
            Array.Copy(bodyBytes, 0, sendBytes, 1 + nameBytes.Length, bodyBytes.Length);

            // 4. �� GUID ��Ϣд����Ϣ��ǰ4���ֽ�
            // 5. ����������Ϣ���ݸ��Ƶ�����������
            byte[] sendBytes1 = new byte[sendBytes.Length + 3];
            sendBytes1[0] = (byte)(state.guid >> 24);
            sendBytes1[1] = (byte)((state.guid >> 16) & 0xff);
            sendBytes1[2] = (byte)((state.guid >> 8) & 0xff);
            sendBytes1[3] = (byte)(state.guid & 0xff);
            Array.Copy(sendBytes, 1, sendBytes1, 4, sendBytes.Length - 1);

            // 6. ʹ�� UdpClient ������Ϣ
            receiveServerUdp.Send(sendBytes1, sendBytes1.Length, serverIpendPoint);

            Console.WriteLine($"�ر���Ϣ�ѷ��͵����������� {serverType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"���� UDP ��Ϣ������������ {serverType} ʧ��: {ex.Message}");
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
                // ʹ�� IPEndPoint ������ȷ�� state ����
                iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                receiveBuf = receiveClientUdp.EndReceive(ar, ref iPEndPoint);
            }catch (Exception ex)
            {
                return;
            }
            

            // ������ȷ�� ClientState ����
            ClientState state = clientStates.Values
                .FirstOrDefault(cs => ((IPEndPoint)cs.socket.RemoteEndPoint).Address.Equals(iPEndPoint.Address)
                                       && ((IPEndPoint)cs.socket.RemoteEndPoint).Port.Equals(iPEndPoint.Port));

            if (state == null)
            {
                Console.WriteLine("δ�ҵ��ͻ���״̬");
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
