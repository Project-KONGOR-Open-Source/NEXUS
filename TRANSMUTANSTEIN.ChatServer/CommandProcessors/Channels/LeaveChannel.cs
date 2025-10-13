namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_LEAVE_CHANNEL)]
public class LeaveChannel : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        LeaveChannelRequestData requestData = new (buffer);

        ChatChannel
            .Get(session, requestData.ChannelName)
            .Leave(session);
    }
}

public class LeaveChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string ChannelName = buffer.ReadString();
}
