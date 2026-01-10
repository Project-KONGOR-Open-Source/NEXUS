namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

public class ChatChannel
{
    private static int _nextChannelId = 0;
    public int ID { get; } = Interlocked.Increment(ref _nextChannelId);

    public required string Name { get; set; }

    private string? _topic;
    public string Topic
    {
        get => _topic ?? $"Welcome To The {Name} Channel !";
        set => _topic = value;
    }

    public required ChatProtocol.ChatChannelType Flags { get; set; }

    public ConcurrentDictionary<string, ChatChannelMember> Members { get; set; } = [];

    /// <summary>
    ///     Channel password for access control.
    ///     This value is NULL if no password is set.
    /// </summary>
    public string? Password { get; set; } = null;

    public bool IsFull => (Members.Count < ChatProtocol.MAX_USERS_PER_CHANNEL) is false;

    public bool IsPermanent => Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT);

    public bool HasPassword() => string.IsNullOrEmpty(Password) is false;

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

    public ChatChannel Join(ChatSession session, string? providedPassword = null)
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
            // Legacy Behavior: No Error Message Sent To Client (Silent Rejection)
            // TODO: Send Error Response To Client Indicating They Cannot Join This Clan Channel

            return this;
        }

        // Reject Join Request If The Channel Is A Clan Channel And The Client Is Not In The Clan
        if (Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN))
        {
            if (session.Account.Clan is null || Name != session.Account.Clan.GetChatChannelName())
            {
                // Legacy Behavior: No Error Message Sent To Client (Silent Rejection)
                // TODO: Send Error Response To Client Indicating They Cannot Join This Clan Channel

                return this;
            }
        }

        // TODO: Reject Join Request If The Channel Has The CHAT_CHANNEL_FLAG_UNJOINABLE Flag

        // TODO: Reject Join Request As Non-Administrator If Channel Is Full

        // TODO: Reject Join Request If Response Buffer Would Overlow With A Data Size Greater Than 16384 Bytes (16 Kilobytes)

        ChatChannelMember newMember = new (session, this);

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
                    // TODO: Send Error Response To Client Indicating Incorrect Password (Requires Direct User Messaging Implementation)

                    return this;
                }
            }
        }

        if (Members.TryAdd(session.Account.Name, newMember) is false)
            Log.Error(@"[BUG] Failed To Add Account ""{AccountName}"" To Channel ""{ChannelName}""", session.Account.Name, Name);

        Log.Information(@"[DEBUG] Client Joined Channel ""{ChannelName}"" (ID: {ChannelID}) - Sending ChangedChannel (0x0004)", Name, ID);

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
            response.WriteInt8(member.Account.GetChatClientFlags());         // Client's Flags (Chat Client Type)
            response.WriteString(member.Account.ChatSymbolNoPrefixCode);          // Chat Symbol
            response.WriteString(member.Account.NameColourNoPrefixCode);          // Name Colour
            response.WriteString(member.Account.IconNoPrefixCode);                // Account Icon
            response.WriteInt32(member.Account.AscensionLevel);                   // Ascension Level
        }

        // Announce To The Requesting Client That They Have Joined The Channel
        session.Send(response);

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        BroadcastJoin(session);

        // Track This Channel In The Client's Current Channels List
        session.CurrentChannels.Add(ID);

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
            // Remove This Channel From The Client's Current Channels List
            session.CurrentChannels.Remove(ID);

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

        if (requester.HasHigherAdministratorLevelThan(target))
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

            ChatSession targetSession = Context.ClientChatSessions.Values.Single(session => session.Account.ID == targetAccountID);

            // Remove The Target Member From The Channel
            Leave(targetSession);
        }

        // TODO: Notify The Requester That Their Attempt To Kick The Target Failed Due To Insufficient Permissions
    }

    /// <summary>
    ///     Check if a member is currently silenced in this channel.
    /// </summary>
    /// <param name="session">The session to check.</param>
    /// <returns>TRUE if the member is silenced, FALSE otherwise.</returns>
    public bool IsSilenced(ChatSession session)
    {
        ChatChannelMember? member = Members.Values
            .SingleOrDefault(channelMember => channelMember.Account.ID == session.Account.ID);

        return member?.IsSilenced() ?? false;
    }

    /// <summary>
    ///     Silence a member in this channel.
    /// </summary>
    /// <param name="requesterSession">The session requesting the silence (must have higher administrator level).</param>
    /// <param name="targetAccountID">The account ID of the member to silence.</param>
    /// <param name="durationMilliseconds">The duration of the silence in milliseconds.</param>
    public void Silence(ChatSession requesterSession, int targetAccountID, int durationMilliseconds)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);
        ChatChannelMember target = Members.Values.Single(member => member.Account.ID == targetAccountID);

        // Requester Must Have Higher Administrator Level Than Target (Strict Inequality)
        if (requester.HasHigherAdministratorLevelThan(target) is false)
        {
            // TODO: Notify Requester That They Don't Have Permission

            return;
        }

        // Set Silence Expiration On Target Member
        target.SilencedUntil = DateTime.UtcNow.AddMilliseconds(durationMilliseconds);

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
    public void SetPassword(ChatSession session, string password)
    {
        // User Must Be A Member Of The Channel To Set Password
        if (Members.TryGetValue(session.Account.Name, out ChatChannelMember? member) is false)
        {
            // TODO: Send Error Response To Client Indicating Not A Member (Requires Direct User Messaging Implementation)

            return;
        }

        // Check If Member Has Elevated Privileges, Which Are Required To Set Channel Password
        if (member.HasElevatedPrivileges() is false)
        {
            // TODO: Send Error Response To Client Indicating Insufficient Permissions (Requires Direct User Messaging Implementation)

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
}

