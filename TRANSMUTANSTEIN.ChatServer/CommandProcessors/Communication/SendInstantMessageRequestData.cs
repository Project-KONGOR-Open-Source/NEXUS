namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

public class SendInstantMessageRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
    public string TargetName { get; } = buffer.ReadString();
    public string Message { get; } = buffer.ReadString();
    public byte Flags { get; } = buffer.ReadInt8(); // 1 = Request Echo/Saved History? 
}