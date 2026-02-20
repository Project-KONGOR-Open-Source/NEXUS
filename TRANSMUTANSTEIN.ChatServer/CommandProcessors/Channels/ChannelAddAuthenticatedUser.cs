namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Adds a user to a channel's authenticated user list.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_ADD_AUTH_USER)]
public class ChannelAddAuthenticatedUser : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ChannelAddAuthenticatedUserRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.AddAuthenticatedUser(session, requestData.TargetName);
    }
}

file class ChannelAddAuthenticatedUserRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public string TargetName { get; init; }

    public ChannelAddAuthenticatedUserRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetName = buffer.ReadString();
    }
}
