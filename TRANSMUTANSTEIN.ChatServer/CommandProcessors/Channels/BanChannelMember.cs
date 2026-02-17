namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Bans a player from a channel.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN)]
public class BanChannelMember : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        BanChannelMemberRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.Ban(session, requestData.TargetName);
    }
}

file class BanChannelMemberRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public string TargetName { get; init; }

    public BanChannelMemberRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetName = buffer.ReadString();
    }
}
