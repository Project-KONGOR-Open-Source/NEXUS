namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class ClientChatSession(TCPServer server, IServiceProvider serviceProvider) : ChatSession(server, serviceProvider)
{
    /// <summary>
    ///     Gets set after a successful client handshake following the <see cref="Accept"/> method.
    ///     Contains metadata about the client connected to this chat session.
    ///     This property is NULL before authentication, but is guaranteed non-NULL after <see cref="Accept"/> is called.
    /// </summary>
    public ClientChatSessionMetadata Metadata { get; set; } = null!;

    /// <summary>
    ///     Gets set after a successful client handshake following the <see cref="Accept"/> method.
    ///     Contains the account information of the client connected to this chat session.
    ///     This property is NULL before authentication, but is guaranteed non-NULL after <see cref="Accept"/> is called.
    /// </summary>
    public Account Account { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the data associated with the start of a match.
    /// </summary>
    public MatchInformation? MatchInformation { get; set; }

    /// <summary>
    ///     Tracks the channel IDs that this client is currently a member of.
    ///     The maximum number of channels cannot exceed the value of <see cref="ChatProtocol.MAX_CHANNELS_PER_CLIENT"/>.
    /// </summary>
    public HashSet<int> CurrentChannels { get; set; } = [];

    public ClientChatSession Accept(Account account)
    {
        // Link The Account To The Chat Session
        Account = account;

        // Add The Chat Session To The Chat Sessions Collection
        Context.ClientChatSessions.AddOrUpdate(account.Name, this, (key, existing) => this);

        ChatBuffer accept = new ();

        accept.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        Send(accept);

        return this;
    }

    /// <summary>
    ///     Completes the connection process by sending initial status updates and broadcasting the client's online status to peers.
    /// </summary>
    public ClientChatSession SetOnline()
    {
        // Get The Connection Status Of All Friends And Clan Members That Are Currently Online (Excluding Invisible Clients)
        // This Sends CHAT_CMD_INITIAL_STATUS To Self
        ReceiveFriendAndClanMemberConnectionStatus();

        // Notify Friends And Clan Members That This Client Is Now Connected
        // This Sends CHAT_CMD_UPDATE_STATUS To Peers
        UpdateStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

        return this;
    }

    public async Task<ClientChatSession> PrepareToJoinMatch(IDatabase distributedCacheStore, string serverAddress)
    {
        List<string> serverAddressParts = [.. serverAddress.Split(':')];

        if (serverAddressParts.Count is not 2)
        {
            Log.Error(@"[BUG] Invalid Match Server Address ""{MatchServerAddress}""", serverAddress);

            return this;
        }

        string serverIPAddress = serverAddressParts.First();
        int serverPort = Convert.ToInt32(serverAddressParts.Last());

        MatchServer? server = await distributedCacheStore.GetMatchServerByIPAddressAndPort(serverIPAddress, serverPort);

        if (server is null)
        {
            Log.Error(@"[BUG] Client Account ID ""{AccountID}"" Attempted To Join Match On Unknown Server ""{ServerAddress}""", Account.ID, serverAddress);

            return this;
        }

        Metadata.MatchServerConnectedTo = server;

        UpdateStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME, server);

        return this;
    }

    public async Task<ClientChatSession> JoinMatch(IDatabase distributedCacheStore, int matchID, bool joinMatchChannel = true)
    {
        // Leave Old Match Channels Before Joining The New One
        LeaveAllChannels(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_SERVER);

        if (matchID >= 0) // Client May Send -1 For The Match ID If There Is No Valid One (e.g. Match Aborted)
        {
            MatchInformation? matchInformation = await distributedCacheStore.GetMatchInformation(matchID);

            if (matchInformation is not null)
            {
                MatchInformation = matchInformation;

                MatchServer? server = await distributedCacheStore.GetMatchServerByID(matchInformation.ServerID);

                if (server is not null)
                    Metadata.MatchServerConnectedTo = server;
            }

            else
            {
                Log.Warning(@"Match ID {MatchID} Not Found In Cache For Session {SessionID}", matchID, ID);
            }

            // Join Match Channel If Match ID Is Valid And Client Should Join (Spectators/Mentors Don't Join)
            if (joinMatchChannel)
                JoinMatchChannel(matchID);
        }

        else
        {
            Log.Debug(@"Client Account ID ""{AccountID}"" Joined A Match With Invalid Match ID {MatchID} (Likely A Public Game)", Account.ID, matchID);
        }

        // Always Update Status To IN_GAME, Regardless Of Match ID Validity
        // The Match Server Should Already Be Set From PrepareToJoinMatch
        UpdateStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME, Metadata.MatchServerConnectedTo);

        return this;
    }

    /// <summary>
    ///     Joins a match-specific chat channel.
    ///     Uses SERVER | HIDDEN flags per original implementation.
    /// </summary>
    /// <param name="matchID">The match ID to join the channel for.</param>
    public ClientChatSession JoinMatchChannel(int matchID)
    {
        if (matchID < 0)
            return this;

        // Create Or Get The Match Channel (Uses SERVER | HIDDEN Flags)
        ChatChannel matchChannel = ChatChannel.GetOrCreateMatchChannel(matchID);

        // Join The Match Channel (Silent Join - No Full Broadcast To Channel Members)
        if (matchChannel.Members.ContainsKey(Account.Name) is false)
        {
            ChatChannelMember newMember = new (this, matchChannel);
            matchChannel.Members.TryAdd(Account.Name, newMember);
            CurrentChannels.Add(matchChannel.ID);
        }

        return this;
    }

    public ClientChatSession LeaveMatch()
    {
        // Leave Match Channels (Only The Channels With The SERVER Flag)
        LeaveAllChannels(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_SERVER);

        // Clear Match State
        Metadata.MatchServerConnectedTo = null;
        MatchInformation = null;

        // Rejoins Default Channel, Then Updates Status, So That Friends/Clan Members See The Player In A Channel When The Status Update Fires
        if (Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
            RejoinDefaultChannel();

        UpdateStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

        return this;
    }

    /// <summary>
    ///     Leaves all chat channels that match the specified flags.
    ///     If no flags are specified, leaves all channels.
    /// </summary>
    /// <param name="flags">Channel flags to filter which channels to leave. If zero, leaves all channels.</param>
    public ClientChatSession LeaveAllChannels(ChatProtocol.ChatChannelType flags = 0)
    {
        List<ChatChannel> channelsToLeave = [.. Context.ChatChannels.Values
            .Where(channel => channel.Members.ContainsKey(Account.Name))
            .Where(channel =>
            {
                // If No Flags Specified, Leave All Channels
                if (flags == 0)
                    return true;

                // Otherwise, Only Leave Channels That Have ALL Specified Flags
                return channel.Flags.HasFlag(flags);
            })];

        foreach (ChatChannel channel in channelsToLeave)
            channel.Leave(this);

        return this;
    }

    /// <summary>
    ///     Rejoins the default general chat channel.
    ///     Called when a player leaves a match to return them to the main chat area.
    /// </summary>
    public ClientChatSession RejoinDefaultChannel()
    {
        // Get Or Create The Next Available General Channel (With Overflow Support)
        ChatChannel defaultChannel = ChatChannel.GetOrCreateGeneralChannel();

        // Join The Default Channel If Not Already A Member
        if (defaultChannel.Members.ContainsKey(Account.Name) is false)
            defaultChannel.Join(this);

        return this;
    }

    /// <summary>
    ///     Updates the client's connection status and broadcasts it to all friends and clan members.
    ///     This method is the primary way to change and broadcast status changes.
    /// </summary>
    /// <param name="status">The new connection status.</param>
    /// <param name="matchServer">The match server, required when status is JOINING_GAME or IN_GAME.</param>
    public void UpdateStatus(ChatProtocol.ChatClientStatus status, MatchServer? matchServer = null)
    {
        // Same-Status Check: Prevent Redundant Broadcasts
        if (Metadata.LastKnownClientState == status)
            return;

        Metadata.LastKnownClientState = status;

        // Do Not Broadcast If Client Is Invisible
        if (Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
            return;

        BroadcastConnectionStatusUpdate(status, matchServer);
    }

    public ClientChatSession Reject(ChatProtocol.ChatRejectReason reason)
    {
        ChatBuffer reject = new ();

        reject.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
        reject.WriteInt8(Convert.ToByte(reason)); // Rejection Reason

        Send(reject);

        return this;
    }

    public void LogOut()
    {
        ChatBuffer logout = new ();

        logout.WriteCommand(ChatProtocol.Command.CHAT_CMD_LOGOUT);
        logout.WriteBool(true); // Whether Or Not To Log Out Staff Accounts

        Send(logout);

        // TODO: Send Notification With Logout Reason To Client
    }

    public void Terminate()
    {
        // If Account Is NULL, The Session Was Never Authenticated - Just Disconnect And Dispose
        if (Account is null)
        {
            Disconnect();
            Dispose();

            return;
        }

        // Leave All Chat Channels (With No Flags = All Channels)
        LeaveAllChannels();

        // Remove From Matchmaking Group If In One
        MatchmakingService.GetMatchmakingGroup(Account.ID)?.RemoveMember(Account.ID);

        // Remove Any Pending Clan Invites For This Account
        PendingClan.Invites.TryRemove(Account.ID, out _);

        // Remove Any Pending Clan Creations Founded By This Account
        PendingClan.Creations.TryRemove(Account.ID, out _);

        // Send Disconnection Notification To Online Peers (Friends And Clan Members)
        UpdateStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED);

        // Log The Client Out And Disconnect The Chat Session
        LogOut(); Disconnect();

        // Remove The Chat Session From The Chat Sessions Collection
        if (Context.ClientChatSessions.TryRemove(Account.Name, out ClientChatSession? _) is false)
            Log.Error(@"Failed To Remove Chat Session For Account Name ""{ClientInformation.Account.Name}""", Account.Name);

        // Dispose Of The Chat Session
        Dispose();
    }

    /// <summary>
    ///     Checks if this client should send an auto-response when receiving a whisper.
    ///     Returns the auto-response type if the client is AFK or DND, or NULL if no auto-response should be sent.
    /// </summary>
    /// <returns>The chat mode type if an auto-response is needed, or NULL if not.</returns>
    public ChatProtocol.ChatModeType? GetAutoResponseMode()
    {
        return Metadata.ClientChatModeState switch
        {
            ChatProtocol.ChatModeType.CHAT_MODE_AFK => ChatProtocol.ChatModeType.CHAT_MODE_AFK,
            ChatProtocol.ChatModeType.CHAT_MODE_DND => ChatProtocol.ChatModeType.CHAT_MODE_DND,
            _                                       => null
        };
    }

    /// <summary>
    ///     Determines if a whisper should be blocked based on this client's chat mode.
    ///     DND mode blocks whispers entirely (except from buddies/clan).
    ///     AFK mode allows whispers but sends an auto-response.
    /// </summary>
    /// <returns>TRUE if the whisper should be blocked, FALSE otherwise.</returns>
    public bool ShouldBlockWhisper()
    {
        return Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND;
    }

    /// <summary>
    ///     Checks if the specified account is a friend or clan member of this client.
    ///     Used to determine if messages from muted/blocking users should still be delivered.
    /// </summary>
    /// <param name="accountID">The account ID to check.</param>
    /// <returns>TRUE if the account is a friend or clan member, FALSE otherwise.</returns>
    public bool IsFriendOrClanMember(int accountID)
    {
        bool isFriend = Account.FriendedPeers.Any(friend => friend.ID == accountID);
        bool isClanMember = Account.Clan?.Members.Any(member => member.ID == accountID) ?? false;

        return isFriend || isClanMember;
    }

    /// <summary>
    ///     Sends the client's connection status packet to all friends and clan members that are currently online.
    ///     This is the internal method that constructs and sends the CHAT_CMD_UPDATE_STATUS packet.
    ///     Use <see cref="UpdateStatus"/> for public status changes.
    /// </summary>
    private void BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus status, MatchServer? matchServer = null)
    {
        List<int> clanMemberIDs = [.. Account.Clan?.Members.Select(clanMember => clanMember.ID) ?? []];
        List<int> friendIDs = [.. Account.FriendedPeers.Select(friend => friend.ID)];

        List<ClientChatSession> onlinePeerSessions = [.. Context.ClientChatSessions.Values
            .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Account.ID) || clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Account.ID))
            .Select(chatSession => chatSession).Distinct()]; // Get All Online Friends And Clan Members

        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);
        update.WriteInt32(Account.ID);                          // Client's Account ID
        update.WriteInt8(Convert.ToByte(status));               // Client's Status
        update.WriteInt8(Account.GetChatClientFlags());         // Client's Flags (Chat Client Type)
        update.WriteInt32(Account.Clan?.ID ?? 0);               // Client's Clan ID
        update.WriteString(Account.Clan?.Name ?? string.Empty); // Client's Clan Name
        update.WriteString(Account.ChatSymbolNoPrefixCode);     // Chat Symbol
        update.WriteString(Account.NameColourNoPrefixCode);     // Name Colour
        update.WriteString(Account.IconNoPrefixCode);           // Account Icon

        if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME || status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
        {
            if (matchServer is null)
            {
                Log.Error(@"[BUG] A Connection Status Update Was Requested For Account Name ""{ClientInformation.Account.Name}"" While Connected To A Match Server, But The Match Server Is NULL", Account.Name);

                return;
            }

            update.WriteString($"{matchServer.IPAddress}:{matchServer.Port}"); // Server Address This Client Is Connected To, In The Form Of "X.X.X.X:P"

            if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            {
                update.WriteString(MatchInformation?.MatchName ?? string.Empty);                // Match Name
                update.WriteInt32(MatchInformation?.MatchID ?? 0);                              // Match ID

                bool hasExtendedInfo = MatchInformation is not null;
                update.WriteBool(hasExtendedInfo);                                              // Has Extended Server Info

                if (hasExtendedInfo && MatchInformation is not null)
                {
                    update.WriteInt8(Convert.ToByte(MatchInformation.MatchType));               // Arranged Match Type
                    update.WriteString(Account.Name);                                           // Player Name
                    update.WriteString(matchServer.Location);                                   // Region
                    update.WriteString(MatchInformation.MatchMode.ToString());                  // Game Mode Name
                    update.WriteInt8(Convert.ToByte(MatchInformation.MaximumPlayersCount / 2)); // Team Size
                    update.WriteString(MatchInformation.Map);                                   // Map Name
                    update.WriteInt8(Convert.ToByte(MatchInformation.Tier));                    // Tier (Deprecated)

                    // Official Status: 0 = Unofficial (Deprecated), 1 = Official With Stats, 2 = Official Without Stats
                    byte officialStatus = MatchInformation.Options.HasFlag(MatchOptions.Official)
                        ? (MatchInformation.Options.HasFlag(MatchOptions.NoStatistics) ? (byte)2 : (byte)1)
                        : (byte)0;

                    update.WriteInt8(officialStatus);

                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.NoLeavers));         // No Leavers
                    update.WriteBool(false);                                                            // Private (Not Tracked In MatchInformation)
                    update.WriteBool(false);                                                            // All Heroes (Not Tracked In MatchInformation)
                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.CasualMode));        // Casual Mode
                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.AllRandom));         // Force Random (Deprecated)
                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.AutoBalanced));      // Auto-Balanced
                    update.WriteBool(false);                                                            // Advanced Options (Not Tracked In MatchInformation)
                    update.WriteInt16(0);                                                               // Minimum PSR (Not Tracked In MatchInformation)
                    update.WriteInt16(0);                                                               // Maximum PSR (Not Tracked In MatchInformation)
                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.DevelopmentHeroes)); // Development Heroes
                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.Hardcore));          // Hardcore
                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.VerifiedOnly));      // Verified Only
                    update.WriteBool(MatchInformation.Options.HasFlag(MatchOptions.Gated));             // Gated
                }
            }
        }

        update.WriteInt32(Account.AscensionLevel); // Client's Ascension Level

        foreach (ClientChatSession onlinePeerSession in onlinePeerSessions)
            onlinePeerSession.Send(update);
    }

    /// <summary>
    ///     Receive the connection status of all friends and clan members that are currently online.
    ///     Does not include invisible clients.
    /// </summary>
    private void ReceiveFriendAndClanMemberConnectionStatus()
    {
        List<int> clanMemberIDs = [.. Account.Clan?.Members.Select(clanMember => clanMember.ID) ?? []];
        List<int> friendIDs = [.. Account.FriendedPeers.Select(friend => friend.ID)];

        Log.Debug(@"Sending Initial Status To ""{AccountName}"": FriendCount={FriendCount}, ClanMemberCount={ClanMemberCount}",
            Account.Name, friendIDs.Count, clanMemberIDs.Count);

        List<ClientChatSession> onlinePeerSessions = Context.ClientChatSessions.Values
            .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Account.ID) || clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Account.ID))
            .Where(chatSession => chatSession.Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
            .Select(chatSession => chatSession).Distinct().ToList(); // Get All Online Friends And Clan Members That Are Not Invisible

        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Command.CHAT_CMD_INITIAL_STATUS);
        update.WriteInt32(onlinePeerSessions.Count); // Number Of Online Peers

        foreach (ClientChatSession onlinePeerSession in onlinePeerSessions)
        {
            ChatProtocol.ChatClientStatus status = onlinePeerSession.Metadata.LastKnownClientState;

            Log.Debug(@"Initial Status Peer: Name=""{Name}"", ID={ID}, NameColour=""{NameColour}"", Icon=""{Icon}"", AscensionLevel={AscensionLevel}",
                onlinePeerSession.Account.Name, onlinePeerSession.Account.ID, onlinePeerSession.Account.NameColourNoPrefixCode,
                onlinePeerSession.Account.IconNoPrefixCode, onlinePeerSession.Account.AscensionLevel);

            update.WriteInt32(onlinePeerSession.Account.ID);                      // Client's Account ID
            update.WriteInt8(Convert.ToByte(status));                             // Client's Status
            update.WriteInt8(onlinePeerSession.Account.GetChatClientFlags());     // Client's Flags (Chat Client Type)
            update.WriteString(onlinePeerSession.Account.NameColourNoPrefixCode); // Name Colour
            update.WriteString(onlinePeerSession.Account.IconNoPrefixCode);       // Account Icon

            if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME || status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            {
                // Use The Peer's Own Match Server, Not A Parameter
                MatchServer? peerMatchServer = onlinePeerSession.Metadata.MatchServerConnectedTo;

                if (peerMatchServer is null)
                {
                    // Peer Is In A Game State But Has No Stored Match Server - Skip Server Info
                    update.WriteString(string.Empty);

                    if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
                    {
                        update.WriteString(string.Empty);
                        update.WriteInt32(0);
                    }
                }

                else
                {
                    update.WriteString($"{peerMatchServer.IPAddress}:{peerMatchServer.Port}"); // Server Address This Client Is Connected To

                    if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
                    {
                        update.WriteString(onlinePeerSession.MatchInformation?.MatchName ?? string.Empty); // Match Name
                        update.WriteInt32(onlinePeerSession.MatchInformation?.MatchID ?? 0);               // Match ID
                    }
                }
            }

            update.WriteInt32(onlinePeerSession.Account.AscensionLevel); // Peer's Ascension Level
        }

        Send(update);
    }

    public ClientChatSession SendOptionsAndRemoteCommands()
    {
        ChatBuffer options = new ();

        string[] commands =
        [
            // The Required CVARs Are Already Set By WILLOWMAKER, So There Is No Need To Duplicate Them Here

            // Adding The Following Command Will Log The Client Directly Into A No-Stats Practice Match With Bots
            // "startgame practice GameName map:caldavar allheroes:true devheroes:true mode:botmatch nostats:true"
        ];

        options.WriteCommand(ChatProtocol.Command.CHAT_CMD_OPTIONS);
        options.WriteBool(false);                        // Upload To FTP On Demand (e.g. replays)
        options.WriteBool(true);                         // Upload To HTTP On Demand (e.g. replays)
        options.WriteBool(true);                         // Quests Are Enabled
        options.WriteBool(true);                         // Quest Ladder Is Enabled
        options.WriteBool(false);                        // Override Connect Resend Time
        options.WriteBool(true);                         // Enable Messages
        options.WriteString(string.Join('|', commands)); // Dynamic List Of Console Commands For The Client To Execute

        Send(options);

        return this;
    }
}
