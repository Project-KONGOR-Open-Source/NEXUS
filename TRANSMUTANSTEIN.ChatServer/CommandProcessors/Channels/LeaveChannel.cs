namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_LEAVE_CHANNEL)]
public class LeaveChannel(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        LeaveChannelRequestData requestData = new(buffer);

        ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelName);

        channel?.Leave(chatContext, session);
    }
}