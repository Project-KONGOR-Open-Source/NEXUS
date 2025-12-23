namespace KINESIS.Client;

public class JoinedChannelResponse : ProtocolResponse
{
    private readonly int _channelId;
    public int ChannelId => _channelId;
    private readonly ChatChannelUser _chatChannelUser;
    public ChatChannelUser ChatChannelUser => _chatChannelUser;

    public JoinedChannelResponse(int channelId, ChatChannelUser chatChannelUser)
    {
        _channelId = channelId;
        _chatChannelUser = chatChannelUser;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.JoinedChatChannel);
        buffer.WriteInt32(_channelId);
        buffer.WriteString(_chatChannelUser.DisplayedName);
        buffer.WriteInt32(_chatChannelUser.AccountId);
        buffer.WriteInt8(Convert.ToByte(_chatChannelUser.ChatClientStatus));
        buffer.WriteInt8(Convert.ToByte(_chatChannelUser.ChatAdminLevel));
        buffer.WriteString(_chatChannelUser.ChatSymbol);
        buffer.WriteString(_chatChannelUser.ChatNameColour);
        buffer.WriteString(_chatChannelUser.AccountIcon);
        buffer.WriteInt32(_chatChannelUser.AscensionLevel);
        return buffer;
    }
}
