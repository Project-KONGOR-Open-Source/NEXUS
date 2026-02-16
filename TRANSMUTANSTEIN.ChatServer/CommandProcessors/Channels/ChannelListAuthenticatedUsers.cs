namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Returns the authenticated user list for a channel.
///     C++ reference: <c>c_client.cpp:2343</c> — <c>HandleChannelListAuth</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_LIST_AUTH)]
public class ChannelListAuthenticatedUsers : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ChannelListAuthenticatedUsersRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.SendAuthenticatedUserList(session);
    }
}

file class ChannelListAuthenticatedUsersRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public ChannelListAuthenticatedUsersRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
    }
}
