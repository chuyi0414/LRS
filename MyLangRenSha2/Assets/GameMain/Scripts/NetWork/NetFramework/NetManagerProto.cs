/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using ProtoBuf;
using System.Reflection;

public static class NetManagerProto
{
    private static Socket socket;

    private static ByteArray byteArray;
    private static List<IExtensible> msgList;
    private static Queue<ByteArray> writeQueue;
    private static int processMsgCount = 10;
    //是否启用心跳
    private static bool isUsePing = true;
    //上一次发送ping的时间
    private static float lastPingTime = 0;
    //上一次收到pong的时间
    private static float lastPongTime = 0;
    private static float pingInterval = 2;

    private static bool isConnecting;
    private static bool isClosing;
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close,
    }
    //事件委托
    public delegate void EventListener(string err);
    public static Dictionary<NetEvent, EventListener> eventListener = new Dictionary<NetEvent, EventListener>();

    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent] += listener;
        }
        else
        {
            eventListener.Add(netEvent, listener);
        }
    }

    public static void RemoveListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent] -= listener;
            if (eventListener[netEvent] == null)
            {
                eventListener.Remove(netEvent);
            }
        }
    }

    public static void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent](err);
        }
    }

    //消息委托
    public delegate void MsgListener(IExtensible msgBase);
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

    public static void FireMsg(string msgName, IExtensible msgBase)
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
        msgList = new List<IExtensible>();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;

        lastPingTime = Time.time;
        lastPongTime = Time.time;

        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.Log("已连接，不可重复连接");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("连接失败，正在连接");
            return;
        }
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
            Debug.Log("连接成功");
            isConnecting = false;

            FireEvent(NetEvent.ConnectSucc, "");

            //cs
            //MsgTest msg = new MsgTest();
            //Send(msg);

            //接收消息
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0
                , ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("连接失败" + e.Message);
            isConnecting = false;
            FireEvent(NetEvent.ConnectSucc, "");
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            //接收消息
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            //断开连接
            if (count == 0)
            {
                Close();
                return;
            }
            byteArray.writeIndex += count;
            //处理消息
            OnReceiveData();
            //长度过小扩容
            if (byteArray.Remain < 8)
            {
                byteArray.MoveBytes();
                byteArray.ReSize(count * 2);
            }
            //继续接收
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0
                , ReceiveCallback, socket);
        }
        catch (SocketException e)
        {

        }
    }

    private static void OnReceiveData()
    {
        if (byteArray.Length <= 2)
            return;
        byte[] bytes = byteArray.bytes;
        int readIndex = byteArray.readIndex;
        //解析消息总体长度
        short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        if (byteArray.Length < length + 2)
            return;
        byteArray.readIndex += 2;
        int nameCount = 0;
        string protoName = ProtobufTool.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);
        if (protoName == "")
        {
            Debug.Log("协议名解析失败");
            return;
        }
        byteArray.readIndex += nameCount;

        //解析协议体
        int bodyLength = length - nameCount;
        IExtensible msgBase = ProtobufTool.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyLength);
        byteArray.readIndex += bodyLength;

        byteArray.MoveBytes();
        lock (msgList)
        {
            msgList.Add(msgBase);
        }

        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }

        //MsgTest msg = (MsgTest)msgBase;
        //Debug.Log(msg.protoName);
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
            FireEvent(NetEvent.ConnectSucc, "");
        }
    }

    public static void Send(IExtensible msg)
    {
        if (socket == null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        if (isClosing)
            return;

        byte[] nameBytes = ProtobufTool.EncodeName(msg);
        byte[] bodyBytes = ProtobufTool.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;

        byte[] sendBytes = new byte[len + 2];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);

        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }

        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);
        }
    }

    private static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
            return;

        int count = socket.EndSend(ar);

        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }

        ba.readIndex += count;
        if (ba.Length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.First();
            }
        }
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIndex, ba.Length, 0, SendCallBack, socket);
        }
        if (isClosing)
        {
            socket.Close();
        }
    }

    //处理消息
    public static void MsgUpdate()
    {
        if (msgList.Count == 0)
            return;

        for (int i = 0; i < processMsgCount; i++)
        {
            IExtensible msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[i];
                    msgList.RemoveAt(i);
                }

            }
            if (msgBase != null)
            {
                PropertyInfo info = msgBase.GetType().GetProperty("protoName");
                string s = info.GetValue(msgBase).ToString();
                FireMsg(s, msgBase);
            }
            else
            {
                break;
            }
        }
    }

    private static void PingUpdate()
    {
        if (!isUsePing)
            return;

        if (Time.time - lastPingTime > pingInterval)
        {
            //发送心跳
            proto.MsgPing.MsgPing msg = new proto.MsgPing.MsgPing();
            Send(msg);
            lastPingTime = Time.time;
        }

        //断开处理
        if (Time.time - lastPongTime > pingInterval * 5)
        {
            Close();
        }
    }

    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    private static void OnMsgPong(IExtensible msgBase)
    {
        lastPingTime = Time.time;
    }
}*/