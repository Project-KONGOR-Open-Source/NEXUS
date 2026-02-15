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

    public async Task<ClientChatSession> JoinMatch(IDatabase distributedCacheStore, int matchID)
    {
        // Client May Send -1 As Match ID If They Don't Have A Valid One (e.g. Joining A Public Game)
        // Legacy HON-Chat-Server Behaviour: Status Is ALWAYS Updated To IN_GAME, Even With Invalid Match ID
        // Only The Match Channel Join Is Skipped For Invalid Match IDs
        if (matchID >= 0)
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

            // TODO: Join Match Channel If Match ID Is Valid
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

    public ClientChatSession LeaveMatch()
    {
        Metadata.MatchServerConnectedTo = null;
        MatchInformation = null;

        UpdateStatus(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

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

        // Get All Chat Channels The Client Is A Member Of
        List<ChatChannel> channels = [.. Context.ChatChannels.Values.Where(channel => channel.Members.ContainsKey(Account.Name))];

        // Remove The Client From All Chat Channels They Are A Member Of
        foreach (ChatChannel channel in channels)
            channel.Leave(this);

        // Remove From Matchmaking Group If In One
        MatchmakingService.GetMatchmakingGroup(Account.ID)?.RemoveMember(Account.ID);

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

    /// <summary>
    ///     Receive the connection status of all friends and clan members that are currently online.
    ///     Does not include invisible clients.
    /// </summary>
    private void ReceiveFriendAndClanMemberConnectionStatus()
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

            update.WriteInt32(onlinePeerSession.Account.AscensionLevel);           // Peer's Ascension Level
        }

        Send(update);
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
