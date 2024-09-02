
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using static NetManager;
#nullable disable

public static class NetManager
{
    public enum ServerType
    {
        Gateway,//����
        Fighter,//�߼�

    }

    private static Socket socket;

    private static ByteArray byteArray;
    private static Queue<ByteArray> writeQueue;
    private static int processMsgCount = 10;
    //�Ƿ���������
    private static bool isUsePing = true;
    //��һ�η���ping��ʱ��
    private static float lastPingTime = 0;
    //��һ���յ�pong��ʱ��
    private static float lastPongTime = 0;
    private static float pingInterval = 2;

    private static bool isConnecting;
    private static bool isClosing;

    private static UdpClient udpClient;

    private static System.Timers.Timer reconnectTimer;

    //��Ϣί��
    public delegate void MsgListener(MsgBase msgBase);
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    public static void AddMsgListener(string msgName, MsgListener msgListener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += msgListener;
        }
        else
        {
            msgListeners.Add(msgName, msgListener);
        }
    }

    public static void RemoveMsgListener(string msgName, MsgListener msgListener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= msgListener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    public static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }

    public static void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;

    }

    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Console.WriteLine("�����ӣ������ظ�����");
            return;
        }
        if (isConnecting)
        {
            Console.WriteLine("����ʧ�ܣ���������");
            return;
        }

        // ��ʼ����ʱ��  
        if (reconnectTimer == null)
        {
            reconnectTimer = new System.Timers.Timer(5000); // ���ö�ʱ�����Ϊ5��  
            reconnectTimer.Elapsed += (sender, e) =>
            {
                // ������������  
                reconnectTimer.Stop(); // ֹͣ��ʱ���Ա����ظ�����  
                Connect(ip, port);
            };
        }
        reconnectTimer.Start(); // ������ʱ��  

        Init();
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Console.WriteLine("���ӳɹ�");
            isConnecting = false;
            reconnectTimer.Stop(); // ���ӳɹ���ֹͣ��ʱ��  

            udpClient = new UdpClient((IPEndPoint)socket.LocalEndPoint);
            udpClient.Connect((IPEndPoint)socket.RemoteEndPoint);
            udpClient.BeginReceive(ReceiveUdpCallback, null);

            // ������Ϣ  
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Console.WriteLine("����ʧ��" + e.Message);
            isConnecting = false;
            // ����Ҫ������ֹͣ��ʱ������Ϊ��ʱ��������һ�μ��������������  
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //���յ�������
            int count = socket.EndReceive(ar);
            //�Ͽ�����
            if (count == 0)
            {
                Close();
                return;
            }
            //��������
            byteArray.writeIndex += count;

            //������Ϣ
            OnReceiveData();
            //������ȹ�С������
            if (byteArray.Remain < 8)
            {
                byteArray.MoveBytes();
                byteArray.ReSize(byteArray.Length * 2);
            }
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Console.WriteLine("����ʧ��" + e.Message);
        }
    }

    private static void OnReceiveData()
    {
        if (byteArray.Length <= 2)
            return;
        byte[] bytes = byteArray.bytes;
        int readIndex = byteArray.readIndex;
        //������Ϣ����ĳ���
        short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);


        if (byteArray.Length < length + 2)
            return;
        uint guid = (uint)(bytes[readIndex + 2] << 24 |
                    bytes[readIndex + 3] << 16 |
                    bytes[readIndex + 4] << 8 |
                    bytes[readIndex + 5]);
        byteArray.readIndex += 6;
        //����
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);
        if (protoName == "")
        {
            Console.WriteLine("Э��������ʧ��");
            return;
        }
        byteArray.readIndex += nameCount;


        //����Э����
        int bodyLength = length - nameCount - 4;
        MsgBase msgBase = MsgBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyLength);
        byteArray.readIndex += bodyLength;

        //�ƶ�����
        byteArray.MoveBytes();

        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);
        if (mi != null)
        {
            object[] o = { guid, msgBase };
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("OnReceiveData fail:����ʧ��");
        }

        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }
    }

    private static void Close()
    {
        if (socket != null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
           
        }
    }

    public static void Send(MsgBase msg, uint guid)
    {
        if (socket == null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        if (isClosing)
            return;
        //����
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length + 4;
        byte[] sendBytes = new byte[len + 2];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        sendBytes[2] = (byte)(guid >> 24);
        sendBytes[3] = (byte)((guid >> 16) & 0xff);
        sendBytes[4] = (byte)((guid >> 8) & 0xff);
        sendBytes[5] = (byte)(guid & 0xff);
        Array.Copy(nameBytes, 0, sendBytes, 6, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 6 + nameBytes.Length, bodyBytes.Length);

        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);
    }

    private static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
            return;
        socket.EndSend(ar);
    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }

    #region Udp
    /// <summary>
    /// udp������Ϣ
    /// </summary>
    /// <param name="ar"></param>
    private static void ReceiveUdpCallback(IAsyncResult ar)
    {
        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receiveBuf = udpClient.EndReceive(ar, ref iPEndPoint);

        uint guid = (uint)(receiveBuf[0] << 24 |
                    receiveBuf[1] << 16 |
                    receiveBuf[2] << 8 |
                    receiveBuf[3]);

        int nameCount = 0;
        string protoName = MsgBase.DecodeName(receiveBuf, 4, out nameCount);
        if (protoName == "")
        {
            udpClient.BeginReceive(ReceiveUdpCallback, null);
            Console.WriteLine("����ʧ��");
            return;
        }

        int bodyCount = receiveBuf.Length - nameCount - 4;
        MsgBase msgBase = MsgBase.Decode(protoName, receiveBuf, 4 + nameCount, bodyCount);
        if (msgBase == null)
        {
            udpClient.BeginReceive(ReceiveUdpCallback, null);
            return;
        }

        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);

        if (mi != null)
        {
            object[] o = { guid, msgBase };
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("���ú���ʧ��");
        }
        udpClient.BeginReceive(ReceiveUdpCallback, null);
    }
    /// <summary>
    /// udp����
    /// </summary>
    /// <param name="msg">��Ϣ</param>
    /// <param name="guid">�ͻ��˵�guid</param>
    public static void SendTo(MsgBase msg, uint guid)
    {
        //����
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length + 4;
        byte[] sendBytes = new byte[len];
        //���guid
        sendBytes[0] = (byte)(guid >> 24);
        sendBytes[1] = (byte)((guid >> 16) & 0xff);
        sendBytes[2] = (byte)((guid >> 8) & 0xff);
        sendBytes[3] = (byte)(guid & 0xff);
        //�������������鵱��
        Array.Copy(nameBytes, 0, sendBytes, 4, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 4 + nameBytes.Length, bodyBytes.Length);

        udpClient.Send(sendBytes, sendBytes.Length);
    }
    #endregion
}