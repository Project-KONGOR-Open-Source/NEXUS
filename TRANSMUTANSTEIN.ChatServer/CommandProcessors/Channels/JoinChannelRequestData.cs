namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class JoinChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string ChannelName { get; } = buffer.ReadString();
}