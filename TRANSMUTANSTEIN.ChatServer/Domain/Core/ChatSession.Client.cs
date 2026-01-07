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
    public MatchStartData? MatchData { get; set; }

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

        // Notify Self, Clan Members, And Friends That This Client Is Now Connected
        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

        // Get The Connection Status Of All Friends And Clan Members That Are Currently Online (Excluding Invisible Clients)
        ReceiveFriendAndClanMemberConnectionStatus();

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

        Metadata.LastKnownClientState = ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME;

        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME, server);

        return this;
    }

    public async Task<ClientChatSession> JoinMatch(IDatabase distributedCacheStore, int matchID)
    {
        MatchStartData? matchData = await distributedCacheStore.GetMatchStartData(matchID);

        if (matchData is null)
        {
            Log.Error(@"[BUG] Match ID {MatchID} Not Found In Cache For Session {SessionID}", matchID, ID);

            return this;
        }

        MatchData = matchData;

        MatchServer? server = await distributedCacheStore.GetMatchServerByID(matchData.ServerID);

        if (server is null)
        {
            Log.Error(@"[BUG] Client Account ID ""{AccountID}"" Attempted To Join Match On Unknown Server", Account.ID);

            return this;
        }

        Metadata.LastKnownClientState = ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME;

        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME, server);

        return this;
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
        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED);

        ChatBuffer logout = new ();

        logout.WriteCommand(ChatProtocol.Command.CHAT_CMD_LOGOUT);
        logout.WriteBool(true); // Whether Or Not To Log Out Staff Accounts

        Send(logout);

        // TODO: Send Notification With Logout Reson To Client

        // TODO: Send Logout Notification To Friends And Clan Members
    }

    public void Terminate()
    {
        if (Account is not null)
        {
            // Get All Chat Channels The Client Is A Member Of
            List<ChatChannel> channels = [.. Context.ChatChannels.Values.Where(channel => channel.Members.ContainsKey(Account.Name))];

            // Remove The Client From All Chat Channels They Are A Member Of
            foreach (ChatChannel channel in channels)
                channel.Leave(this);

            // Remove From Matchmaking Group If In One
            MatchmakingService.GetMatchmakingGroup(Account.ID)?.RemoveMember(Account.ID);

            // Send Disconnection Notification To Online Peers (Friends And Clan Members)
            BroadcastDisconnection();

            // Log The Client Out And Disconnect The Chat Session
            LogOut();

            // Remove The Chat Session From The Chat Sessions Collection
            if (Context.ClientChatSessions.TryRemove(Account.Name, out ClientChatSession? _) is false)
                Log.Error(@"Failed To Remove Chat Session For Account Name ""{ClientInformation.Account.Name}""", Account.Name);
        }

        Disconnect();

        // Dispose Of The Chat Session
        Dispose();
    }

    /// <summary>
    ///     Sends a notification to each friend and clan member to notify them that the account has come online.
    /// </summary>
    public void BroadcastConnection()
    {
        // Get Friend And Clan Member Chat Sessions
        List<ClientChatSession> onlinePeerSessions = GetOnlinePeerSessions();

        // Notify Each Online Peer Of The Connection
        foreach (ClientChatSession onlinePeerSession in onlinePeerSessions)
        {
            ChatBuffer connect = new ();

            connect.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);
            connect.WriteString(Account.NameWithClanTag);                                                  // Client's Account Name
            connect.WriteInt32(Account.ID);                                                                // Client's Account ID
            connect.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)); // Chat Client Status
            connect.WriteInt8(Account.GetChatClientFlags());                                               // Client's Flags (Chat Client Type)
            connect.WriteInt32(Account.Clan?.ID ?? 0);                                                     // Client's Clan ID
            connect.WriteString(Account.Clan?.Name ?? string.Empty);                                       // Client's Clan Name
            connect.WriteString(Account.ChatSymbolNoPrefixCode);                                           // Account's Chat Symbol
            connect.WriteString(Account.NameColourNoPrefixCode);                                           // Account's Name Colour
            connect.WriteString(Account.IconNoPrefixCode);                                                 // Account's Icon

            // Send The Connection Notification To The Online Peer
            onlinePeerSession.Send(connect);
        }
    }

    /// <summary>
    ///     Sends a notification to each friend and clan member to notify them that the account is no longer online.
    /// </summary>
    public void BroadcastDisconnection()
    {
        // Get Friend And Clan Member Chat Sessions
        List<ClientChatSession> onlinePeerSessions = GetOnlinePeerSessions();

        // Notify Each Online Peer Of The Disconnection
        foreach (ClientChatSession onlinePeerSession in onlinePeerSessions)
        {
            ChatBuffer disconnect = new ();

            disconnect.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);
            disconnect.WriteString(Account.NameWithClanTag);                                                     // Client's Account Name
            disconnect.WriteInt32(Account.ID);                                                                   // Client's Account ID
            disconnect.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED)); // Chat Client Status
            disconnect.WriteInt8(Account.GetChatClientFlags());                                                  // Client's Flags (Chat Client Type)
            disconnect.WriteInt32(Account.Clan?.ID ?? 0);                                                        // Client's Clan ID
            disconnect.WriteString(Account.Clan?.Name ?? string.Empty);                                          // Client's Clan Name
            disconnect.WriteString(Account.ChatSymbolNoPrefixCode);                                              // Account's Chat Symbol
            disconnect.WriteString(Account.NameColourNoPrefixCode);                                              // Account's Name Colour
            disconnect.WriteString(Account.IconNoPrefixCode);                                                    // Account's Icon

            // Send The Disconnection Notification To The Online Peer
            onlinePeerSession.Send(disconnect);
        }
    }

    /// <summary>
    ///     Retrieves a list of chat sessions for all online peers who are either friends or clan members, excluding the current account.
    /// </summary>
    /// <remarks>
    ///     Duplicate sessions are removed from the returned list.
    ///     The current account is excluded from clan member sessions.
    /// </remarks>
    /// <returns>
    ///     A list of <see cref="ClientChatSession"/> objects representing active chat sessions with online friends and clan members.
    ///     The list is empty if no such sessions are available.
    /// </returns>
    private List<ClientChatSession> GetOnlinePeerSessions()
    {
        // Get All Friend IDs
        HashSet<int> friendIDs = [.. Account.FriendedPeers.Select(friend => friend.ID)];

        // Get All Friend Chat Sessions
        List<ClientChatSession> friendSessions = [.. Context.ClientChatSessions.Values
            .Where(chatSession => friendIDs.Contains(chatSession.Account.ID))];

        // Get All Clan Member IDs (Excluding Self)
        HashSet<int> clanMemberIDs = [.. Account.Clan?.Members
            .Where(clanMember => clanMember.ID != Account.ID)
            .Select(clanMember => clanMember.ID) ?? []];

        // Get All Clan Member Chat Sessions (Excluding Self)
        List<ClientChatSession> clanMemberSessions = [.. Context.ClientChatSessions.Values
            .Where(chatSession => clanMemberIDs.Contains(chatSession.Account.ID))];

        // Combine Friend And Clan Member Sessions, Removing Duplicates
        List<ClientChatSession> onlinePeerSessions = [.. friendSessions.Concat(clanMemberSessions).Distinct()];

        return onlinePeerSessions;
    }

    /// <summary>
    ///     Sends the client's connection status to all friends and clan members that are currently online.
    ///     Also sends the client's connection status to the client itself, so they can see their own status.
    /// </summary>
    private void BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus status, MatchServer? matchServer = null)
    {
        if (Metadata.LastKnownClientState == status) return; else Metadata.LastKnownClientState = status;

        if (Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
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
                    // TODO: Populate With Real Match Name
                    update.WriteString(string.Empty);               // Match Name
                    // TODO: Populate With Real Match ID
                    update.WriteInt32(default(int));                // Match ID
                    update.WriteBool(false);                        // Has Extended Server Info

                    // TODO: Set Extended Server Info To TRUE And Populate The Following Fields

                    /*
                        [1] EArrangedMatchType - arranged match type
                        [X] string - client's name
                        [X] string - server's region
                        [X] string - server's game mode
                        [1] unsigned char - server's team size
                        [X] string - server's map name
                        [1] unsigned char - server's tier (deprecated)
                        [1] unsigned char - server's official status (0 = unofficial (deprecated), 1 = official with stats, 2 = official without stats)
                        [1] bool - server's "no leavers" flag
                        [1] bool - server's "private" flag
                        [1] bool - server's "all heroes" flag
                        [1] bool - server's "casual mode" flag
                        [1] bool - server's "all random" flags (deprecated)
                        [1] bool - server's "auto balanced" flag
                        [1] bool - server's "advanced options" flag
                        [2] unsigned short - server's minimum PSR allowed
                        [2] unsigned short - server's maximum PSR allowed
                        [1] bool - server's "dev heroes" flag
                        [1] bool - server's "hardcore" flag
                        [1] bool - server's "verified only" flag
                        [1] bool - server's "gated" flag

                        or ...

                        << pJoinedServer->GetArrangedMatchType()                         // Arranged Match Type (0 = Public, 1 = Matchmaking, 2 = Scheduled match, 3 = Unscheduled match, 4 = Matchmaking midwars)
                        << GetNameUTF8() << byte('\0')                                   // Player Name
                        << WStringToUTF8(pJoinedServer->GetLocation()) << byte('\0')     // Region
                        << WStringToUTF8(pJoinedServer->GetGameModeName()) << byte('\0') // Game Mode Name (banningdraft)
                        << pJoinedServer->GetTeamSize()                                  // Team Size
                        << WStringToUTF8(pJoinedServer->GetMapName()) << byte('\0')      // Map Name (caldavar)
                        << pJoinedServer->GetTier()                                      // Tier - Noobs Only (0), Noobs Allowed (1), Pro (2)
                        << pJoinedServer->GetOfficial()                                  // 0 - Unofficial, 1 - Official w/ stats, 2 - Official w/o stats
                        << pJoinedServer->GetNoLeaver()                                  // No Leavers (1), Leavers (0)
                        << pJoinedServer->GetPrivate()                                   // Private (1), Not Private (0)
                        << pJoinedServer->GetAllHeroes()                                 // All Heroes (1), Not All Heroes (0)
                        << pJoinedServer->GetCasualMode()                                // Casual Mode (1), Not Casual Mode (0)
                        << pJoinedServer->GetForceRandom()                               // Force Random (1), Not Force Random (0) -- (NOTE: Deprecated)
                        << pJoinedServer->GetAutoBalanced()                              // Auto Balanced (1), Non Auto Balanced (0)
                        << pJoinedServer->GetAdvancedOptions()                           // Advanced Options	(1), No Advanced Options (0)
                        << pJoinedServer->GetMinPSR()                                    // Min PSR
                        << pJoinedServer->GetMaxPSR()                                    // Max PSR
                        << pJoinedServer->GetDevHeroes()                                 // Dev Heroes (1), Non Dev Heroes (0)
                        << pJoinedServer->GetHardcore()                                  // Hardcore (1), Non Hardcore (0)
                        << pJoinedServer->GetVerifiedOnly()                              // Verified Only (1), Everyone (0)
                        << pJoinedServer->GetGated()                                     // Gated (1), Non Gated (0)
                    */
                }
            }

            update.WriteInt32(Account.AscensionLevel);              // Client's Ascension Level

            foreach (ClientChatSession onlinePeerSession in onlinePeerSessions)
                onlinePeerSession.Send(update);
        }
    }

    /// <summary>
    ///     Receive the connection status of all friends and clan members that are currently online.
    ///     Does not include invisible clients.
    /// </summary>
    private void ReceiveFriendAndClanMemberConnectionStatus(MatchServer? matchServer = null)
    {
        List<int> clanMemberIDs = [.. Account.Clan?.Members.Select(clanMember => clanMember.ID) ?? []];
        List<int> friendIDs = [.. Account.FriendedPeers.Select(friend => friend.ID)];

        List<ClientChatSession> onlinePeerSessions = Context.ClientChatSessions.Values
            .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Account.ID) || clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Account.ID))
            .Where(chatSession => chatSession.Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
            .Select(chatSession => chatSession).Distinct().ToList(); // Get All Online Friends And Clan Members That Are Not Invisible

        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Command.CHAT_CMD_INITIAL_STATUS);
        update.WriteInt32(onlinePeerSessions.Count);                               // Number Of Online Peers

        foreach (ClientChatSession onlinePeerSession in onlinePeerSessions)
        {
            ChatProtocol.ChatClientStatus status = onlinePeerSession.Metadata.LastKnownClientState;

            update.WriteInt32(onlinePeerSession.Account.ID);                       // Client's Account ID
            update.WriteInt8(Convert.ToByte(status));                              // Client's Status
            update.WriteInt8(onlinePeerSession.Account.GetChatClientFlags());      // Client's Flags (Chat Client Type)
            update.WriteString(onlinePeerSession.Account.NameColourNoPrefixCode);  // Name Colour
            update.WriteString(onlinePeerSession.Account.IconNoPrefixCode);        // Account Icon

            if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME || status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            {
                if (matchServer is null)
                {
                    Log.Error(@"[BUG] A Connection Status Update Was Requested For Account Name ""{ClientInformation.Account.Name}"" While Connected To A Match Server, But The Match Server Is NULL", Account.Name);

                    continue;
                }

                update.WriteString($"{matchServer.IPAddress}:{matchServer.Port}"); // Server Address This Client Is Connected To, In The Form Of "X.X.X.X:P"

                if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
                {
                    // TODO: Populate With Real Match Name
                    update.WriteString(string.Empty);                              // Match Name
                    // TODO: Populate With Real Match ID
                    update.WriteInt32(default(int));                               // Match ID
                }
            }

            update.WriteInt32(Account.AscensionLevel);                             // Client's Ascension Level

            onlinePeerSession.Send(update);
        };
    }

    public ClientChatSession SendOptionsAndRemoteCommands()
    {
        ChatBuffer options = new ();

        string[] commands =
        [
            "cg_24hourClock true",
            "con_showNet true",
            "http_printDebugInfo true",
            "php_printDebugInfo true",
            "sys_dumpOnFatal true"

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
