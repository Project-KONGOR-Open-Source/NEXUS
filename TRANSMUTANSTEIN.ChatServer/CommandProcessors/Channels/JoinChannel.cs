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

file class JoinChannelRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; init; }

    public JoinChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
    }
}
