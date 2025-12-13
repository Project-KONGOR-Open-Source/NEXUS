namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL_PASSWORD)]
public class JoinPasswordProtectedChannel : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        JoinPasswordProtectedChannelRequestData requestData = new (buffer);

        ChatChannel
            .GetOrCreate(session, requestData.ChannelName)
            .Join(session, requestData.Password);
    }
}

file class JoinPasswordProtectedChannelRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; init; }

    public string Password { get; init; }

    public JoinPasswordProtectedChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
        Password = buffer.ReadString();
    }
}
