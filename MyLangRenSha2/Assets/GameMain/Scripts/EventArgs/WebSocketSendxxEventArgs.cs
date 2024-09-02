using GameFramework;
using GameFramework.Event;
using System.Collections.Generic;

public class WebSocketSendxxEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(WebSocketSendxxEventArgs).GetHashCode();

    // ˽���ֶ����洢��Ϣ
    private Dictionary<string, object> _message;

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    public Dictionary<string, object> Message
    {
        get
        {
            return _message;
        }
        set
        {
            _message = value;
        }
    }

    // ���� WebSocketxxEventArgs ʵ���ķ���
    public static WebSocketSendxxEventArgs Create(Dictionary<string, object> message)
    {
        WebSocketSendxxEventArgs e = ReferencePool.Acquire<WebSocketSendxxEventArgs>();
        e.Message = message;
        return e;
    }

    // �����¼������ķ���
    public override void Clear()
    {
        _message = null;
    }
}
