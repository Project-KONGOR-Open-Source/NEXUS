namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class ChatRollRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string Parameters { get; } = buffer.ReadString();

    public int ChannelID { get; } = buffer.ReadInt32();
}
