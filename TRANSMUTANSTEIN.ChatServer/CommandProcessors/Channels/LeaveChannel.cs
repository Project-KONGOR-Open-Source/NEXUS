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