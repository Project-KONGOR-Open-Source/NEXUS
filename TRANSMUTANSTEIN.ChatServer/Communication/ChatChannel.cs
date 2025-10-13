namespace TRANSMUTANSTEIN.ChatServer.Communication;

public class ChatChannel
{
    public int ID => Name.GetDeterministicInt32Hash();

    public required string Name { get; set; }

    public string Topic => $"Welcome To The {Name} Channel !";

    public required ChatProtocol.ChatChannelType Flags { get; set; }

    public ConcurrentDictionary<string, ChatChannelMember> Members { get; set; } = [];

    public bool IsFull => (Members.Count < ChatProtocol.MAX_USERS_PER_CHANNEL) is false;

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="GetOrCreate"/> As The Primary Mechanism For Creating Chat Channels
    /// </summary>
    private ChatChannel() { }

    public static ChatChannel GetOrCreate(string channelName, ChatSession session)
    {
        bool isClanChannel = session.ClientInformation.Account.Clan is not null && channelName == session.ClientInformation.Account.Clan.GetChatChannelName();

        ChatProtocol.ChatChannelType chatChannelType = isClanChannel
            ? ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN
            : ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT;

        ChatChannel channel = Context.ChatChannels.GetOrAdd(channelName, new ChatChannel { Name = channelName, Flags = chatChannelType });

        return channel;
    }

    public static ChatChannel Get(OneOf<string, int> channelIdentifier, ChatSession session)
    {
        ChatChannel channel = channelIdentifier.Match
        (
            channelName => Context.ChatChannels.Values
                .Single(channel => channel.Name == channelName && channel.Members.ContainsKey(session.ClientInformation.Account.Name)),

            channelID => Context.ChatChannels.Values
                .Single(channel => channel.ID == channelID && channel.Members.ContainsKey(session.ClientInformation.Account.Name))
        );

        return channel;
    }

    public ChatChannel Join(ChatSession session)
    {
        // TODO: Reject Join Request If Client Is Already In The Channel

        // TODO: Reject Join Request If The Channel Is A Clan Channel And The Client Is Not In The Clan

        // TODO: Reject Join Request If The Channel Has The CHAT_CHANNEL_FLAG_UNJOINABLE Flag

        // TODO: Reject Join Request As Non-Administrator If Channel Is Full

        // TODO: Reject Join Request If Response Buffer Would Overlow With A Data Size Greater Than 16384 Bytes (16 Kilobytes)

        ChatChannelMember newMember = new (session, this);

        if (Members.TryAdd(session.ClientInformation.Account.Name, newMember) is false)
            session.Logger.LogError(@"[BUG] Failed To Add Account ""{AccountName}"" To Channel ""{ChannelName}""", session.ClientInformation.Account.Name, Name);

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);

        response.WriteString(Name);                                               // Channel Name
        response.WriteInt32(ID);                                                  // Channel ID
        response.WriteInt8(Convert.ToByte(Flags));                                // Channel Flags
        response.WriteString(Topic);                                              // Channel Topic

        List<ChatChannelMember> administrators = [.. Members.Values.Where(member => member.IsAdministrator)];

        response.WriteInt32(administrators.Count);                                // Count Of Channel Administrators

        foreach (ChatChannelMember administrator in administrators)
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
            response.WriteString(member.Account.ChatSymbolNoPrefixCode);          // Chat Symbol
            response.WriteString(member.Account.NameColourNoPrefixCode);          // Name Colour
            response.WriteString(member.Account.IconNoPrefixCode);                // Account Icon
            response.WriteInt32(member.Account.AscensionLevel);                   // Ascension Level
        }

        response.PrependBufferSize();

        // Announce To The Requesting Client That They Have Joined The Channel
        session.SendAsync(response.Data);

        return this;
    }

    public ChatChannel BroadcastJoin(ChatSession session)
    {
        ChatChannelMember newMember = Members.Values.Single(member => member.Account.ID == session.ClientInformation.Account.ID);

        List<ChatChannelMember> existingMembers = [.. Members.Values.Where(member => member.Account.ID != session.ClientInformation.Account.ID)];

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOINED_CHANNEL);

        broadcast.WriteInt32(ID);                                          // Channel ID
        broadcast.WriteString(newMember.Account.NameWithClanTag);          // Member Account Name
        broadcast.WriteInt32(newMember.Account.ID);                        // Member Account ID
        broadcast.WriteInt8(Convert.ToByte(newMember.ConnectionStatus));   // Connection Status
        broadcast.WriteInt8(Convert.ToByte(newMember.AdministratorLevel)); // Channel Administrator Level
        broadcast.WriteString(newMember.Account.ChatSymbolNoPrefixCode);   // Chat Symbol
        broadcast.WriteString(newMember.Account.NameColourNoPrefixCode);   // Name Colour
        broadcast.WriteString(newMember.Account.IconNoPrefixCode);         // Account Icon
        broadcast.WriteInt32(newMember.Account.AscensionLevel);            // Ascension Level

        broadcast.PrependBufferSize();

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        Parallel.ForEach(existingMembers, (existingMember) => existingMember.Session.SendAsync(broadcast.Data));

        return this;
    }

    public void Leave(ChatSession session)
    {
        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);

        broadcast.WriteInt32(session.ClientInformation.Account.ID); // Member Account ID
        broadcast.WriteInt32(ID);                                   // Channel ID

        broadcast.PrependBufferSize();

        // Announce To The Channel Members (Including To Self) That A Client Has Left The Channel
        Parallel.ForEach(Members.Values, (member) => member.Session.SendAsync(broadcast.Data));

        if (Members.TryRemove(session.ClientInformation.Account.Name, out _) is false)
            session.Logger.LogError(@"[BUG] Failed To Remove Account ""{AccountName}"" From Channel ""{ChannelName}""", session.ClientInformation.Account.Name, Name);

        // If There Are No Remaining Members And The Channel Is Not Permanent, Remove It Entirely
        if (Members.IsEmpty && (Flags & ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT) == 0)
            Context.ChatChannels.TryRemove(Name, out _);
    }

    public void Kick(int kickerID, int kickedID)
    {
        // TODO: Implement Route And Use Complex Object Parameter

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK);

        broadcast.WriteInt32(ID);       // Channel ID
        broadcast.WriteInt32(kickerID); // Kicker Account ID
        broadcast.WriteInt32(kickedID); // Kicked Account ID

        broadcast.PrependBufferSize();

        // Announce To The Channel Members (Including To Self) That A Client Was Kicked From The Channel
        Parallel.ForEach(Members.Values, (member) => member.Session.SendAsync(broadcast.Data));

        ChatSession kickedSession = Context.ChatSessions.Values.Single(session => session.ClientInformation.Account.ID == kickerID);

        if (Members.TryRemove(kickedSession.ClientInformation.Account.Name, out _) is false)
            kickedSession.Logger.LogError(@"[BUG] Failed To Remove Account ""{AccountName}"" From Channel ""{ChannelName}""", kickedSession.ClientInformation.Account.Name, Name);

        // If There Are No Remaining Members And The Channel Is Not Permanent, Remove It Entirely
        if (Members.IsEmpty && (Flags & ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT) == 0)
            Context.ChatChannels.TryRemove(Name, out _);
    }
}
