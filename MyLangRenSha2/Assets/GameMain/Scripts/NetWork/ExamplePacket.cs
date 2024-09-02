using GameFramework.Network;

public class ExamplePacket : Packet
{
    public override int Id => 0; // 示例 ID，实际使用时可以根据需求定义

    public int UserId { get; set; }
    public string Message { get; set; }

    public ExamplePacket(int userId, string message)
    {
        UserId = userId;
        Message = message;
    }

    public override string ToString()
    {
        return $"Packet ID: {Id}, UserId: {UserId}, Message: {Message}";
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }
}
