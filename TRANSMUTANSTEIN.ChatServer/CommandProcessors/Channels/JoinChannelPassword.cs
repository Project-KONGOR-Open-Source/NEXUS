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

public class JoinPasswordProtectedChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string ChannelName = buffer.ReadString();

    public string Password = buffer.ReadString();
}
