namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class GetChannelListRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
}
