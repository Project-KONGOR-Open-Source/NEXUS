namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL)]
public class JoinChannel : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        JoinChannelRequestData requestData = new (buffer);

        ChatChannel
            .GetOrCreate(session, requestData.ChannelName)
            .Join(session);
    }
}

public class JoinChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string ChannelName = buffer.ReadString();
}
