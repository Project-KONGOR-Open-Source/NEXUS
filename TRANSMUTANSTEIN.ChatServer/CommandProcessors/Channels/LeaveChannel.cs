namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_LEAVE_CHANNEL)]
public class LeaveChannel : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        LeaveChannelRequestData requestData = new(buffer);

        ChatChannel
            .Get(session, requestData.ChannelName)
            .Leave(session);
    }
}

file class LeaveChannelRequestData
{
    public LeaveChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ChannelName { get; }
}