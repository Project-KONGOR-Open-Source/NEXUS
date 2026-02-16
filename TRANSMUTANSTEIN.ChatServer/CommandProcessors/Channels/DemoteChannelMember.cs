namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Demotes a member's admin level in a channel.
///     C++ reference: <c>c_client.cpp:2224</c> — <c>HandleChannelDemote</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_DEMOTE)]
public class DemoteChannelMember : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        DemoteChannelMemberRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.Demote(session, requestData.TargetAccountID);
    }
}

file class DemoteChannelMemberRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public int TargetAccountID { get; init; }

    public DemoteChannelMemberRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetAccountID = buffer.ReadInt32();
    }
}
