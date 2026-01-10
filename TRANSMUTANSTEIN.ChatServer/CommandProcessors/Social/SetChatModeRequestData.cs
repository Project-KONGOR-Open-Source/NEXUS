namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class SetChatModeRequestData
{
    public SetChatModeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChatModeType = buffer.ReadInt8();
        Reason = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }
    public byte ChatModeType { get; }
    public string Reason { get; }
}
