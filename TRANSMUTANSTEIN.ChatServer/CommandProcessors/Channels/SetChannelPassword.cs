namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SET_PASSWORD)]
public class SetChannelPassword : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SetChannelPasswordRequestData requestData = new(buffer);

        ChatChannel channel = ChatChannel.Get(session, requestData.ChannelID);

        channel.SetPassword(session, requestData.Password);
    }
}