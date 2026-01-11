namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class JoinPasswordProtectedChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string ChannelName { get; } = buffer.ReadString();

    public string Password { get; } = buffer.ReadString();
}