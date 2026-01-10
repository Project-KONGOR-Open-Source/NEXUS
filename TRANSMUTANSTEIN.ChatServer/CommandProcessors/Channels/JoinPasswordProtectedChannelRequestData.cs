namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class JoinPasswordProtectedChannelRequestData
{
    public JoinPasswordProtectedChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
        Password = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; }

    public string Password { get; }
}
