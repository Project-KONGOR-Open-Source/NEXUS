namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class JoinChannelRequestData
{
    public JoinChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; }
}
