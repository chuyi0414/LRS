using GameFramework;
using GameFramework.Event;
using System.Collections.Generic;

public class WebSocketSendxxEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(WebSocketSendxxEventArgs).GetHashCode();

    // 私有字段来存储消息
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

    // 创建 WebSocketxxEventArgs 实例的方法
    public static WebSocketSendxxEventArgs Create(Dictionary<string, object> message)
    {
        WebSocketSendxxEventArgs e = ReferencePool.Acquire<WebSocketSendxxEventArgs>();
        e.Message = message;
        return e;
    }

    // 重置事件参数的方法
    public override void Clear()
    {
        _message = null;
    }
}
