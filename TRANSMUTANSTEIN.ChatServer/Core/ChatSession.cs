namespace TRANSMUTANSTEIN.ChatServer.Core;

public partial class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    /// <summary>
    ///     Gets set after a successful client handshake following the <see cref="Accept"/> method.
    ///     Contains metadata about the client connected to this chat session.
    ///     This property is NULL before authentication, but is guaranteed non-NULL after <see cref="Accept"/> is called.
    /// </summary>
    public ChatSessionMetadata Metadata { get; set; } = null!;

    /// <summary>
    ///     Gets set after a successful client handshake following the <see cref="Accept"/> method.
    ///     Contains the account information of the client connected to this chat session.
    ///     This property is NULL before authentication, but is guaranteed non-NULL after <see cref="Accept"/> is called.
    /// </summary>
    public Account Account { get; set; } = null!;

    /// <summary>
    ///     Tracks the channel IDs that this client is currently a member of.
    ///     The maximum number of channels cannot exceed the value of <see cref="ChatProtocol.MAX_CHANNELS_PER_CLIENT"/>.
    /// </summary>
    public HashSet<int> CurrentChannels { get; set; } = [];

    public ChatSession Accept(ChatSessionMetadata metadata, Account account)
    {
        // Embed The Client Information In The Chat Session
        Metadata = metadata;

        // Link The Account To The Chat Session
        Account = account;

        // Add The Chat Session To The Chat Sessions Collection
        Context.ChatSessions.AddOrUpdate(account.Name, this, (key, existing) => this);

        ChatBuffer accept = new ();

        accept.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        Send(accept);

        // Notify Self, Clan Members, And Friends That This Client Is Now Connected
        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

        // Get The Connection Status Of All Friends And Clan Members That Are Currently Online (Excluding Invisible Clients)
        ReceiveFriendAndClanMemberConnectionStatus();

        return this;
    }

    public ChatSession Reject(ChatProtocol.ChatRejectReason reason)
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
        // Get All Chat Channels The Client Is A Member Of
        List<ChatChannel> channels = [.. Context.ChatChannels.Values.Where(channel => channel.Members.ContainsKey(Account.Name))];

        // Remove The Client From All Chat Channels They Are A Member Of
        foreach (ChatChannel channel in channels)
            channel.Leave(this);

        // Remove From Matchmaking Group If In One
        MatchmakingService.GetMatchmakingGroup(Account.ID)?.RemoveMember(Account.ID);

        // Log The Client Out And Disconnect The Chat Session
        LogOut(); Disconnect();

        // Remove The Chat Session From The Chat Sessions Collection
        if (Context.ChatSessions.TryRemove(Account.Name, out ChatSession? _) is false)
            Log.Error(@"Failed To Remove Chat Session For Account Name ""{ClientInformation.Account.Name}""", Account.Name);

        // Dispose Of The Chat Session
        Dispose();
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

            List<ChatSession> onlinePeerSessions = Context.ChatSessions.Values
                .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Account.ID) || clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Account.ID))
                .Select(chatSession => chatSession).Distinct().ToList(); // Get All Online Friends And Clan Members

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

            foreach (ChatSession onlinePeerSession in onlinePeerSessions)
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

        List<ChatSession> onlinePeerSessions = Context.ChatSessions.Values
            .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Account.ID) || clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Account.ID))
            .Where(chatSession => chatSession.Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
            .Select(chatSession => chatSession).Distinct().ToList(); // Get All Online Friends And Clan Members That Are Not Invisible

        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Command.CHAT_CMD_INITIAL_STATUS);
        update.WriteInt32(onlinePeerSessions.Count);                               // Number Of Online Peers

        foreach (ChatSession onlinePeerSession in onlinePeerSessions)
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

    public ChatSession SendOptionsAndRemoteCommands()
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

public partial class ChatSession
{
    private static ConcurrentDictionary<ushort, Type> CommandToTypeMap { get; set; } = [];

    private byte[] RemainingPreviouslyReceivedData { get; set; } = [];

    protected override void OnConnected()
    {
        Log.Information("Chat Session ID {SessionID} Was Created", ID);
    }

    protected override void OnError(SocketError error)
    {
        Log.Information("Chat Session ID {SessionID} Caught A Socket Error With Code {SocketErrorCode}", ID, error);
    }

    protected override void OnDisconnected()
    {
        Log.Information("Chat Session ID {SessionID} Has Terminated", ID);
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        byte[] received = RemainingPreviouslyReceivedData.Concat(buffer[(int) offset..(int) size]).ToArray();

        List<byte[]> segments = ExtractDataSegments(received, out byte[] remaining);

        RemainingPreviouslyReceivedData = remaining;

        foreach (byte[] segment in segments)
            ProcessDataSegment(segment);
    }

    private static List<byte[]> ExtractDataSegments(byte[] buffer, out byte[] remaining)
    {
        remaining = [];

        List<byte[]> segments = [];

        int offset = 0;

        while (offset < buffer.Length)
        {
            ushort size = BitConverter.ToUInt16([buffer[offset + 0], buffer[offset + 1]]);

            if (size + 2 <= buffer.Length - offset)
            {
                byte[] segment = buffer[(offset + 2)..(offset + 2 + size)];

                segments.Add(segment);

                offset += 2 + size;
            }

            else
            {
                remaining = buffer[offset..buffer.Length];

                offset = buffer.Length;
            }
        }

        return segments;
    }

    private void ProcessDataSegment(byte[] segment)
    {
        ushort command = BitConverter.ToUInt16([segment[0], segment[1]]);

        Type? commandType = GetCommandType(command);

        if (commandType is null)
        {
            string output = new StringBuilder($@"Missing Type Mapping For Command: ""0x{command:X4}""")
                .Append(Environment.NewLine).Append($"Message UTF8 Bytes: {string.Join(':', segment)}")
                .Append(Environment.NewLine).Append($"Message UTF8 Text: {Encoding.UTF8.GetString(segment)}")
                .ToString();

            Log.Error(output);
        }

        else
        {
            if (TRANSMUTANSTEIN.RunsInDevelopmentMode)
                Log.Debug(@"Processing Command: ""0x{Command}""", command.ToString("X4"));

            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
            {
                try
                {
                    ChatBuffer buffer = new (segment);

                    commandTypeInstance.Switch
                        (synchronous => synchronous.Process(this, buffer), async asynchronous => await asynchronous.Process(this, buffer));
                }

                catch (Exception exception)
                {
                    Log.Error(exception, @"[BUG] Error Processing Command: ""0x{Command}""", command.ToString("X4"));
                }
            }

            else Log.Error(@"[BUG] Could Not Create Command Type Instance For Command: ""0x{Command}""", command.ToString("X4"));
        }
    }

    private Type? GetCommandType(ushort command)
    {
        if (CommandToTypeMap.TryGetValue(command, out Type? type))
            return type;

        Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

        type = types.SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null
            && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

        if (type is not null)
            if (CommandToTypeMap.TryAdd(command, type) is false && CommandToTypeMap.ContainsKey(command) is false)
                Log.Error(@"[BUG] Could Not Add Command-To-Type Mapping For Command ""0x{Command}"" And Type ""{TypeName}""", command.ToString("X4"), type.Name);

        return type;
    }

    private OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>? GetCommandTypeInstance(Type type)
    {
        object instance = ActivatorUtilities.CreateInstance(serviceProvider, type);

        if (instance is ISynchronousCommandProcessor synchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT0(synchronous);

        if (instance is IAsynchronousCommandProcessor asynchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT1(asynchronous);

        Log.Error(@"[BUG] Command Type ""{TypeName}"" Does Not Implement A Supported Processor Interface", type.Name);

        return null;
    }

    /// <summary>
    ///     Send buffer data with non-destructive size prefixing, which does not mutate the original buffer.
    ///     Enforces 16KB (16384 bytes) maximum packet size, as to not exceed the chat protocol's limitations.
    /// </summary>
    public bool Send(ChatBuffer buffer)
    {
        if (buffer.Size > ChatProtocol.MAX_PACKET_SIZE)
        {
            Log.Error(@"Packet Of {PacketSize} Bytes Exceeds Maximum Allowed Size Of {MaximumPacketSize} Bytes", buffer.Size, ChatProtocol.MAX_PACKET_SIZE);

            return false;
        }

        short messageLength = Convert.ToInt16(buffer.Size);

        byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
        byte[] messageBytes = new byte[messageLengthBytes.Length + messageLength];

        Array.Copy(messageLengthBytes, 0, messageBytes, 0, messageLengthBytes.Length);
        Array.Copy(buffer.Data, 0, messageBytes, messageLengthBytes.Length, messageLength);

        return SendAsync(messageBytes);
    }
}
