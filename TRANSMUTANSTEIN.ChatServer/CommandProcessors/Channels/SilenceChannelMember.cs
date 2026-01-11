namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_USER)]
public class SilenceChannelMember : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SilenceChannelMemberRequestData requestData = new(buffer);

        ChatChannel channel = ChatChannel.Get(session, requestData.ChannelId);

        // Find The Target Account ID By Name
        ChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession =>
                chatSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        if (targetSession is null)
        {
            // TODO: Notify Requester That Target User Was Not Found

            return;
        }

        channel.Silence(session, targetSession.Account.ID, requestData.DurationMilliseconds);
    }
}