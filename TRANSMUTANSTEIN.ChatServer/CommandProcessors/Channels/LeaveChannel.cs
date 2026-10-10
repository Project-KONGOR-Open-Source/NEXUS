namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_LEAVE_CHANNEL)]
public class LeaveChannel : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        LeaveChannelRequestData requestData = new (buffer);

        // The Game Client Sends Leave Requests Without Checking Whether It Is In The Channel (e.g. For Every Saved Channel When Clearing Its Auto-Connect List), So Requests For Channels The Client Is Not In Are Ignored
        if (ChatChannel.TryGet(session, requestData.ChannelName, out ChatChannel? channel))
            channel.Leave(session);
    }
}

file class LeaveChannelRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; init; }

    public LeaveChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
    }
}
