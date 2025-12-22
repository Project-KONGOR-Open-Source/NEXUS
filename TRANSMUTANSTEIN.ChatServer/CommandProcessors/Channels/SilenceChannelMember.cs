namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_USER)]
public class SilenceChannelMember : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        SilenceChannelMemberRequestData requestData = new (buffer);

        ChatChannel channel = ChatChannel.Get(session, requestData.ChannelID);

        // Find The Target Account ID By Name
        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        if (targetSession is null)
        {
            // TODO: Notify Requester That Target User Was Not Found

            return;
        }

        channel.Silence(session, targetSession.Account.ID, requestData.DurationMilliseconds);
    }
}

file class SilenceChannelMemberRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public string TargetName { get; init; }

    public int DurationMilliseconds { get; init; }

    public SilenceChannelMemberRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetName = buffer.ReadString();
        DurationMilliseconds = buffer.ReadInt32();
    }
}
