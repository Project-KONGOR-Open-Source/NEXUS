namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Enables authentication on a channel.
///     C++ reference: <c>c_client.cpp:2275</c> — <c>HandleChannelSetAuth</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SET_AUTH)]
public class ChannelSetAuthentication : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ChannelSetAuthenticationRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.EnableAuthentication(session);
    }
}

file class ChannelSetAuthenticationRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public ChannelSetAuthenticationRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
    }
}
