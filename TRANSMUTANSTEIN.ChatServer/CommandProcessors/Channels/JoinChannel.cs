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