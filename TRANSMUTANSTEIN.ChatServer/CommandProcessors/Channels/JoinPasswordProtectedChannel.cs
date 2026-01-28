namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL_PASSWORD)]
public class JoinPasswordProtectedChannel(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        JoinPasswordProtectedChannelRequestData requestData = new(buffer);

        ChatChannel
            .GetOrCreate(chatContext, session, requestData.ChannelName)
            .Join(session, requestData.Password);
    }
}