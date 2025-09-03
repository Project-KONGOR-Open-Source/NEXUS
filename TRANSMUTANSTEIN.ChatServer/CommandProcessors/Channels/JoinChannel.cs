namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL)]
public class JoinChannel(MerrickContext merrick, ILogger<JoinChannel> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;

    private ILogger<JoinChannel> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        JoinChannelRequestData requestData = new (buffer);

        // TODO: Handle Max Channels
        // If the channel has MAX_USERS_PER_CHANNEL or more in it, fails silently.

        // TODO: Handle Channel Passwords And Permissions
        // If the client is in 8 or more channels, fails and the chat server will send CHAT_CMD_MAX_CHANNELS back.
        // If the channel is password protected and the client is not an admin of the channel, fails and the chat server will send CHAT_CMD_JOIN_CHANNEL_PASSWORD back.

        Response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);

        ChatProtocol.ChatChannelType flags;

        if (session.ClientInformation.Account.Clan is not null &&  requestData.Channel == $"Clan {session.ClientInformation.Account.Clan.Name}")
        {
            flags = ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN;
        }

        else
        {
            flags = ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT;
        }

        ChatChannel channel = Context.ChatChannels.GetOrAdd(requestData.Channel, new ChatChannel { Name = requestData.Channel, Flags = flags });

        channel.Members.TryAdd(session.ClientInformation.Account.Name, new ChatChannelMember(session));

        Response.WriteString(channel.Name);                                         // Channel Name
        Response.WriteInt32(channel.ID);                                            // Channel ID
        Response.WriteInt8(Convert.ToByte(channel.Flags));                          // Channel Flags
        Response.WriteString($@"Welcome To The ""{channel.Name}"" Channel");        // Channel Topic

        Response.WriteInt32(channel.Administrators.Count);                          // Count Of Channel Administrators

        foreach (ChatChannelMember administrator in channel.Administrators.Values)
        {
            Response.WriteInt32(administrator.Account.ID);                          // Administrator Account ID
            Response.WriteInt8(Convert.ToByte(administrator.AdministratorLevel));   // Channel Administrator Level
        }

        Response.WriteInt32(channel.Members.Count);                                 // Count Of Channel Members

        foreach (ChatChannelMember member in channel.Members.Values)
        {
            Response.WriteString(member.Account.NameWithClanTag);                   // Member Account Name
            Response.WriteInt32(member.Account.ID);                                 // Member Account ID
            Response.WriteInt8(Convert.ToByte(member.ConnectionStatus));            // Connection Status
            Response.WriteInt8(Convert.ToByte(member.AdministratorLevel));          // Channel Administrator Level
            Response.WriteString(member.ChatSymbol);                                // Chat Symbol
            Response.WriteString(member.NameColour);                                // Name Colour
            Response.WriteString(member.AccountIcon);                               // Account Icon
            Response.WriteInt32(member.Account.AscensionLevel);                     // Ascension Level
        }

        Response.PrependBufferSize();

        // Announce To The Requesting Client That They Have Joined The Channel
        session.SendAsync(Response.Data);

        ChatChannelMember newMember = channel.Members.Values.Single(member => member.Account.ID == session.ClientInformation.Account.ID);
        IEnumerable<ChatChannelMember> existingMembers = channel.Members.Values.Where(member => member.Account.ID != session.ClientInformation.Account.ID);

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOINED_CHANNEL);

        broadcast.WriteInt32(channel.ID);                                   // Channel ID
        broadcast.WriteString(newMember.Account.NameWithClanTag);           // Member Account Name
        broadcast.WriteInt32(newMember.Account.ID);                         // Member Account ID
        broadcast.WriteInt8(Convert.ToByte(newMember.ConnectionStatus));    // Connection Status
        broadcast.WriteInt8(Convert.ToByte(newMember.AdministratorLevel));  // Channel Administrator Level
        broadcast.WriteString(newMember.ChatSymbol);                        // Chat Symbol
        broadcast.WriteString(newMember.NameColour);                        // Name Colour
        broadcast.WriteString(newMember.AccountIcon);                       // Account Icon
        broadcast.WriteInt32(newMember.Account.AscensionLevel);             // Ascension Level

        broadcast.PrependBufferSize();

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        Parallel.ForEach(existingMembers, (existingMember) => { existingMember.Session.SendAsync(broadcast.Data); });
    }
}

public class JoinChannelRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string Channel = buffer.ReadString();
}
