namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

public class SendInstantMessageRequestData
{
    public SendInstantMessageRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
        Flags = buffer.ReadInt8();
    }

    public byte[] CommandBytes { get; init; }
    public string TargetName { get; }
    public string Message { get; }
    public byte Flags { get; } // 1 = Request Echo/Saved History? 
}
