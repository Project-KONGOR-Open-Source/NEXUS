namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Lifts a ban on a player in a channel.
///     C++ reference: <c>c_client.cpp:2167</c> — <c>HandleChannelUnban</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_UNBAN)]
public class UnbanChannelMember : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        UnbanChannelMemberRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.LiftBan(session, requestData.TargetName);
    }
}

file class UnbanChannelMemberRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public string TargetName { get; init; }

    public UnbanChannelMemberRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetName = buffer.ReadString();
    }
}
