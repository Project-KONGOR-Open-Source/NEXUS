namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SET_PASSWORD)]
public class SetChannelPassword : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        SetChannelPasswordRequestData requestData = new (buffer);

        ChatChannel channel = ChatChannel.Get(session, requestData.ChannelID);

        channel.SetPassword(session, requestData.Password);
    }
}

public class SetChannelPasswordRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ChannelID = buffer.ReadInt32();

    public string Password = buffer.ReadString();
}
