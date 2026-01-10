namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class SetChannelTopicRequestData
{
    public SetChannelTopicRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        Topic = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }
    public int ChannelID { get; }
    public string Topic { get; }
}
