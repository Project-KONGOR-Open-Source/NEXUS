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