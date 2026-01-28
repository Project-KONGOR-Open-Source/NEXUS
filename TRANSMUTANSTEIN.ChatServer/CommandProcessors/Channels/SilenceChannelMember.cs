namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_USER)]
public class SilenceChannelMember(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SilenceChannelMemberRequestData requestData = new(buffer);

        ChatChannel? channel = ChatChannel.Get(chatContext, session, requestData.ChannelId);

        if (channel is null)
        {
            return;
        }

        // Find The Target Account ID By Name
        // We look in the channel members first as silencing only applies to current members
        ChatChannelMember? targetMember = channel.Members.Values
            .FirstOrDefault(m => m.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        // Fallback: Try stripped name
        if (targetMember is null && requestData.TargetName.Contains(']'))
        {
             int closingBracketIndex = requestData.TargetName.IndexOf(']');
             if (closingBracketIndex >= 0 && closingBracketIndex < requestData.TargetName.Length - 1)
             {
                 string strippedName = requestData.TargetName[(closingBracketIndex + 1)..];
                 
                 targetMember = channel.Members.Values
                    .FirstOrDefault(m => m.Account.Name.Equals(strippedName, StringComparison.OrdinalIgnoreCase));
             }
        }

        if (targetMember is null)
        {
            // TODO: Notify Requester That Target User Was Not Found
            return;
        }

        channel.Silence(session, targetMember.Account.ID, requestData.DurationMilliseconds);
    }
}
