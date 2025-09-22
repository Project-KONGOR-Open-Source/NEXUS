namespace TRANSMUTANSTEIN.ChatServer.Communication;

public class ChatChannel
{
    public int ID => Name.GetDeterministicInt32Hash();

    public required string Name { get; set; }

    public string Topic => $"Welcome To The {Name} Channel!";

    public required ChatProtocol.ChatChannelType Flags { get; set; }

    public ConcurrentDictionary<string, ChatChannelMember> Administrators { get; set; } = [];

    public ConcurrentDictionary<string, ChatChannelMember> Members { get; set; } = [];

    public List<ChatChannelMember> MembersAndAdministrators => [.. Members.Values, .. Administrators.Values];

    public static ChatChannel GetOrCreate(JoinChannelRequestData requestData, ChatSession session)
    {
        bool isClanChannel = session.ClientInformation.Account.Clan is not null && requestData.Channel == $"Clan {session.ClientInformation.Account.Clan.Name}";

        ChatProtocol.ChatChannelType chatChannelType = isClanChannel
            ? ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN
            : ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT;

        ChatChannel channel = Context.ChatChannels.GetOrAdd(requestData.Channel, new ChatChannel { Name = requestData.Channel, Flags = chatChannelType });

        return channel;
    }

    public ChatChannel Join(ChatSession session)
    {
        ChatChannelMember newMember = new (session);

        if (newMember.AdministratorLevel is ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE)
            Members.TryAdd(session.ClientInformation.Account.Name, newMember);

        if (newMember.AdministratorLevel is not ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE)
            Administrators.TryAdd(session.ClientInformation.Account.Name, newMember);

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);

        response.WriteString(Name);                                               // Channel Name
        response.WriteInt32(ID);                                                  // Channel ID
        response.WriteInt8(Convert.ToByte(Flags));                                // Channel Flags
        response.WriteString(Topic);                                              // Channel Topic

        response.WriteInt32(Administrators.Count);                                // Count Of Channel Administrators

        foreach (ChatChannelMember administrator in Administrators.Values)
        {
            response.WriteInt32(administrator.Account.ID);                        // Administrator Account ID
            response.WriteInt8(Convert.ToByte(administrator.AdministratorLevel)); // Channel Administrator Level
        }

        response.WriteInt32(Members.Count);                                       // Count Of Channel Members

        foreach (ChatChannelMember member in Members.Values)
        {
            response.WriteString(member.Account.NameWithClanTag);                 // Member Account Name
            response.WriteInt32(member.Account.ID);                               // Member Account ID
            response.WriteInt8(Convert.ToByte(member.ConnectionStatus));          // Connection Status
            response.WriteInt8(Convert.ToByte(member.AdministratorLevel));        // Channel Administrator Level
            response.WriteString(member.ChatSymbol);                              // Chat Symbol
            response.WriteString(member.NameColour);                              // Name Colour
            response.WriteString(member.AccountIcon);                             // Account Icon
            response.WriteInt32(member.Account.AscensionLevel);                   // Ascension Level
        }

        response.PrependBufferSize();

        // Announce To The Requesting Client That They Have Joined The Channel
        session.SendAsync(response.Data);

        return this;
    }

    public ChatChannel BroadcastJoin(ChatSession session)
    {
        ChatChannelMember newMember = MembersAndAdministrators.Single(member => member.Account.ID == session.ClientInformation.Account.ID);

        List<ChatChannelMember> existingMembers = [..MembersAndAdministrators.Where(member => member.Account.ID != session.ClientInformation.Account.ID)];

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOINED_CHANNEL);

        broadcast.WriteInt32(ID);                                          // Channel ID
        broadcast.WriteString(newMember.Account.NameWithClanTag);          // Member Account Name
        broadcast.WriteInt32(newMember.Account.ID);                        // Member Account ID
        broadcast.WriteInt8(Convert.ToByte(newMember.ConnectionStatus));   // Connection Status
        broadcast.WriteInt8(Convert.ToByte(newMember.AdministratorLevel)); // Channel Administrator Level
        broadcast.WriteString(newMember.ChatSymbol);                       // Chat Symbol
        broadcast.WriteString(newMember.NameColour);                       // Name Colour
        broadcast.WriteString(newMember.AccountIcon);                      // Account Icon
        broadcast.WriteInt32(newMember.Account.AscensionLevel);            // Ascension Level

        broadcast.PrependBufferSize();

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        Parallel.ForEach(existingMembers, (existingMember) => existingMember.Session.SendAsync(broadcast.Data));

        return this;
    }
}
