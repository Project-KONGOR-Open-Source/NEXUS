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

file class SetChannelPasswordRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public string Password { get; init; }

    public SetChannelPasswordRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        Password = buffer.ReadString();
    }
}
