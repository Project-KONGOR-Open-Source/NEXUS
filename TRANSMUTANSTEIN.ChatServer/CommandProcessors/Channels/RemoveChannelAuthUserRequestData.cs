namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class RemoveChannelAuthUserRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string ChannelName { get; } = buffer.ReadString();

    public string TargetName { get; } = buffer.ReadString();
}
