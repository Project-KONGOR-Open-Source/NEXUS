namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class LeaveChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string ChannelName { get; } = buffer.ReadString();
}