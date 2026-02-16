namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Promotes a member's admin level in a channel.
///     C++ reference: <c>c_client.cpp:2206</c> — <c>HandleChannelPromote</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_PROMOTE)]
public class PromoteChannelMember : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        PromoteChannelMemberRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.Promote(session, requestData.TargetAccountID);
    }
}

file class PromoteChannelMemberRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public int TargetAccountID { get; init; }

    public PromoteChannelMemberRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetAccountID = buffer.ReadInt32();
    }
}
