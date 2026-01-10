namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL_PASSWORD)]
public class JoinPasswordProtectedChannel : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        JoinPasswordProtectedChannelRequestData requestData = new(buffer);

        ChatChannel
            .GetOrCreate(session, requestData.ChannelName)
            .Join(session, requestData.Password);
    }
}

file class JoinPasswordProtectedChannelRequestData
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