namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL)]
public class JoinChannel : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        JoinChannelRequestData requestData = new(buffer);

        ChatChannel
            .GetOrCreate(session, requestData.ChannelName)
            .Join(session);
    }
}

file class JoinChannelRequestData
{
    public JoinChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; }
}