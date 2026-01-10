namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class LeaveChannelRequestData
{
    public LeaveChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; }
}
