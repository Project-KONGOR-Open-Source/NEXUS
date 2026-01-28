namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_PROMOTE)]
public class PromoteChannelMember(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        PromoteChannelMemberRequestData requestData = new(buffer);

        ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelID);

        channel?.Promote(session, requestData.TargetAccountID);
    }
}
