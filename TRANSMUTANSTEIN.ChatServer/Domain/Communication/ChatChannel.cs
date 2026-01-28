using MERRICK.DatabaseContext.Extensions;

using TRANSMUTANSTEIN.ChatServer.Domain.Clans;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

public class ChatChannel
{
    private static int _nextChannelId;

    private static readonly HashSet<string> SystemChannels =
    [
        "General",
        "Lobby",
        "Help",
        "Support",
        "Development",
        "Staff",
        "KONGOR"
    ];

    private string? _topic;

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="GetOrCreate" /> As The Primary Mechanism For Creating Chat Channels
    /// </summary>
    private ChatChannel() { }

    public int ID { get; } = Interlocked.Increment(ref _nextChannelId);

    public required string Name { get; set; }

    public string Topic
    {
        get => _topic ?? $"Welcome To The {Name} Channel !";
        set => _topic = value;
    }

    public required ChatProtocol.ChatChannelType Flags { get; set; }

    public ConcurrentDictionary<string, ChatChannelMember> Members { get; set; } = [];

    public ConcurrentDictionary<int, ChatProtocol.AdminLevel> CustomAdmins { get; } = [];

    private readonly ConcurrentDictionary<int, byte> _bannedAccountIDs = [];

    // Store Authorized User IDs for Auth-Required Channels
    private readonly ConcurrentDictionary<int, byte> _authorizedAccountIDs = [];

    /// <summary>
    ///     Channel password for access control.
    ///     This value is NULL if no password is set.
    /// </summary>
    public string? Password { get; set; }

    public bool IsFull => (Members.Count < ChatProtocol.MAX_USERS_PER_CHANNEL) is false;

    public bool IsPermanent => Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT);

    public bool HasPassword()
    {
        return string.IsNullOrEmpty(Password) is false;
    }

    public static ChatChannel GetOrCreate(IChatContext chatContext, ChatSession? session, string channelName)
    {
        // Enforce Clan Channel Flag if the name matches the Clan Channel pattern ("Clan {Name}")
        // This prevents non-clan members from creating "spoof" permanent channels with Clan names
        bool isClanChannel =
            (session?.Account?.Clan is not null && channelName == session.Account.Clan.GetChatChannelName()) ||
            channelName.StartsWith("Clan ", StringComparison.OrdinalIgnoreCase);

        ChatProtocol.ChatChannelType chatChannelType = channelName switch
        {
            _ when isClanChannel => ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED |
                                    ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN,
            _ when SystemChannels.Contains(channelName) => ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_RESERVED |
                                                           ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT,
            _ => ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_GENERAL_USE
        };

        ChatChannel channel = chatContext.ChatChannels.GetOrAdd(channelName,
            new ChatChannel { Name = channelName, Flags = chatChannelType });

        return channel;
    }

    public static ChatChannel? Get(IChatContext chatContext, ChatSession session, OneOf<string, int> channelIdentifier)
    {
        ChatChannel? channel = channelIdentifier.Match
        (
            channelName => chatContext.ChatChannels.Values
                .SingleOrDefault(channel => channel.Name == channelName && channel.Members.ContainsKey(session.Account.Name)),
            channelID => chatContext.ChatChannels.Values
                .SingleOrDefault(channel => channel.ID == channelID && channel.Members.ContainsKey(session.Account.Name))
        );

        return channel;
    }

    public ChatChannel Join(ChatSession session, string? providedPassword = null)
    {
        if (session?.Account is null)
        {
            return this;
        }

        // Staff Accounts Are Exempt From Channel Limit Restrictions, For Moderation And Administration Purposes
        if (session.Account.Type is not AccountType.Staff)
        {
            // Log A Bug-Type Error If The Client Has Exceeded The Maximum Number Of Channels
            if (session.CurrentChannels.Count > ChatProtocol.MAX_CHANNELS_PER_CLIENT)
            {
                Log.Error(
                    @"[BUG] Account ""{AccountName}"" Has Exceeded The Maximum Number Of Channels ({MaxChannels})",
                    session.Account.Name, ChatProtocol.MAX_CHANNELS_PER_CLIENT);
            }

            // Reject Join Request If Client Has Reached Maximum Number Of Channels
            if (session.CurrentChannels.Count == ChatProtocol.MAX_CHANNELS_PER_CLIENT)
            {
                ChatBuffer error = new();

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
                ChatBuffer accessDenied = new();
                accessDenied.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
                accessDenied.WriteString("Channel Service");
                accessDenied.WriteString("You do not have the correct permission to access this channel.");

                session.Send(accessDenied);

                return this;
            }
        }

        if (_bannedAccountIDs.ContainsKey(session.Account.ID))
        {
            ChatBuffer banned = new();
            // Note: This matches the "Victim Notification" OpCode (0x0034) but payload here is Channel ID.
            // The user said CHAT_CMD_CHANNEL_IS_BANNED expects [String ChannelName].
            // If I change it here, I fix it for Join rejection too.
            banned.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_IS_BANNED);
            banned.WriteString(Name); // Channel Name

            session.Send(banned);

            return this;
        }

        // Check Auth Required
        if (Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_AUTH_REQUIRED))
        {
            // If not in auth list and not staff
            if (!_authorizedAccountIDs.ContainsKey(session.Account.ID) && session.Account.Type != AccountType.Staff)
            {
                // TODO: Send Auth Failed Response?
                // There isn't a specific "Join Auth Failed" command, usually generic or silence.
                // Assuming silent fail or similar to banned.
                return this;
            }
        }

        // TODO: Reject Join Request If The Channel Has The CHAT_CHANNEL_FLAG_UNJOINABLE Flag

        // TODO: Reject Join Request As Non-Administrator If Channel Is Full

        // TODO: Reject Join Request If Response Buffer Would Overlow With A Data Size Greater Than 16384 Bytes (16 Kilobytes)

        ChatChannelMember newMember = new(session, this);

        // Check For Password Protection On The Channel
        // Staff Accounts And Channel Administrators Bypass Password Checks
        if (HasPassword())
        {
            if (newMember.IsAdministrator is false)
            {
                // If No Password Was Provided, Send Password Prompt To Client
                if (string.IsNullOrEmpty(providedPassword))
                {
                    ChatBuffer prompt = new();

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
        {
            Log.Error(@"[BUG] Failed To Add Account ""{AccountName}"" To Channel ""{ChannelName}""",
                session.Account.Name, Name);
        }

        // Grant Admin Rights To The First Joiner Of A Custom Channel
        if (Members.Count == 1 &&
            Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN) is false &&
            Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_PERMANENT) is false)
        {
            CustomAdmins.TryAdd(session.Account.ID, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER);
        }

        Log.Information(
            @"[DEBUG] Client Joined Channel ""{ChannelName}"" (ID: {ChannelID}) - Sending ChangedChannel (0x0004)",
            Name, ID);

        ChatBuffer response = new();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);
        response.WriteString(Name); // Channel Name
        response.WriteInt32(ID); // Channel ID
        response.WriteInt8(Convert.ToByte(Flags)); // Channel Flags
        response.WriteString(Topic); // Channel Topic

        List<ChatChannelMember> administrators = [.. Members.Values.Where(member => member.IsAdministrator)];

        response.WriteInt32(administrators.Count); // Count Of Channel Administrators

        foreach (ChatChannelMember administrator in administrators)
        {
            response.WriteInt32(administrator.Account.ID); // Administrator Account ID
            response.WriteInt8(Convert.ToByte(administrator.AdministratorLevel)); // Channel Administrator Level
        }

        response.WriteInt32(Members.Count); // Count Of Channel Members

        foreach (ChatChannelMember member in Members.Values)
        {
            response.WriteString(member.Account.GetNameWithClanTag()); // Member Account Name
            response.WriteInt32(member.Account.ID); // Member Account ID
            response.WriteInt8((byte) ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);
            response.WriteInt8(member.Account.GetChatClientFlags()); // Client's Flags (Chat Client Type)
            response.WriteString(member.Account.GetChatSymbolNoPrefixCode()); // Chat Symbol
            response.WriteString(member.Account.GetNameColourNoPrefixCode()); // Name Colour
            response.WriteString(member.Account.GetIconNoPrefixCode()); // Account Icon
            response.WriteInt32(member.Account.AscensionLevel); // Ascension Level
        }

        // Announce To The Requesting Client That They Have Joined The Channel
        session.Send(response);

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        BroadcastJoin(newMember);

        // Track This Channel In The Client's Current Channels List
        session.CurrentChannels.Add(ID);

        return this;
    }

    private void BroadcastJoin(ChatChannelMember newMember)
    {
        if (newMember?.Account is null)
        {
            Log.Error("[BUG] BroadcastJoin Called With Null Member Or Account");
            return;
        }

        List<ChatChannelMember> existingMembers =
            [.. Members.Values.Where(member => member.Account.ID != newMember.Account.ID)];

        ChatBuffer broadcast = new();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOINED_CHANNEL);
        broadcast.WriteInt32(ID); // Channel ID
        broadcast.WriteString(newMember.Account.GetNameWithClanTag()); // Member Account Name
        broadcast.WriteInt32(newMember.Account.ID); // Member Account ID
        broadcast.WriteInt8((byte) ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);
        broadcast.WriteInt8(newMember.Account.GetChatClientFlags()); // Client's Flags (Chat Client Type)
        broadcast.WriteString(newMember.Account.GetChatSymbolNoPrefixCode()); // Chat Symbol
        broadcast.WriteString(newMember.Account.GetNameColourNoPrefixCode()); // Name Colour
        broadcast.WriteString(newMember.Account.GetIconNoPrefixCode()); // Account Icon
        broadcast.WriteInt32(newMember.Account.AscensionLevel); // Ascension Level

        // Announce To The Existing Channel Members That A New Client Has Joined The Channel
        foreach (ChatChannelMember existingMember in existingMembers)
        {
            existingMember.Session.Send(broadcast);
        }
    }

    public void Leave(IChatContext chatContext, ChatSession session)
    {
        if (Members.TryRemove(session.Account.Name, out ChatChannelMember? member) is false)
        {
            Log.Error(@"[BUG] Failed To Remove Account ""{AccountName}"" From Channel ""{ChannelName}""",
                session.Account.Name, Name);
        }

        if (member is not null)
        {
            // Remove This Channel From The Client's Current Channels List
            session.CurrentChannels.Remove(ID);

            ChatBuffer broadcast = new();

            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);
            broadcast.WriteInt32(member.Account.ID); // Member Account ID
            broadcast.WriteInt32(ID); // Channel ID

            // Notify The Leaver
            member.Session.Send(broadcast);

            switch (Members.IsEmpty)
            {
                // If There Are No Remaining Members And The Channel Is Not Permanent, Dispose Of It
                case true when IsPermanent is false:
                    {
                        if (chatContext.ChatChannels.TryRemove(Name, out ChatChannel? channel) is false)
                        {
                            Log.Error(@"[BUG] Failed To Remove Channel ""{ChannelName}"" From Global Channel List", Name);
                        }

                        if (channel is null)
                        {
                            Log.Error(@"[BUG] Chat Channel Instance For Channel ""{ChannelName}"" Is NULL", Name);
                        }
                        else
                        {
                            // Clear state
                            channel.CustomAdmins.Clear();
                            channel._bannedAccountIDs.Clear();
                            channel._authorizedAccountIDs.Clear();
                        }

                        break;
                    }
                case true when IsPermanent:
                    {
                        CustomAdmins.Clear();
                        break;
                    }
                case false:
                    {
                        // Announce To The Remaining Channel Members That A Client Has Left The Channel
                        foreach (ChatChannelMember channelMember in Members.Values)
                        {
                            channelMember.Session.Send(broadcast);
                        }

                        break;
                    }
            }
        }

        else
        {
            Log.Error(
                @"[BUG] Chat Channel Member Instance For Account ""{AccountName}"" In Channel ""{ChannelName}"" Is NULL",
                session.Account.Name, Name);
        }
    }

    public void Kick(IChatContext chatContext, ChatSession requesterSession, int targetAccountID)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);
        ChatChannelMember target = Members.Values.Single(member => member.Account.ID == targetAccountID);

        if (requester.HasHigherAdministratorLevelThan(target))
        {
            ChatBuffer broadcast = new();

            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK);
            broadcast.WriteInt32(ID); // Channel ID
            broadcast.WriteInt32(requesterSession.Account.ID); // Kicker Account ID
            broadcast.WriteInt32(targetAccountID); // Kicked Account ID

            // Announce To The Channel Members That A Client Will Be Kicked From The Channel
            foreach (ChatChannelMember member in Members.Values)
            {
                member.Session.Send(broadcast);
            }

            ChatSession targetSession =
                chatContext.ClientChatSessions.Values.Single(session => session.Account.ID == targetAccountID);

            // Remove The Target Member From The Channel
            Leave(chatContext, targetSession);
        }
    }

    public bool IsSilenced(ChatSession session)
    {
        ChatChannelMember? member = Members.Values
            .SingleOrDefault(channelMember => channelMember.Account.ID == session.Account.ID);

        return member?.IsSilenced() ?? false;
    }

    public void Silence(ChatSession requesterSession, int targetAccountID, int durationMilliseconds)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);
        ChatChannelMember target = Members.Values.Single(member => member.Account.ID == targetAccountID);

        // Requester Must Have Higher Administrator Level Than Target (Strict Inequality)
        bool outranks = requester.HasHigherAdministratorLevelThan(target);
        Log.Information("[DEBUG] Silence Check: Requester={RName} ({RLevel}) Target={TName} ({TLevel}) Outranks={Outranks}",
            requester.Account.Name, requester.AdministratorLevel, target.Account.Name, target.AdministratorLevel, outranks);

        if (outranks is false)
        {
            Log.Information("[DEBUG] Silence Failed: Does not outrank");
            return;
        }

        if (durationMilliseconds == 0)
        {
            // Unsilence
            target.SilencedUntil = null;

            ChatBuffer broadcast = new();
            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_LIFTED);
            broadcast.WriteString(Name); // Channel Name
            broadcast.WriteString(requester.Account.GetNameWithClanTag()); // Requester Name
            broadcast.WriteString(target.Account.GetNameWithClanTag()); // Target Name

            BroadcastMessage(broadcast);
        }
        else
        {
            // Silence
            target.SilencedUntil = DateTime.UtcNow.AddMilliseconds(durationMilliseconds);

            ChatBuffer broadcast = new();
            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_PLACED);
            broadcast.WriteString(Name); // Channel Name
            broadcast.WriteString(requester.Account.GetNameWithClanTag()); // Requester Name
            broadcast.WriteString(target.Account.GetNameWithClanTag()); // Target Name
            broadcast.WriteInt32(durationMilliseconds); // Duration In Milliseconds

            BroadcastMessage(broadcast);
        }
    }

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

    public void SetPassword(ChatSession session, string password)
    {
        if (Members.TryGetValue(session.Account.Name, out ChatChannelMember? member) is false)
        {
            return;
        }

        if (member.HasElevatedPrivileges() is false)
        {
            return;
        }

        Password = string.IsNullOrEmpty(password) ? null : password;

        ChatBuffer broadcast = new();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SET_PASSWORD);
        broadcast.WriteInt32(ID); // Channel ID
        broadcast.WriteString(session.Account.GetNameWithClanTag()); // Password Setter's Name

        BroadcastMessage(broadcast);
    }

    public void Ban(IChatContext chatContext, ChatSession requesterSession, int targetAccountID, string targetName)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        // Check Permissions (Must have elevated privileges to ban)
        bool hasPrivileges = requester.HasElevatedPrivileges();
        
        // FORCE LOGGING ERROR LEVEL
        Log.Error("[CRITICAL DEBUG] Ban Check: Requester={Name} Level={Level} HasElevated={HasPrivileges}", 
            requester.Account.Name, requester.AdministratorLevel, hasPrivileges);

        // throw new Exception("BAN DEBUG HIT - IF YOU SEE THIS, CODE IS UPDATED");

        if (hasPrivileges is false)
        {
            Log.Error("[CRITICAL DEBUG] Ban Failed: Insufficient Privileges");
            return;
        }

        _bannedAccountIDs.TryAdd(targetAccountID, 0);

        // If target is currently in the channel, kick them
        if (Members.Values.Any(m => m.Account.ID == targetAccountID))
        {
            Kick(chatContext, requesterSession, targetAccountID);
        }

        // Broadcast CHAT_CMD_CHANNEL_BAN (0x0032) to Channel
        // Payload: [Int ChannelID] [Int BannerID] [String TargetName]
        ChatBuffer broadcast = new();
        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN);
        broadcast.WriteInt32(ID); // Channel ID
        broadcast.WriteInt32(requesterSession.Account.ID); // Banner ID
        broadcast.WriteString(targetName); // Target Name
        BroadcastMessage(broadcast);

        // Notify Victim via CHAT_CMD_CHANNEL_IS_BANNED (0x0034)
        // Payload: [String ChannelName]
        ChatSession? targetSession = chatContext.ClientChatSessions.Values
            .FirstOrDefault(s => s.Account.ID == targetAccountID);

        if (targetSession != null)
        {
            ChatBuffer victimMsg = new();
            victimMsg.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_IS_BANNED);
            victimMsg.WriteString(Name); // Channel Name
            targetSession.Send(victimMsg);
        }
    }

    public void Unban(ChatSession requesterSession, int targetAccountID, string targetName)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        if (requester.HasElevatedPrivileges() is false)
        {
            return;
        }

        if (_bannedAccountIDs.TryRemove(targetAccountID, out _))
        {
            // Broadcast CHAT_CMD_CHANNEL_UNBAN (0x0033)
            // Payload: [Int ChannelID] [Int UnbannerID] [String TargetName]
            ChatBuffer broadcast = new();
            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_UNBAN);
            broadcast.WriteInt32(ID); // Channel ID
            broadcast.WriteInt32(requesterSession.Account.ID); // Unbanner ID
            broadcast.WriteString(targetName); // Target Name

            BroadcastMessage(broadcast);
        }
    }

    public void AddAuthUser(ChatSession requesterSession, int targetAccountID, string targetName)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        // Need Admin/Leader
        if (requester.HasElevatedPrivileges() is false)
        {
            // Send Failure?
            return;
        }

        _authorizedAccountIDs.TryAdd(targetAccountID, 0);

        // Protocol: CHAT_CMD_CHANNEL_ADD_AUTH_USER (0x0040)
        // The protocol description says "User Wants To Add A User".
        // Is there a broadcast for success?
        // Protocol lists: CHAT_CMD_CHANNEL_ADD_AUTH_FAIL (0x0044).
        // It doesn't list a specific success broadcast other than maybe repeating the command or LIST_AUTH.
        // Assuming no broadcast needed or simple acknowledgement.
        // Let's assume just updating the state is the goal for now.

        // If we want to support listing auth users, we need to store them.
        // I added _authorizedAccountIDs.
    }

    public void RemoveAuthUser(ChatSession requesterSession, int targetAccountID, string targetName)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        if (requester.HasElevatedPrivileges() is false)
        {
            return;
        }

        _authorizedAccountIDs.TryRemove(targetAccountID, out _);
    }

    // Helper to get auth list?
    public List<int> GetAuthorizedUsers()
    {
        return _authorizedAccountIDs.Keys.ToList();
    }

    public void Promote(ChatSession requesterSession, int targetAccountID)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        if (requester.HasElevatedPrivileges() is false)
        {
            return;
        }

        CustomAdmins[targetAccountID] = ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER;

        ChatBuffer broadcast = new();
        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_PROMOTE);
        broadcast.WriteInt32(ID); // Channel ID
        broadcast.WriteInt32(targetAccountID); // Target ID
        broadcast.WriteInt8((byte) ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER); // New Rank

        BroadcastMessage(broadcast);
    }

    public void Demote(ChatSession requesterSession, int targetAccountID)
    {
        ChatChannelMember requester = Members.Values.Single(member => member.Account.ID == requesterSession.Account.ID);

        if (requester.HasElevatedPrivileges() is false)
        {
            return;
        }

        if (CustomAdmins.TryRemove(targetAccountID, out _))
        {
            ChatBuffer broadcast = new();
            broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_DEMOTE);
            broadcast.WriteInt32(ID); // Channel ID
            broadcast.WriteInt32(targetAccountID); // Target ID

            BroadcastMessage(broadcast);
        }
    }

    public void Roll(ChatSession requesterSession, string parameters)
    {
        int max = 100;
        if (int.TryParse(parameters, out int parsedMax) && parsedMax > 0)
        {
            max = parsedMax;
        }

        int roll = Random.Shared.Next(1, max + 1);

        string resultText = $"rolled {roll} (1-{max})";

        ChatBuffer broadcast = new();
        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHAT_ROLL);
        broadcast.WriteInt32(requesterSession.Account.ID);
        broadcast.WriteInt32(ID);
        broadcast.WriteString(resultText);

        BroadcastMessage(broadcast);
    }
}