namespace TRANSMUTANSTEIN.ChatServer.Communication;

public class ChatChannel
{
    public int ID => Name.GetDeterministicInt32Hash();

    public required string Name { get; set; }

    public string Topic => $"Welcome To The {Name} Channel !";

    public required ChatProtocol.ChatChannelType Flags { get; set; }

    public ConcurrentDictionary<string, ChatChannelMember> Members { get; set; } = [];

    public bool IsFull => (Members.Count < ChatProtocol.MAX_USERS_PER_CHANNEL) is false;

    public bool IsPermanent => Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT);

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="GetOrCreate"/> As The Primary Mechanism For Creating Chat Channels
    /// </summary>
    private ChatChannel() { }

    public static ChatChannel GetOrCreate(ChatSession session, string channelName)
    {
        bool isClanChannel = session.Account.Clan is not null && channelName == session.Account.Clan.GetChatChannelName();

        ChatProtocol.ChatChannelType chatChannelType = isClanChannel
            ? ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN
            : ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT;

        ChatChannel channel = Context.ChatChannels.GetOrAdd(channelName, new ChatChannel { Name = channelName, Flags = chatChannelType });

        return channel;
    }

    public static ChatChannel Get(ChatSession session, OneOf<string, int> channelIdentifier)
    {
        ChatChannel channel = channelIdentifier.Match
        (
            channelName => Context.ChatChannels.Values
                .Single(channel => channel.Name == channelName && channel.Members.ContainsKey(session.Account.Name)),

            channelID => Context.ChatChannels.Values
                .Single(channel => channel.ID == channelID && channel.Members.ContainsKey(session.Account.Name))
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

        if (Members.TryAdd(session.Account.Name, newMember) is false)
            Log.Error(@"[BUG] Failed To Add Account ""{AccountName}"" To Channel ""{ChannelName}""", session.Account.Name, Name);

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

        // Announce To The Requesting Client That They Have Joined The Channel
        session.Send(response);

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        BroadcastJoin(session);

        return this;
    }

    private void BroadcastJoin(ChatSession session)
    {
        ChatChannelMember newMember = Members.Values.Single(member => member.Account.ID == session.Account.ID);

        List<ChatChannelMember> existingMembers = [.. Members.Values.Where(member => member.Account.ID != session.Account.ID)];

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

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        foreach (ChatChannelMember existingMember in existingMembers)
            existingMember.Session.Send(broadcast);
    }

    public void Leave(ChatSession session)
    {
        if (Members.TryRemove(session.Account.Name, out ChatChannelMember? member) is false)
            Log.Error(@"[BUG] Failed To Remove Account ""{AccountName}"" From Channel ""{ChannelName}""", session.Account.Name, Name);

        if (member is not null)
        {
            // If There Are No Remaining Members And The Channel Is Not Permanent, Dispose Of It
            if (Members.IsEmpty is true && IsPermanent is false)
            {
                if (Context.ChatChannels.TryRemove(Name, out ChatChannel? channel) is false)
                    Log.Error(@"[BUG] Failed To Remove Channel ""{ChannelName}"" From Global Channel List", Name);

                if (channel is null)
                    Log.Error(@"[BUG] Chat Channel Instance For Channel ""{ChannelName}"" Is NULL", Name);
            }

            else if (Members.IsEmpty is false)
            {
                ChatBuffer broadcast = new ();

                broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);
                broadcast.WriteInt32(member.Account.ID); // Member Account ID
                broadcast.WriteInt32(ID);                // Channel ID

                List<ChatChannelMember> channelMembers = [member, .. Members.Values];

                // Announce To The Channel Members (Including The Leaving Member) That A Client Has Left The Channel
                foreach (ChatChannelMember channelMember in channelMembers)
                    channelMember.Session.Send(broadcast);
            }
        }

        else Log.Error(@"[BUG] Chat Channel Member Instance For Account ""{AccountName}"" In Channel ""{ChannelName}"" Is NULL", session.Account.Name, Name);
    }

    public void Kick(ChatSession requesterSession, int targetAccountID)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);
        ChatChannelMember target = Members.Values.Single(member => member.Account.ID == targetAccountID);

        if (requester.AdministratorLevel > target.AdministratorLevel)
        {
            ChatBuffer broadcast = new ();

            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK);
            broadcast.WriteInt32(ID);                          // Channel ID
            broadcast.WriteInt32(requesterSession.Account.ID); // Kicker Account ID
            broadcast.WriteInt32(targetAccountID);             // Kicked Account ID

            // Announce To The Channel Members That A Client Will Be Kicked From The Channel
            // If The Requester's Administrator Level Is Less Than Or Equal To The Target's, This Operation Fails Silently
            foreach (ChatChannelMember member in Members.Values)
                member.Session.Send(broadcast);

            ChatSession targetSession = Context.ChatSessions.Values.Single(session => session.Account.ID == targetAccountID);

            // Remove The Target Member From The Channel
            Leave(targetSession);
        }

        // TODO: Notify The Requester That Their Attempt To Kick The Target Failed Due To Insufficient Permissions
    }
}
