namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

/// <summary>
///     Sets a channel's topic.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_TOPIC)]
public class ChannelTopic : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ChannelTopicRequestData requestData = new (buffer);

        ChatChannel? channel = Context.ChatChannels.Values
            .SingleOrDefault(channel => channel.ID == requestData.ChannelID);

        channel?.SetTopic(session, requestData.Topic);
    }
}

file class ChannelTopicRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; init; }

    public string Topic { get; init; }

    public ChannelTopicRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        Topic = buffer.ReadString();
    }
}
