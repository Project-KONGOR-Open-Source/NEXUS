namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class BanFromChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public int ChannelID { get; } = buffer.ReadInt32();

    public string TargetName { get; } = buffer.ReadString();
}
