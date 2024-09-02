using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
#nullable disable

public static class NetManager
{

    public static Socket listenfd;
    public static Dictionary<Socket,ClientState> stats = new Dictionary<Socket,ClientState>();
    public static List<Socket> Sockets = new List<Socket>();

    private static float pingInterval = 1000;

    public static void Connect(string ip,int port)
    {
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress iPAddress = IPAddress.Parse(ip);
        IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
        listenfd.Bind(iPEndPoint);
        listenfd.Listen(0);

        Console.WriteLine("�����������ɹ�");

        while (true)
        {
            Sockets.Clear();
            //����˵�
            Sockets.Add(listenfd);
            //�ͻ��˵�
            foreach (Socket sock in stats.Keys)
            {
                Sockets.Add(sock);
            }
            Socket.Select(Sockets,null,null,1000);
            for(int i =0;i<Sockets.Count;i++)
            {
                Socket s = Sockets[i];
                if(s==listenfd)
                {
                    Accept(s);
                }
                else
                {
                    Receive(s);
                }
            }
            CheckPing();
        }
    }

    private static void Receive(Socket socket)
    {
        ClientState state= stats[socket];
        ByteArray readBuffer = state.readBuffer;

        if(readBuffer.Remain<=0)
        {
            readBuffer.MoveBytes();
        }
        if (readBuffer.Remain <= 0)
        {
            Console.WriteLine("Receive ʧ��");
        }

        int count = 0;
        try
        {
            count = socket.Receive(readBuffer.bytes,readBuffer.writeIndex,
                readBuffer.Remain,0);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Receive ʧ��" + e.Message);
            return;
        }

        if(count<=0)
        {
            Console.WriteLine("�ͻ��˹ر�"+socket.RemoteEndPoint.ToString());
        }

        readBuffer.writeIndex += count;
        //������Ϣ
        OnReceiveData(state);
        readBuffer.MoveBytes();
    }

    private static void OnReceiveData(ClientState state)
    {
        ByteArray readBuffer = state.readBuffer;
        byte[] bytes = readBuffer.bytes;
        int readIndex = readBuffer.readIndex;

        if (readBuffer.Length <= 2)
            return;
        //�����ܳ���
        short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        if(readBuffer.Length<length)
            return;
        readBuffer.readIndex += 2;

        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuffer.bytes,readBuffer.readIndex,out nameCount);
        if(protoName=="")
        {
            Console.WriteLine("OnReceiveData ʧ��,Э����Ϊ��");
            return;
        }
        readBuffer.readIndex += nameCount;

        int bodyLength = length - nameCount;
        MsgBase msgBase =MsgBase.Decode(protoName, readBuffer.bytes, readBuffer.readIndex, bodyLength);
        readBuffer.readIndex += bodyLength;
        readBuffer.MoveBytes();

        //MsgTest msg = (MsgTest)msgBase;
        //Console.WriteLine(msg.protoName);
        //Send(state,msg);

        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);
        Console.WriteLine("protoName:"+ protoName);
        if(mi != null)
        {
            object[] o = { state, msgBase };
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("����ʧ��" );
        }

        if(readBuffer.Length>2)
        {
            OnReceiveData(state);
        }
    }

    private static void Accept(Socket listenfd)
    {
        try
        {
            Socket socket = listenfd.Accept();
            Console.WriteLine("Accept:" + socket.RemoteEndPoint);
            //���������ͻ��˶���
            ClientState state = new ClientState();
            state.socket = socket;

            state.lastPingTime = GetTimeStamp();
            stats.Add(socket, state);

        }
        catch(SocketException e)
        {
            Console.WriteLine("Accept ʧ��"+ e.Message);
            return;
        }
    }

    public static void Send(ClientState state,MsgBase msgBase)
    {
        if (state == null || !state.socket.Connected)
            return;

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
            state.socket.Send(sendBytes,0,sendBytes.Length,0);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Send ʧ��" + e.Message);
            return;
        }
    }

    private static void Close(ClientState state)
    {
        state.socket.Close();
        stats.Remove(state.socket);
    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }

    private static void CheckPing()
    {
        foreach(ClientState state in stats.Values)
        {
            if (GetTimeStamp() - state.lastPingTime > (pingInterval * 10))
            {
                Console.WriteLine("�������ƣ��Ͽ�����" + state.socket.RemoteEndPoint);
                Close(state);
                return;
            }
        }
    }
}

public class MsgTest : MsgBase
{
    public MsgTest()
    {
        protoName = "MsgTest";
    }
}