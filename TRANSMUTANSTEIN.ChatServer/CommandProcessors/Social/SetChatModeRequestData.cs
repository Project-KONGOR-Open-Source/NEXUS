namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class SetChatModeRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
    public byte ChatModeType { get; } = buffer.ReadInt8();
    public string Reason { get; } = buffer.ReadString();
}