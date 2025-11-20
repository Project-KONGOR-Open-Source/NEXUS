namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_USER)]
public class SilenceChannelMember : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SilenceChannelMemberRequestData requestData = new (buffer);

        ChatChannel channel = ChatChannel.Get(session, requestData.ChannelID);

        // Find The Target Account ID By Name
        ChatSession? targetSession = Context.ChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        if (targetSession is null)
        {
            // TODO: Notify Requester That Target User Was Not Found

            return;
        }

        channel.Silence(session, targetSession.Account.ID, requestData.DurationMilliseconds);
    }
}

public class SilenceChannelMemberRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ChannelID = buffer.ReadInt32();

    public string TargetName = buffer.ReadString();

    public int DurationMilliseconds = buffer.ReadInt32();
}
