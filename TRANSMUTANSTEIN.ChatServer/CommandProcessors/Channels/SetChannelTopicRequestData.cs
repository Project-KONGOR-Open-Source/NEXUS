namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class SetChannelTopicRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
    public int ChannelId { get; } = buffer.ReadInt32();
    public string Topic { get; } = buffer.ReadString();
}