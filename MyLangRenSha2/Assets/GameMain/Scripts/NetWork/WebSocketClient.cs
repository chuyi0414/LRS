using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using UnityGameFramework.Runtime;
using System.Collections.Generic;
using GameFramework.Event;

public class WebSocketClient 
{
    private ClientWebSocket webSocket;
    private Uri serverUri = new Uri("ws://127.0.0.1:9000/lrs"); // WebSocket 服务器的地址
    private CancellationTokenSource cts;
    private const int HeartbeatInterval = 100000; // 心跳间隔，单位为毫秒

    async void Start()
    {
        GameEntry.Event.Subscribe(WebSocketSendxxEventArgs.EventId, OnSendxxx);
        GameEntry.Event.Subscribe(WebSocketReceivexxEventArgs.EventId, OnReceivexx);
        cts = new CancellationTokenSource();
        webSocket = new ClientWebSocket();

        try
        {
            // 连接到 WebSocket 服务器
            await webSocket.ConnectAsync(serverUri, cts.Token);
            Debug.Log("已连接到服务器!");

            // 开始接收消息
            ReceiveMessages();

            // 开始发送心跳消息
            //StartHeartbeat();
            /*SendMessage(new Dictionary<string, object>
            {
                {
                    "type","测试"
                }
                ,
                {
                    "data","你好"
                }
            });*/
        }
        catch (Exception ex)
        {
            Debug.LogError($"异常: {ex.Message}");
        }
    }

    private void OnReceivexx(object sender, GameEventArgs e)
    {
        
    }

    private void OnSendxxx(object sender, GameEventArgs e)
    {
        WebSocketSendxxEventArgs ne = (WebSocketSendxxEventArgs)e;
        SendMessage(ne.Message);
    }

    async void OnDestroy()
    {
        if (webSocket != null)
        {
            // 关闭 WebSocket 连接
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭", cts.Token);
            webSocket.Dispose();
        }
        cts.Cancel();
        cts.Dispose();
    }

    // 公开的方法来发送消息，传入 Dictionary 对象
    public async void SendMessage(Dictionary<string, object> message)
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            try
            {
                // 将 Dictionary 对象序列化为 JSON 字符串
                string jsonMessage = JsonConvert.SerializeObject(message);
                byte[] buffer = CustomEncode(jsonMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);
                Debug.Log("已发送消息: " + jsonMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"发送消息异常: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("WebSocket 未连接或已关闭，无法发送消息。");
        }
    }

    private async void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

            // 检查接收的消息是否为空
            if (result.Count == 0)
            {
                Debug.Log("收到空消息，忽略处理");
                continue;
            }

            string receivedMessage = CustomDecode(buffer, result.Count);

            // 处理 JSON 消息
            if (!string.IsNullOrEmpty(receivedMessage))
            {
                try
                {
                    // 反序列化为 Dictionary 对象
                    Dictionary<string, object> receivedObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedMessage);
                    Debug.Log("从服务器接收到: " + JsonConvert.SerializeObject(receivedObject, Formatting.Indented));
                    GameEntry.Event.Fire(this,WebSocketReceivexxEventArgs.Create(receivedObject));
                }
                catch (JsonException jsonEx)
                {
                    Debug.LogError($"反序列化 JSON 异常: {jsonEx.Message}");
                }
            }
        }
    }

    private async void StartHeartbeat()
    {
        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                // 心跳消息封装为 Dictionary 对象
                var heartbeatMessage = new Dictionary<string, object> { { "type", "PING" } };

                // 将 Dictionary 对象序列化为 JSON 字符串
                string jsonHeartbeat = JsonConvert.SerializeObject(heartbeatMessage);

                // 发送心跳消息
                byte[] heartbeatBuffer = CustomEncode(jsonHeartbeat);
                await webSocket.SendAsync(new ArraySegment<byte>(heartbeatBuffer), WebSocketMessageType.Text, true, cts.Token);
                Debug.Log("已发送心跳: " + jsonHeartbeat);
                // 等待下一个心跳间隔
                await Task.Delay(HeartbeatInterval);
            }
            catch (Exception ex)
            {
                Debug.LogError($"心跳异常: {ex.Message}");
                break;
            }
        }
    }

    private byte[] CustomEncode(string message)
    {
        // 自定义编码逻辑（UTF-8）
        return Encoding.UTF8.GetBytes(message);
    }

    private string CustomDecode(byte[] buffer, int count)
    {
        // 自定义解码逻辑（UTF-8）
        return Encoding.UTF8.GetString(buffer, 0, count);
    }
}
