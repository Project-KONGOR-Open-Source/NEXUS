namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

public class ChatChannel
{
    public int ID => Name.GetDeterministicInt32Hash();

    public required string Name { get; set; }

    public string Topic { get; set; } = string.Empty;

    public required ChatProtocol.ChatChannelType Flags { get; set; }

    public ConcurrentDictionary<string, ChatChannelMember> Members { get; set; } = [];

    /// <summary>
    ///     Set of account IDs banned from this channel.
    /// </summary>
    public HashSet<int> BannedAccountIDs { get; set; } = [];

    /// <summary>
    ///     Account IDs silenced in this channel, each mapped to the instant at which its silence expires.
    ///     Silences are kept by account ID rather than on the channel membership, so that leaving and rejoining the channel does not lift them.
    /// </summary>
    public ConcurrentDictionary<int, DateTime> SilencedAccounts { get; set; } = [];

    /// <summary>
    ///     Set of lowercase account names authenticated to join this channel when authentication is enabled.
    /// </summary>
    public HashSet<string> AuthenticatedAccountNames { get; set; } = [];

    /// <summary>
    ///     Channel password for access control.
    ///     This value is <see langword="null"/> if no password is set.
    /// </summary>
    public string? Password { get; set; } = null;

    // Clan Channels Have No Member Cap, So That Every Member Of A Clan Can Always Join The Clan's Channel
    public bool IsFull => IsClanChannel is false && (Members.Count < ChatProtocol.MAX_USERS_PER_CHANNEL) is false;

    public bool IsPermanent => Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT);

    public bool IsGeneralChannel => Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_GENERAL_USE);

    public bool IsClanChannel => Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN);

    public bool IsAuthenticationRequired => Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_AUTH_REQUIRED);

    public bool HasPassword() => string.IsNullOrEmpty(Password) is false;

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="GetOrCreate"/> As The Primary Mechanism For Creating Chat Channels
    /// </summary>
    private ChatChannel() { }

    public static ChatChannel GetOrCreate(ClientChatSession session, string channelName)
    {
        // If The Requested Channel Is The General Channel Base Name, Route To The Overflow-Aware Method
        if (channelName.Equals(ChatProtocol.CHAT_CHANNEL_BASE_NAME, StringComparison.OrdinalIgnoreCase))
            return GetOrCreateGeneralChannel();

        Clan? clan = session.Account.Clan;

        bool isClanChannel = clan is not null && channelName == clan.GetChatChannelName();

        ChatProtocol.ChatChannelType chatChannelType = isClanChannel
            ? ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN
            : ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT;

        ChatChannel channel = Context.ChatChannels.GetOrAdd(channelName, new ChatChannel
        {
            Name = channelName,
            Flags = chatChannelType,
            Topic = clan is not null && isClanChannel ? clan.GetDefaultWelcomeMessage() : $"Welcome To The {channelName} Channel !"
        });

        return channel;
    }

    /// <summary>
    ///     Gets or creates the channel which a client has asked to join by name.
    ///     The general channel names are routed through the load balancing across the general channels, so a numbered general channel is never joined or created directly.
    ///     Returns <see langword="false"/>, after notifying the client, if the name belongs to a reserved channel which the client is not entitled to, such as the channel of another clan or another role, or a match or matchmaking group channel.
    /// </summary>
    public static bool TryGetOrCreateForJoinRequest(ClientChatSession session, string channelName, [NotNullWhen(true)] out ChatChannel? channel)
    {
        channel = null;

        if (ChatChannels.IsGeneralChannel(channelName))
        {
            channel = GetOrCreateGeneralChannel();

            return true;
        }

        if (ChatChannels.IsReservedChannel(channelName))
        {
            // The Reserved Channels Which A Client Is Entitled To Are Its Default Channels, Such As Its Clan Channel And The Channels Of Its Role
            // Match And Matchmaking Group Channels Are Never Among Them, Since Their Members Are Added By The Chat Server Itself
            string? defaultChannelName = session.Account.GetDefaultChatChannels()
                .SingleOrDefault(defaultChannel => defaultChannel.Equals(channelName, StringComparison.OrdinalIgnoreCase));

            if (defaultChannelName is null)
            {
                SendSystemMessage(session, channelName, ChatChannels.IsClanChannel(channelName) ? "Only Clan Members Can Join This Channel" : "This Channel Cannot Be Joined");

                return false;
            }

            // Reserved Channels Are Matched Case-Insensitively, So That A Differently-Cased Request Cannot Create A Look-Alike Channel
            channelName = defaultChannelName;
        }

        channel = GetOrCreate(session, channelName);

        return true;
    }

    /// <summary>
    ///     Gets or creates a general chat channel with overflow support.
    ///     Finds the first general channel which is not full.
    ///     If all existing general channels are full, a new numbered channel is created (e.g. "KONGOR 2", "KONGOR 3").
    ///     The first channel is named after the base name alone while it is the only general channel, and is renamed (e.g. "KONGOR 1") once the first overflow channel is created.
    ///     The first channel is permanent, but overflow channels are removed automatically when they become empty.
    /// </summary>
    public static ChatChannel GetOrCreateGeneralChannel()
    {
        string baseName = ChatProtocol.CHAT_CHANNEL_BASE_NAME;

        // Find The First General Channel With Capacity
        // Several General Channels Can Have Capacity At Once (e.g. Once Members Leave The First One While An Overflow Channel Is Still In Use), So The First One In Order Is Taken
        ChatChannel? availableChannel = Context.ChatChannels.Values
            .Where(channel => channel.IsGeneralChannel)
            .OrderBy(channel => channel.Name.Length)
            .ThenBy(channel => channel.Name)
            .FirstOrDefault(channel => channel.IsFull is false);

        if (availableChannel is not null)
            return availableChannel;

        // All General Channels Are Full (Or None Exist), So A New One Is Created, With The First One Named After The Base Name Alone
        bool isFirstChannel = Context.ChatChannels.Values.Any(channel => channel.IsGeneralChannel) is false;

        string channelName = baseName;

        if (isFirstChannel is false)
        {
            // The First Channel Takes A Number Alongside The Overflow Channels, So That The General Channels Are Named Consistently
            if (Context.ChatChannels.TryGetValue(baseName, out ChatChannel? unnumberedChannel))
                unnumberedChannel.Rename($"{baseName} 1");

            int channelNumber = 2;

            // Empty Overflow Channels Are Removed, Which Can Leave Gaps In The Numbering, So The Lowest Free Number Is Taken
            while (Context.ChatChannels.ContainsKey($"{baseName} {channelNumber}"))
                channelNumber++;

            channelName = $"{baseName} {channelNumber}";
        }

        // The First General Channel Is Permanent; Overflow Channels Are Not
        ChatProtocol.ChatChannelType flags = ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED
            | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_GENERAL_USE;

        if (isFirstChannel)
            flags |= ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT;

        ChatChannel channel = Context.ChatChannels.GetOrAdd(channelName, new ChatChannel
        {
            Name = channelName,
            Flags = flags,
            Topic = $"Welcome To Heroes Of Newerth !"
        });

        return channel;
    }

    /// <summary>
    ///     Compacts the general channels into as few general channels as their members need, since members leaving over time can spread the remaining members thinly across many general channels.
    ///     The members of the highest-numbered general channels which are no longer needed are moved into the lowest-numbered general channels which are not full, and each emptied overflow channel is removed.
    ///     Nothing is moved unless doing so removes at least one general channel, so that members are not moved without a reason.
    ///     Once a single general channel remains, it is named after the base name alone again.
    /// </summary>
    public static void CompactGeneralChannels()
    {
        MoveMembersOutOfUnneededGeneralChannels();

        List<ChatChannel> remainingChannels = [.. Context.ChatChannels.Values.Where(channel => channel.IsGeneralChannel)];

        if (remainingChannels is [ChatChannel lastChannel] && lastChannel.Name != ChatProtocol.CHAT_CHANNEL_BASE_NAME)
            lastChannel.Rename(ChatProtocol.CHAT_CHANNEL_BASE_NAME);
    }

    private static void MoveMembersOutOfUnneededGeneralChannels()
    {
        List<ChatChannel> generalChannels = [.. Context.ChatChannels.Values
            .Where(channel => channel.IsGeneralChannel)
            .OrderBy(channel => channel.Name.Length)
            .ThenBy(channel => channel.Name)];

        int memberCount = generalChannels.Sum(channel => channel.Members.Count);

        int requiredChannelCount = Math.Max(1, (int) Math.Ceiling(memberCount / (double) ChatProtocol.MAX_USERS_PER_CHANNEL));

        if (generalChannels.Count <= requiredChannelCount)
            return;

        List<ChatChannel> targetChannels = [.. generalChannels.Take(requiredChannelCount)];

        // The Highest-Numbered Channels Are Emptied First, So That The Lowest Numbers Stay In Use
        foreach (ChatChannel sourceChannel in generalChannels.Skip(requiredChannelCount).Reverse())
        {
            List<ChatChannelMember> members = [.. sourceChannel.Members.Values];

            foreach (ChatChannelMember member in members)
            {
                ClientChatSession session = member.Session;

                // Members Can Leave On Their Own While The Channels Are Being Compacted
                if (sourceChannel.Members.ContainsKey(session.Account.Name) is false)
                    continue;

                // A Member Already In One Of The Remaining Channels Only Needs To Leave This One, While Any Other Member Is Moved Into The First Remaining Channel Which Will Take Them
                ChatChannel? targetChannel = targetChannels.FirstOrDefault(channel => channel.Members.ContainsKey(session.Account.Name))
                    ?? targetChannels.FirstOrDefault(channel => channel.IsFull is false && channel.BannedAccountIDs.Contains(session.Account.ID) is false);

                // A Member Who Cannot Be Moved Stays Where They Are, Which Keeps Their Channel Open
                if (targetChannel is null)
                    continue;

                bool isMovedIntoTargetChannel = targetChannel.Members.ContainsKey(session.Account.Name) is false;

                if (isMovedIntoTargetChannel && targetChannel.AddMember(session, new ChatChannelMember(session, targetChannel)) is false)
                    continue;

                sourceChannel.Leave(session);

                if (isMovedIntoTargetChannel)
                    targetChannel.SendSystemMessage(session, $"Moved Here From {sourceChannel.Name} To Consolidate The General Channels");
            }
        }
    }

    /// <summary>
    ///     Gets or creates a match-specific chat channel.
    ///     Match channels use SERVER flag (for post-match chat) and HIDDEN flag.
    /// </summary>
    /// <param name="matchID">The match ID.</param>
    /// <returns>The match channel.</returns>
    public static ChatChannel GetOrCreateMatchChannel(int matchID)
    {
        string matchChannelName = $"{ChatChannels.MatchChannelPrefix} {matchID}";

        ChatChannel channel = Context.ChatChannels.GetOrAdd(matchChannelName, new ChatChannel
        {
            Name = matchChannelName,
            Flags = ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_SERVER
                  | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_HIDDEN
        });

        return channel;
    }

    /// <summary>
    ///     Gets or creates a matchmaking group chat channel.
    ///     Group channels use RESERVED (system-created), UNJOINABLE (cannot be manually joined), and HIDDEN flags.
    /// </summary>
    /// <param name="groupID">The matchmaking group ID.</param>
    /// <returns>The group channel.</returns>
    public static ChatChannel GetOrCreateGroupChannel(int groupID)
    {
        string groupChannelName = $"{ChatChannels.GroupChannelPrefix} {groupID}";

        ChatChannel channel = Context.ChatChannels.GetOrAdd(groupChannelName, new ChatChannel
        {
            Name = groupChannelName,
            Flags = ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED
                  | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_UNJOINABLE
                  | ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_HIDDEN
        });

        return channel;
    }

    /// <summary>
    ///     Attempts to get the channel, identified by either name or ID, which the session is a member of.
    ///     Returns <see langword="false"/> if no such channel exists or if the session is not a member of it.
    /// </summary>
    public static bool TryGet(ClientChatSession session, ChatChannelIdentifier channelIdentifier, [NotNullWhen(true)] out ChatChannel? channel)
    {
        channel = channelIdentifier switch
        {
            string channelName => Context.ChatChannels.Values
                .SingleOrDefault(candidate => candidate.Name == channelName && candidate.Members.ContainsKey(session.Account.Name)),

            int channelID      => Context.ChatChannels.Values
                .SingleOrDefault(candidate => candidate.ID == channelID && candidate.Members.ContainsKey(session.Account.Name))
        };

        return channel is not null;
    }

    public ChatChannel Join(ClientChatSession session, string? providedPassword = null)
    {
        // Staff Accounts Are Exempt From Channel Limit Restrictions, For Moderation And Administration Purposes
        if (session.Account.Type is not AccountType.Staff)
        {
            // Log A Bug-Type Error If The Client Has Exceeded The Maximum Number Of Channels
            if (session.CurrentChannels.Count > ChatProtocol.MAX_CHANNELS_PER_CLIENT)
                Log.Error(@"[BUG] Account ""{AccountName}"" Has Exceeded The Maximum Number Of Channels ({MaxChannels})", session.Account.Name, ChatProtocol.MAX_CHANNELS_PER_CLIENT);

            // Reject Join Request If Client Has Reached Maximum Number Of Channels
            if (session.CurrentChannels.Count == ChatProtocol.MAX_CHANNELS_PER_CLIENT)
            {
                ChatBuffer error = new ();

                error.WriteCommand(ChatProtocol.Command.CHAT_CMD_MAX_CHANNELS);

                session.Send(error);

                return this;
            }
        }

        // Reject Join Request If Client Is Already In The Channel
        if (Members.ContainsKey(session.Account.Name))
        {
            SendSystemMessage(session, "You Are Already A Member Of This Channel");

            return this;
        }

        // Reject Join Request If Client Is Banned From The Channel
        if (BannedAccountIDs.Contains(session.Account.ID))
        {
            ChatBuffer banned = new ();

            banned.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_IS_BANNED);
            banned.WriteString(Name);

            session.Send(banned);

            return this;
        }

        // Reject Join Request If The Channel Is A Clan Channel And The Client Is Not In The Clan
        if (Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN))
        {
            if (session.Account.Clan is null || Name != session.Account.Clan.GetChatChannelName())
            {
                SendSystemMessage(session, "Only Clan Members Can Join This Channel");

                return this;
            }
        }

        // Reject Manual Join Requests For Channels Flagged As Unjoinable, Such As Matchmaking Group Channels
        // Internal Joins Bypass This Path By Adding Members Directly, Mirroring The Forced-Join Behaviour Of The Authoritative "CChannel::CanJoin"
        if (Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_UNJOINABLE))
        {
            SendSystemMessage(session, "This Channel Cannot Be Joined");

            return this;
        }

        // The New Member Is Constructed Here So That Its Channel Administrator Status Can Be Evaluated For The Capacity Check Below
        ChatChannelMember newMember = new (session, this);

        // Reject Join Request If The Channel Is Full, Unless The Joining Client Is A Channel Administrator (Who May Still Join For Moderation)
        if (IsFull && newMember.IsAdministrator is false)
        {
            SendSystemMessage(session, "This Channel Is Full");

            return this;
        }

        // Check For Password Protection On The Channel
        // Staff Accounts And Channel Administrators Bypass Password Checks
        if (HasPassword())
        {
            if (newMember.IsAdministrator is false)
            {
                // If No Password Was Provided, Send Password Prompt To Client
                if (string.IsNullOrEmpty(providedPassword))
                {
                    ChatBuffer prompt = new ();

                    prompt.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL_PASSWORD);
                    prompt.WriteString(Name); // Channel Name Only (Does Not Reveal Password)

                    session.Send(prompt);

                    return this;
                }

                // If Wrong Password Was Provided, Reject Join Request
                if (Password is not null && Password.Equals(providedPassword, StringComparison.Ordinal) is false)
                {
                    SendSystemMessage(session, "Incorrect Channel Password");

                    return this;
                }
            }
        }

        AddMember(session, newMember);

        return this;
    }

    /// <summary>
    ///     Adds the member to the channel without any of the validation that <see cref="Join"/> performs, then announces the channel to the member and the member to the channel.
    ///     Returns <see langword="false"/>, after notifying the client, if the channel state would not fit in a single packet to the client, in which case the member is not added.
    /// </summary>
    private bool AddMember(ClientChatSession session, ChatChannelMember newMember)
    {
        if (Members.TryAdd(session.Account.Name, newMember) is false)
            Log.Error(@"[BUG] Failed To Add Account ""{AccountName}"" To Channel ""{ChannelName}""", session.Account.Name, Name);

        ChatBuffer response = BuildChannelState();

        // Reject The Join If The Serialised Channel State Would Exceed The Client's Receive Buffer, Rolling Back The Membership So The Client Is Not Left Half-Joined
        if (response.Size > ChatProtocol.MAX_PACKET_SIZE)
        {
            Members.TryRemove(session.Account.Name, out _);

            SendSystemMessage(session, "This Channel Is Too Large To Join At The Moment");

            return false;
        }

        // Announce To The Requesting Client That They Have Joined The Channel
        session.Send(response);

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        BroadcastJoin(session);

        // Track This Channel In The Client's Current Channels List
        session.CurrentChannels.Add(ID);

        // Announce The Synthetic TERMINAL Member To The Joining Client, So That It Can Render The Operational Log Messages Broadcast By <see cref="Terminal.Broadcast"/>
        if (Name == ChatChannels.StaffChannel)
            Terminal.AnnounceSyntheticMember(session);

        return true;
    }

    /// <summary>
    ///     Builds the channel state which a client receives on joining the channel, which describes the channel along with its administrators and members.
    /// </summary>
    private ChatBuffer BuildChannelState()
    {
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
            Log.Debug(@"Channel ""{ChannelName}"" Member Info: Name=""{AccountName}"", ID={AccountID}, ChatSymbol=""{ChatSymbol}"", NameColour=""{NameColour}"", Icon=""{Icon}"", AscensionLevel={AscensionLevel}",
                Name, member.Account.NameWithClanTag, member.Account.ID, member.Account.ChatSymbolNoPrefixCode, member.Account.NameColourNoPrefixCode, member.Account.IconNoPrefixCode, member.Account.AscensionLevel);

            response.WriteString(member.Account.NameWithClanTag);                 // Member Account Name
            response.WriteInt32(member.Account.ID);                               // Member Account ID
            response.WriteInt8(Convert.ToByte(member.ConnectionStatus));          // Connection Status
            response.WriteInt8(Convert.ToByte(member.AdministratorLevel));        // Channel Administrator Level
            response.WriteString(member.Account.ChatSymbolNoPrefixCode);          // Chat Symbol
            response.WriteString(member.Account.NameColourNoPrefixCode);          // Name Colour
            response.WriteString(member.Account.IconNoPrefixCode);                // Account Icon
            response.WriteInt32(member.Account.AscensionLevel);                   // Ascension Level
        }

        return response;
    }

    /// <summary>
    ///     Renames the channel, which also changes its ID, since the ID is derived from the name.
    ///     The game client cannot rename an open channel, so each member's channel is reset instead: the member leaves the channel under its old name and joins it under its new one.
    /// </summary>
    private void Rename(string newName)
    {
        string oldName = Name;
        int oldID = ID;

        if (Context.ChatChannels.TryRemove(oldName, out _) is false)
            Log.Error(@"[BUG] Failed To Remove Channel ""{ChannelName}"" From Global Channel List", oldName);

        Name = newName;

        if (Context.ChatChannels.TryAdd(newName, this) is false)
            Log.Error(@"[BUG] Failed To Add Channel ""{ChannelName}"" To Global Channel List", newName);

        ChatBuffer channelState = BuildChannelState();

        foreach (ChatChannelMember member in Members.Values)
        {
            ChatBuffer left = new ();

            left.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);
            left.WriteInt32(member.Account.ID); // Member Account ID
            left.WriteInt32(oldID);             // Channel ID

            member.Session.Send(left);

            member.Session.CurrentChannels.Remove(oldID);
            member.Session.CurrentChannels.Add(ID);

            member.Session.Send(channelState);

            // The Reset Clears The Member's Chat History, So It Is Explained In Character Rather Than As A Rename
            SendSystemMessage(member.Session, "A Forgetfulness Spell Was Cast, And The Old Scrolls Scattered To The Winds");
        }
    }

    private void BroadcastJoin(ClientChatSession session)
    {
        ChatChannelMember newMember = Members.Values.Single(member => member.Account.ID == session.Account.ID);

        List<ChatChannelMember> existingMembers = [.. Members.Values.Where(member => member.Account.ID != session.Account.ID)];

        Log.Debug(@"Broadcasting Join To Channel ""{ChannelName}"": Name=""{AccountName}"", ID={AccountID}, ChatSymbol=""{ChatSymbol}"", NameColour=""{NameColour}"", Icon=""{Icon}"", AscensionLevel={AscensionLevel}",
            Name, newMember.Account.NameWithClanTag, newMember.Account.ID, newMember.Account.ChatSymbolNoPrefixCode, newMember.Account.NameColourNoPrefixCode, newMember.Account.IconNoPrefixCode, newMember.Account.AscensionLevel);

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

    public void Leave(ClientChatSession session)
    {
        if (Members.TryRemove(session.Account.Name, out ChatChannelMember? member) is false)
            Log.Error(@"[BUG] Failed To Remove Account ""{AccountName}"" From Channel ""{ChannelName}""", session.Account.Name, Name);

        if (member is not null)
        {
            // Remove This Channel From The Client's Current Channels List
            session.CurrentChannels.Remove(ID);

            ChatBuffer broadcast = new ();

            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);
            broadcast.WriteInt32(member.Account.ID); // Member Account ID
            broadcast.WriteInt32(ID);                // Channel ID

            List<ChatChannelMember> channelMembers = [member, .. Members.Values];

            // Announce To The Channel Members (Including The Leaving Member, Even When It Was The Last One) That A Client Has Left The Channel
            // The Leaving Member Needs The Announcement When It Is Removed By The Chat Server Rather Than By Its Own Request, So That The Game Client Closes The Channel
            foreach (ChatChannelMember channelMember in channelMembers)
                channelMember.Session.Send(broadcast);

            // If There Are No Remaining Members And The Channel Is Not Permanent, Dispose Of It
            if (Members.IsEmpty is true && IsPermanent is false)
            {
                if (Context.ChatChannels.TryRemove(Name, out ChatChannel? channel) is false)
                    Log.Error(@"[BUG] Failed To Remove Channel ""{ChannelName}"" From Global Channel List", Name);

                if (channel is null)
                    Log.Error(@"[BUG] Chat Channel Instance For Channel ""{ChannelName}"" Is NULL", Name);
            }
        }

        else Log.Error(@"[BUG] Chat Channel Member Instance For Account ""{AccountName}"" In Channel ""{ChannelName}"" Is NULL", session.Account.Name, Name);
    }

    public void Kick(ClientChatSession requesterSession, int targetAccountID)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        // The Target May Have Left The Channel Before The Kick Arrived, In Which Case There Is Nobody To Kick
        ChatChannelMember? target = Members.Values.SingleOrDefault(member => member.Account.ID == targetAccountID);

        if (target is null)
            return;

        if (requester.HasHigherAdministratorLevelThan(target))
        {
            ChatBuffer broadcast = new ();

            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK);
            broadcast.WriteInt32(ID);                          // Channel ID
            broadcast.WriteInt32(requesterSession.Account.ID); // Kicker Account ID
            broadcast.WriteInt32(targetAccountID);             // Kicked Account ID

            // Announce To The Channel Members That A Client Will Be Kicked From The Channel
            foreach (ChatChannelMember member in Members.Values)
                member.Session.Send(broadcast);

            // Remove The Target Member From The Channel
            Leave(target.Session);
        }

        else SendSystemMessage(requesterSession, "You Do Not Have Permission To Kick That Member");
    }

    /// <summary>
    ///     Check if a member is currently silenced in this channel.
    /// </summary>
    /// <param name="session">The session to check.</param>
    /// <returns><see langword="true"/> if the member is silenced, <see langword="false"/> otherwise.</returns>
    public bool IsSilenced(ClientChatSession session)
    {
        // Staff Accounts Are Immune To Being Silenced
        if (session.Account.Type is AccountType.Staff)
            return false;

        if (SilencedAccounts.TryGetValue(session.Account.ID, out DateTime silencedUntil) is false)
            return false;

        // An Expired Silence Is Removed When It Is Lifted, Which Can Happen Fractionally After It Expires
        return DateTime.UtcNow <= silencedUntil;
    }

    /// <summary>
    ///     Silence a player in this channel.
    ///     The target does not need to be in the channel, in which case the silence applies once the target joins it, and the silence is only announced to the channel if the target is in it.
    /// </summary>
    /// <param name="requesterSession">The session requesting the silence (must have higher administrator level).</param>
    /// <param name="targetSession">The session of the player to silence.</param>
    /// <param name="durationMilliseconds">The duration of the silence in milliseconds.</param>
    public void Silence(ClientChatSession requesterSession, ClientChatSession targetSession, int durationMilliseconds)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        // A Target Who Is Not In The Channel Is Ranked As If It Were A Member
        bool isTargetInChannel = Members.TryGetValue(targetSession.Account.Name, out ChatChannelMember? targetMember);

        ChatChannelMember target = targetMember ?? new ChatChannelMember(targetSession, this);

        // Requester Must Have Higher Administrator Level Than Target (Strict Inequality)
        if (requester.HasHigherAdministratorLevelThan(target) is false)
        {
            SendSystemMessage(requesterSession, "You Do Not Have Permission To Silence That Member");

            return;
        }

        DateTime silencedUntil = DateTime.UtcNow.AddMilliseconds(durationMilliseconds);

        SilencedAccounts[target.Account.ID] = silencedUntil;

        _ = LiftSilenceOnExpiry(target.Account.ID, silencedUntil);

        if (isTargetInChannel is false)
        {
            SendSystemMessage(requesterSession, "That Player Will Be Silenced Upon Joining This Channel");

            return;
        }

        // Broadcast Silence Notification To All Channel Members
        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_PLACED);
        broadcast.WriteString(Name);                              // Channel Name
        broadcast.WriteString(requester.Account.NameWithClanTag); // Requester Name
        broadcast.WriteString(target.Account.NameWithClanTag);    // Target Name
        broadcast.WriteInt32(durationMilliseconds);               // Duration In Milliseconds

        BroadcastMessage(broadcast);
    }

    /// <summary>
    ///     Lifts the silence once it expires, and lets the formerly silenced player know, provided that they are in the channel and have not asked not to be disturbed.
    ///     A silence placed on the same player afterwards replaces this one, in which case this one is not lifted and the later silence is lifted when it expires instead.
    /// </summary>
    private async Task LiftSilenceOnExpiry(int accountID, DateTime silencedUntil)
    {
        TimeSpan remainingDuration = silencedUntil - DateTime.UtcNow;

        if (remainingDuration > TimeSpan.Zero)
            await Task.Delay(remainingDuration);

        if (SilencedAccounts.TryRemove(new KeyValuePair<int, DateTime>(accountID, silencedUntil)) is false)
            return;

        ChatChannelMember? member = Members.Values.SingleOrDefault(candidate => candidate.Account.ID == accountID);

        if (member is null || member.Session.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
            return;

        ChatBuffer lifted = new ();

        lifted.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_LIFTED);
        lifted.WriteString(Name); // Channel Name

        member.Session.Send(lifted);
    }

    /// <summary>
    ///     Broadcast a message to all members of the channel, optionally excluding the sender.
    /// </summary>
    /// <param name="message">The message buffer to broadcast.</param>
    /// <param name="excludeAccountID">Optional account ID to exclude from the broadcast (typically the sender).</param>
    public void BroadcastMessage(ChatBuffer message, int? excludeAccountID = null)
    {
        List<ChatChannelMember> recipients = excludeAccountID.HasValue
            ? [.. Members.Values.Where(member => member.Account.ID != excludeAccountID.Value)]
            : [.. Members.Values];

        foreach (ChatChannelMember recipient in recipients)
        {
            recipient.Session.Send(message);
        }
    }

    /// <summary>
    ///     Sends a one-way system message to a single client to surface a channel-operation failure for which the chat protocol defines no dedicated response.
    ///     The system message is delivered via <see cref="ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL"/>, which the client renders as a server message.
    ///     The system message appears to originate from the channel itself, rather than from a specific user, and is sent only to the specified client session.
    /// </summary>
    /// <param name="session">The client session to notify.</param>
    /// <param name="message">The human-readable message to display to the client.</param>
    public void SendSystemMessage(ClientChatSession session, string message)
        => SendSystemMessage(session, Name, message);

    /// <summary>
    ///     Sends a one-way system message to a single client on behalf of the named channel, which does not need to exist, such as when a request to join it is rejected before it is created.
    /// </summary>
    private static void SendSystemMessage(ClientChatSession session, string channelName, string message)
    {
        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
        response.WriteString(channelName); // Sender Name (The Channel The System Message Relates To)
        response.WriteString(message);     // System Message Text

        session.Send(response);
    }

    /// <summary>
    ///     Sets the channel password. Requires elevated privileges.
    ///     User must be a member of the channel to set the password.
    ///     Broadcasts password change notification to all channel members.
    /// </summary>
    /// <remarks>
    ///     The password can be changed by typing the following chat channel slash command: <c>/password {password}</c>
    ///     <br/>
    ///     To remove the password, use an empty string: <c>/password</c>
    /// </remarks>
    /// <param name="session">The session attempting to set the password.</param>
    /// <param name="password">The new password. Empty string clears the password.</param>
    public void SetPassword(ClientChatSession session, string password)
    {
        // User Must Be A Member Of The Channel To Set Password
        if (Members.TryGetValue(session.Account.Name, out ChatChannelMember? member) is false)
        {
            SendSystemMessage(session, "You Must Be A Member Of This Channel To Set Its Password");

            return;
        }

        // Check If Member Has Elevated Privileges, Which Are Required To Set Channel Password
        if (member.HasElevatedPrivileges() is false)
        {
            SendSystemMessage(session, "You Do Not Have Permission To Set This Channel's Password");

            return;
        }

        // Set The Password (Empty String Clears Password)
        Password = string.IsNullOrEmpty(password) ? null : password;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SET_PASSWORD);
        broadcast.WriteInt32(ID);                               // Channel ID
        broadcast.WriteString(session.Account.NameWithClanTag); // Password Setter's Name

        // Broadcast Password Change To All Channel Members
        BroadcastMessage(broadcast);
    }

    /// <summary>
    ///     Sets the channel topic and broadcasts the change to all members.
    /// </summary>
    public void SetTopic(ClientChatSession requesterSession, string topic)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? member) is false)
            return;

        // Requires At Least Officer Level
        if (member.AdministratorLevel < ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER)
            return;

        string truncatedTopic = topic.Length > ChatProtocol.CHAT_CHANNEL_TOPIC_MAX_LENGTH
            ? topic[..ChatProtocol.CHAT_CHANNEL_TOPIC_MAX_LENGTH]
            : topic;

        if (Topic == truncatedTopic)
            return;

        Topic = truncatedTopic;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_TOPIC);
        broadcast.WriteInt32(ID);
        broadcast.WriteString(Topic);

        BroadcastMessage(broadcast);
    }

    /// <summary>
    ///     Bans a player from the channel, removing them if present.
    /// </summary>
    public void Ban(ClientChatSession requesterSession, string targetName)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

        if (targetSession is null)
            return;

        // Check Admin Level (Target May Or May Not Be In The Channel)
        ChatChannelMember? target = Members.Values
            .SingleOrDefault(member => member.Account.ID == targetSession.Account.ID);

        if (target is not null && requester.HasHigherAdministratorLevelThan(target) is false)
            return;

        // Already Banned
        if (BannedAccountIDs.Add(targetSession.Account.ID) is false)
            return;

        // Remove From Channel If Present
        if (target is not null)
            Leave(targetSession);

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN);
        broadcast.WriteInt32(ID);
        broadcast.WriteInt32(requesterSession.Account.ID);
        broadcast.WriteString(targetSession.Account.Name);

        BroadcastMessage(broadcast);

        // Notify The Banned Player Individually (They Were Already Removed From The Channel)
        if (targetSession.Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_DND)
            targetSession.Send(broadcast);
    }

    /// <summary>
    ///     Lifts a ban on a player.
    /// </summary>
    public void LiftBan(ClientChatSession requesterSession, string targetName)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        // Requires At Least Officer Level
        if (requester.AdministratorLevel < ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER)
            return;

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

        if (targetSession is null)
            return;

        if (BannedAccountIDs.Remove(targetSession.Account.ID) is false)
            return;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_UNBAN);
        broadcast.WriteInt32(ID);
        broadcast.WriteInt32(requesterSession.Account.ID);
        broadcast.WriteString(targetSession.Account.Name);

        BroadcastMessage(broadcast);

        // Notify The Unbanned Player Individually
        if (targetSession.Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_DND)
            targetSession.Send(broadcast);
    }

    /// <summary>
    ///     Promotes a member's admin level in this channel.
    /// </summary>
    public void Promote(ClientChatSession requesterSession, int targetAccountID)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        ChatChannelMember? target = Members.Values
            .SingleOrDefault(member => member.Account.ID == targetAccountID);

        if (target is null)
            return;

        // Source Must Be Greater Than Target + 1 (i.e. At Least Two Levels Above)
        if (requester.HasHigherAdministratorLevelThan(target) is false)
            return;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_PROMOTE);
        broadcast.WriteInt32(ID);
        broadcast.WriteInt32(targetAccountID);
        broadcast.WriteInt32(requesterSession.Account.ID);

        BroadcastMessage(broadcast);
    }

    /// <summary>
    ///     Demotes a member's admin level in this channel.
    /// </summary>
    public void Demote(ClientChatSession requesterSession, int targetAccountID)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        ChatChannelMember? target = Members.Values
            .SingleOrDefault(member => member.Account.ID == targetAccountID);

        if (target is null)
            return;

        if (requester.HasHigherAdministratorLevelThan(target) is false)
            return;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_DEMOTE);
        broadcast.WriteInt32(ID);
        broadcast.WriteInt32(targetAccountID);
        broadcast.WriteInt32(requesterSession.Account.ID);

        BroadcastMessage(broadcast);
    }

    /// <summary>
    ///     Enables authentication on this channel. Only authenticated users can join.
    /// </summary>
    public void EnableAuthentication(ClientChatSession requesterSession)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        if (requester.AdministratorLevel < ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER)
            return;

        Flags |= ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_AUTH_REQUIRED;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SET_AUTH);
        broadcast.WriteInt32(ID);

        BroadcastMessage(broadcast);
    }

    /// <summary>
    ///     Disables authentication on this channel, allowing anyone to join.
    /// </summary>
    public void DisableAuthentication(ClientChatSession requesterSession)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        if (requester.AdministratorLevel < ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER)
            return;

        Flags &= ~ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_AUTH_REQUIRED;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_REMOVE_AUTH);
        broadcast.WriteInt32(ID);

        BroadcastMessage(broadcast);
    }

    /// <summary>
    ///     Adds a user to the channel's authenticated user list.
    /// </summary>
    public void AddAuthenticatedUser(ClientChatSession requesterSession, string targetName)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        if (requester.AdministratorLevel < ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER)
            return;

        // Strip Clan Tag (Everything Before And Including ']')
        int bracketIndex = targetName.IndexOf(']');
        string normalised = bracketIndex >= 0 ? targetName[(bracketIndex + 1)..] : targetName;
        string lowered = normalised.ToLowerInvariant();

        if (AuthenticatedAccountNames.Add(lowered))
        {
            ChatBuffer success = new ();

            success.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_ADD_AUTH_USER);
            success.WriteInt32(ID);
            success.WriteString(targetName);

            requesterSession.Send(success);
        }

        else
        {
            ChatBuffer failure = new ();

            failure.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_ADD_AUTH_FAIL);
            failure.WriteInt32(ID);
            failure.WriteString(targetName);

            requesterSession.Send(failure);
        }
    }

    /// <summary>
    ///     Removes a user from the channel's authenticated user list.
    /// </summary>
    public void RemoveAuthenticatedUser(ClientChatSession requesterSession, string targetName)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        if (requester.AdministratorLevel < ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER)
            return;

        string lowered = targetName.ToLowerInvariant();

        if (AuthenticatedAccountNames.Remove(lowered))
        {
            ChatBuffer success = new ();

            success.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_REMOVE_AUTH_USER);
            success.WriteInt32(ID);
            success.WriteString(targetName);

            requesterSession.Send(success);
        }

        else
        {
            ChatBuffer failure = new ();

            failure.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_REMOVE_AUTH_FAIL);
            failure.WriteInt32(ID);
            failure.WriteString(targetName);

            requesterSession.Send(failure);
        }
    }

    /// <summary>
    ///     Sends the channel's authenticated user list to the requesting client.
    /// </summary>
    public void SendAuthenticatedUserList(ClientChatSession requesterSession)
    {
        if (Members.TryGetValue(requesterSession.Account.Name, out ChatChannelMember? requester) is false)
            return;

        if (requester.AdministratorLevel < ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER)
            return;

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_LIST_AUTH);
        response.WriteInt32(ID);
        response.WriteInt32(AuthenticatedAccountNames.Count);

        foreach (string authenticatedName in AuthenticatedAccountNames)
        {
            // Try To Resolve The Display Name From Online Sessions, Otherwise Use The Stored Name
            ClientChatSession? authenticatedSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(authenticatedName, StringComparison.OrdinalIgnoreCase));

            response.WriteString(authenticatedSession?.Account.Name ?? authenticatedName);
        }

        requesterSession.Send(response);
    }
}

/// <summary>
///     Identifies a chat channel by either its name or its ID.
/// </summary>
public union ChatChannelIdentifier(string, int);
