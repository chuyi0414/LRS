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
    private Uri serverUri = new Uri("ws://127.0.0.1:9000/lrs"); // WebSocket �������ĵ�ַ
    private CancellationTokenSource cts;
    private const int HeartbeatInterval = 100000; // �����������λΪ����

    async void Start()
    {
        GameEntry.Event.Subscribe(WebSocketSendxxEventArgs.EventId, OnSendxxx);
        GameEntry.Event.Subscribe(WebSocketReceivexxEventArgs.EventId, OnReceivexx);
        cts = new CancellationTokenSource();
        webSocket = new ClientWebSocket();

        try
        {
            // ���ӵ� WebSocket ������
            await webSocket.ConnectAsync(serverUri, cts.Token);
            Debug.Log("�����ӵ�������!");

            // ��ʼ������Ϣ
            ReceiveMessages();

            // ��ʼ����������Ϣ
            //StartHeartbeat();
            /*SendMessage(new Dictionary<string, object>
            {
                {
                    "type","����"
                }
                ,
                {
                    "data","���"
                }
            });*/
        }
        catch (Exception ex)
        {
            Debug.LogError($"�쳣: {ex.Message}");
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
            // �ر� WebSocket ����
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "�ر�", cts.Token);
            webSocket.Dispose();
        }
        cts.Cancel();
        cts.Dispose();
    }

    // �����ķ�����������Ϣ������ Dictionary ����
    public async void SendMessage(Dictionary<string, object> message)
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            try
            {
                // �� Dictionary �������л�Ϊ JSON �ַ���
                string jsonMessage = JsonConvert.SerializeObject(message);
                byte[] buffer = CustomEncode(jsonMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);
                Debug.Log("�ѷ�����Ϣ: " + jsonMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"������Ϣ�쳣: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("WebSocket δ���ӻ��ѹرգ��޷�������Ϣ��");
        }
    }

    private async void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

            // �����յ���Ϣ�Ƿ�Ϊ��
            if (result.Count == 0)
            {
                Debug.Log("�յ�����Ϣ�����Դ���");
                continue;
            }

            string receivedMessage = CustomDecode(buffer, result.Count);

            // ���� JSON ��Ϣ
            if (!string.IsNullOrEmpty(receivedMessage))
            {
                try
                {
                    // �����л�Ϊ Dictionary ����
                    Dictionary<string, object> receivedObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedMessage);
                    Debug.Log("�ӷ��������յ�: " + JsonConvert.SerializeObject(receivedObject, Formatting.Indented));
                    GameEntry.Event.Fire(this,WebSocketReceivexxEventArgs.Create(receivedObject));
                }
                catch (JsonException jsonEx)
                {
                    Debug.LogError($"�����л� JSON �쳣: {jsonEx.Message}");
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
                // ������Ϣ��װΪ Dictionary ����
                var heartbeatMessage = new Dictionary<string, object> { { "type", "PING" } };

                // �� Dictionary �������л�Ϊ JSON �ַ���
                string jsonHeartbeat = JsonConvert.SerializeObject(heartbeatMessage);

                // ����������Ϣ
                byte[] heartbeatBuffer = CustomEncode(jsonHeartbeat);
                await webSocket.SendAsync(new ArraySegment<byte>(heartbeatBuffer), WebSocketMessageType.Text, true, cts.Token);
                Debug.Log("�ѷ�������: " + jsonHeartbeat);
                // �ȴ���һ���������
                await Task.Delay(HeartbeatInterval);
            }
            catch (Exception ex)
            {
                Debug.LogError($"�����쳣: {ex.Message}");
                break;
            }
        }
    }

    private byte[] CustomEncode(string message)
    {
        // �Զ�������߼���UTF-8��
        return Encoding.UTF8.GetBytes(message);
    }

    private string CustomDecode(byte[] buffer, int count)
    {
        // �Զ�������߼���UTF-8��
        return Encoding.UTF8.GetString(buffer, 0, count);
    }
}
