namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK)]
public class KickFromChannel : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        KickFromChannelRequestData requestData = new (buffer);

        ChatChannel
            .Get(session, requestData.ChannelID)
            .Kick(session, requestData.TargetAccountID);
    }
}

public class KickFromChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ChannelID = buffer.ReadInt32();

    public int TargetAccountID = buffer.ReadInt32();
}
