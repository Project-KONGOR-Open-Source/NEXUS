namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Disables authentication on a channel.
///     C++ reference: <c>c_client.cpp:2291</c> — <c>HandleChannelRemoveAuth</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_REMOVE_AUTH)]
public class ChannelRemoveAuthentication : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ChannelRemoveAuthenticationRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.DisableAuthentication(session);
    }
}

file class ChannelRemoveAuthenticationRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public ChannelRemoveAuthenticationRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
    }
}
