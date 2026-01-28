namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class ChatEmoteRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string Message { get; } = buffer.ReadString();

    public int ChannelID { get; } = buffer.ReadInt32();
}
