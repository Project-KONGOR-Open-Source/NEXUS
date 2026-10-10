namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK)]
public class KickFromChannel : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        KickFromChannelRequestData requestData = new (buffer);

        if (ChatChannel.TryGet(session, requestData.ChannelID, out ChatChannel? channel))
            channel.Kick(session, requestData.TargetAccountID);
    }
}

file class KickFromChannelRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public int TargetAccountID { get; init; }

    public KickFromChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetAccountID = buffer.ReadInt32();
    }
}
