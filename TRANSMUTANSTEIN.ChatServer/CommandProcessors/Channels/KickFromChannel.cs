namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK)]
public class KickFromChannel : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        KickFromChannelRequestData requestData = new(buffer);

        ChatChannel
            .Get(session, requestData.ChannelID)
            .Kick(session, requestData.TargetAccountID);
    }
}