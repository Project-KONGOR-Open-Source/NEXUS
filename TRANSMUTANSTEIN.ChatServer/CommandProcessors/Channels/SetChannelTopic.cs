namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_TOPIC)]
public class SetChannelTopic(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SetChannelTopicRequestData requestData = new(buffer);

        // Find Channel
        ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelId);

        // If Channel Not Found or User Not Member (Get checks membership), do nothing?
        // Legacy: ChannelTopicRequest checks if channel exists.

        if (channel is null)
        {
            return;
        }

        // Check Permissions
        // ChatChannel has members. Need to check if user is admin/op.
        // ChatChannel.Members is ConcurrentDictionary<string, ChatChannelMember>

        if (channel.Members.TryGetValue(session.Account.Name, out ChatChannelMember? member) is false)
        {
            return;
        }

        // Legacy: if (!member.IsOp && !member.IsAdmin && !member.IsStaff) -> Rejected.
        // ChatChannelMember has IsAdministrator (level >= Officer).

        if (member.HasElevatedPrivileges() is false)
        {
            // TODO: Send Error? Legacy sends nothing explicit here usually, or a generic error.
            return;
        }

        // Update Topic
        channel.Topic = requestData.Topic;

        // Broadcast New Topic To Channel
        ChatBuffer broadcast = new();
        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_TOPIC);
        broadcast.WriteInt32(channel.ID);
        broadcast.WriteString(requestData.Topic);

        channel.BroadcastMessage(broadcast);
    }
}