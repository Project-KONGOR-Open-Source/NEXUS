namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL)]
public class JoinChannel(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        JoinChannelRequestData requestData = new(buffer);

        ChatChannel
            .GetOrCreate(chatContext, session, requestData.ChannelName)
            .Join(session);
    }
}