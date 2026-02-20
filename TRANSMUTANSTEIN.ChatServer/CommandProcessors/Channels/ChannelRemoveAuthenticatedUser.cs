namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Removes a user from a channel's authenticated user list.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_REMOVE_AUTH_USER)]
public class ChannelRemoveAuthenticatedUser : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ChannelRemoveAuthenticatedUserRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.RemoveAuthenticatedUser(session, requestData.TargetName);
    }
}

file class ChannelRemoveAuthenticatedUserRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public string TargetName { get; init; }

    public ChannelRemoveAuthenticatedUserRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetName = buffer.ReadString();
    }
}
