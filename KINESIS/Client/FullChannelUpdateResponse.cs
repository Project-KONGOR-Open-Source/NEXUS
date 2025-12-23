namespace KINESIS.Client;

public class FullChannelUpdateResponse : ProtocolResponse
{
    private readonly string _channelName;
    public string ChannelName => _channelName;
    private readonly int _channelId;
    public int ChannelId => _channelId;
    private readonly ChatChannelFlags _chatChannelFlags;
    public ChatChannelFlags ChatChannelFlags => _chatChannelFlags;
    private readonly string _channelTopic;
    public string ChannelTopic => _channelTopic;
    private readonly ChatChannelUser[] _channelUsers;
    public ChatChannelUser[] ChannelUsers => _channelUsers;

    public FullChannelUpdateResponse(string channelName, int channelId, ChatChannelFlags chatChannelFlags, string channelTopic, List<ChatChannelUser> channelUsers)
    {
        _channelName = channelName;
        _channelId = channelId;
        _chatChannelFlags = chatChannelFlags;
        _channelTopic = channelTopic;
        _channelUsers = channelUsers.ToArray(); // make a copy in case the List gets mutated.
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.FullChannelUpdate);
        buffer.WriteString(_channelName);
        buffer.WriteInt32(_channelId);
        buffer.WriteInt8(Convert.ToByte(_chatChannelFlags));
        buffer.WriteString(_channelTopic);

        ChatChannelUser[] admins = _channelUsers.Where(user => user.ChatAdminLevel != ChatAdminLevel.None).ToArray();
        buffer.WriteInt32(admins.Length);
        foreach (ChatChannelUser admin in admins)
        {
            buffer.WriteInt32(admin.AccountId);
            buffer.WriteInt8(Convert.ToByte(admin.ChatAdminLevel));
        }

        buffer.WriteInt32(_channelUsers.Length);
        foreach (var channelUser in _channelUsers)
        {
            buffer.WriteString(channelUser.DisplayedName);
            buffer.WriteInt32(channelUser.AccountId);
            buffer.WriteInt8(Convert.ToByte(channelUser.ChatClientStatus));
            buffer.WriteInt8(Convert.ToByte(channelUser.ChatAdminLevel));
            buffer.WriteString(channelUser.ChatSymbol);
            buffer.WriteString(channelUser.ChatNameColour);
            buffer.WriteString(channelUser.AccountIcon);
            buffer.WriteInt32(channelUser.AscensionLevel);
        }

        return buffer;
    }
}
