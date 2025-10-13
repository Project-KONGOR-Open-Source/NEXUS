namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK)]
public class KickFromChannel : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        KickFromChannelRequestData requestData = new (buffer);

        ChatChannel
            .Get(requestData.ChannelID, session)
            .Kick(session.ClientInformation.Account.ID, requestData.KickedAccountID);
    }
}

public class KickFromChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ChannelID = buffer.ReadInt32();

    public int KickedAccountID = buffer.ReadInt32();
}
