namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class SetChannelPasswordRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public int ChannelId { get; } = buffer.ReadInt32();

    public string Password { get; } = buffer.ReadString();
}