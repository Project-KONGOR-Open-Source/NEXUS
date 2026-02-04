using System.Globalization;

using MERRICK.DatabaseContext.Extensions;
using global::TRANSMUTANSTEIN.ChatServer.Contracts;

namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public partial class ChatSession
{
    private readonly object _cleanupLock = new();

    /// <summary>
    ///     Gets set after a successful client handshake following the <see cref="Accept" /> method.
    ///     Contains metadata about the client connected to this chat session.
    ///     This property is NULL before authentication, but is guaranteed non-NULL after <see cref="Accept" /> is called.
    /// </summary>
    public ClientChatSessionMetadata ClientMetadata { get; set; } = null!;

    /// <summary>
    ///     Tracks the channel IDs that this client is currently a member of.
    ///     The maximum number of channels cannot exceed the value of <see cref="ChatProtocol.MAX_CHANNELS_PER_CLIENT" />.
    /// </summary>
    public HashSet<int> CurrentChannels { get; set; } = [];

    public ChatSession Accept(Account account)
    {
        // Link The Account To The Chat Session
        Account = account;

        // Add The Chat Session To The Chat Sessions Collection
        _chatContext.ClientChatSessions.AddOrUpdate(account.Name, this, (key, existing) => this);

        ChatBuffer accept = new();

        accept.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        Send(accept);

        // Send Options And Remote Commands To Client (Must Be Sent Before Initial Status)
        SendOptionsAndRemoteCommands();

        // Get The Connection Status Of All Friends And Clan Members That Are Currently Online (Excluding Invisible Clients)
        ReceiveFriendAndClanMemberConnectionStatus();

        // Notify Self, Clan Members, And Friends That This Client Is Now Connected
        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

        return this;
    }

    public async Task<ChatSession> PrepareToJoinMatch(IDatabase distributedCacheStore, string serverAddress)
    {
        List<string> serverAddressParts = [.. serverAddress.Split(':')];

        if (serverAddressParts.Count is not 2)
        {
            Log.Error(@"[BUG] Invalid Match Server Address ""{MatchServerAddress}""", serverAddress);

            return this;
        }

        string serverIPAddress = serverAddressParts.First();
        int serverPort = Convert.ToInt32(serverAddressParts.Last(), CultureInfo.InvariantCulture);

        MatchServer? server = await distributedCacheStore.GetMatchServerByIPAddressAndPort(serverIPAddress, serverPort);

        if (server is null)
        {
            Log.Error(
                @"[BUG] Client Account ID ""{AccountID}"" Attempted To Join Match On Unknown Server ""{ServerAddress}""",
                Account.ID, serverAddress);

            return this;
        }

        // Store Match Server Info In Metadata
        ClientMetadata.MatchServerAddress = $"{server.IPAddress}:{server.Port}";

        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME, server);

        return this;
    }

    public async Task<ChatSession> JoinMatch(IDatabase distributedCacheStore, int matchID)
    {
        MatchStartData? matchData = await distributedCacheStore.GetMatchStartData(matchID);

        if (matchData is null)
        {
            Log.Error(@"[BUG] Match ID {MatchID} Not Found In Cache For Session {SessionID}", matchID, ID);

            return this;
        }

        MatchServer? server = await distributedCacheStore.GetMatchServerByID(matchData.ServerID);

        if (server is null)
        {
            Log.Error(@"[BUG] Client Account ID ""{AccountID}"" Attempted To Join Match On Unknown Server", Account.ID);

            return this;
        }

        // Store Match ID In Metadata
        ClientMetadata.MatchID = matchID;

        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME, server);

        return this;
    }

    public ChatSession Reject(ChatProtocol.ChatRejectReason reason)
    {
        ChatBuffer reject = new();

        reject.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
        reject.WriteInt8(Convert.ToByte(reason, CultureInfo.InvariantCulture)); // Rejection Reason

        Send(reject);

        return this;
    }

    public void LogOut()
    {
        BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED);

        ChatBuffer logout = new();

        logout.WriteCommand(ChatProtocol.Command.CHAT_CMD_LOGOUT);
        logout.WriteBool(true); // Whether Or Not To Log Out Staff Accounts

        Send(logout);

        // TODO: Send Notification With Logout Reson To Client

        // TODO: Send Logout Notification To Friends And Clan Members
    }

    public void Cleanup()
    {
        lock (_cleanupLock)
        {
            Account account = Account; // Cache Account to local variable

            if (account is null)
            {
                return;
            }

            // Get All Chat Channels The Client Is A Member Of
            List<ChatChannel> channels =
                [.. _chatContext.ChatChannels.Values.Where(channel => channel.Members.ContainsKey(account.Name))];

            // Remove The Client From All Chat Channels They Are A Member Of
            foreach (ChatChannel channel in channels)
            {
                channel.Leave(_chatContext, this);
            }

            // Remove From Matchmaking Group If In One
            _chatContext.Matchmaking.GetMatchmakingGroup(account.ID)?.RemoveMember(_chatContext.Matchmaking, account.ID);

            // Log The Client Out And Disconnect The Chat Session
            LogOut();

            // Remove The Chat Session From The Chat Sessions Collection
            if (_chatContext.ClientChatSessions.TryRemove(account.Name, out ChatSession? _) is false)
            {
                Log.Error(@"Failed To Remove Chat Session For Account Name ""{ClientInformation.Account.Name}""",
                    account.Name);
            }

            // Prevent Double-Cleanup
            Account = null!;
        }
    }

    public void TerminateClient()
    {
        Cleanup();

        Disconnect();

        // Dispose Of The Chat Session
        Dispose();
    }


    /// <summary>
    ///     Retrieves a list of chat sessions for all online peers who are observing the current account.
    ///     This includes:
    ///     1. Users who have added the current account as a friend (Reverse Lookup).
    ///     2. Clan members (Mutual).
    /// </summary>
    /// <remarks>
    ///     Duplicate sessions are removed from the returned list.
    ///     The current account is excluded from the list.
    /// </remarks>
    /// <returns>
    ///     A list of <see cref="ChatSession" /> objects representing active chat sessions of observers.
    ///     The list is empty if no such sessions are available.
    /// </returns>
    private List<ChatSession> GetOnlineObservers()
    {
        // Get All Clan Member IDs (Excluding Self) - Mutual Relationship
        HashSet<int> clanMemberIDs =
        [
            .. (Account.Clan?.Members ?? [])
            .Where(clanMember => clanMember.ID != Account.ID)
            .Select(clanMember => clanMember.ID)
        ];

        // Find Sessions That Should Be Notified
        List<ChatSession> observerSessions =
        [
            .. _chatContext.ClientChatSessions.Values
                .Where(chatSession =>
                    chatSession.Account.ID != Account.ID &&
                    (
                        // Case 1: They have friended ME (Reverse Lookup)
                        chatSession.Account.FriendedPeers.Any(fp => fp.ID == Account.ID) ||
                        // Case 2: They are in my CLAN (Mutual)
                        clanMemberIDs.Contains(chatSession.Account.ID)
                    )
                )
        ];

        return observerSessions;
    }

    /// <summary>
    ///     Sends the client's connection status to all friends and clan members that are currently online.
    ///     Also sends the client's connection status to the client itself, so they can see their own status.
    /// </summary>
    public void BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus status, MatchServer? matchServer = null)
    {
        ClientMetadata.LastKnownClientState = status;

        if (ClientMetadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            return;
        }

        // Get All Online Observers (People who friend ME or are in my CLAN)
        List<ChatSession> onlineObserverSessions = GetOnlineObservers();

        Log.Information(
            "Broadcasting Status Update {Status} for Account {AccountID} ({AccountName}) to {ObserverCount} Observers: {ObserverNames}",
            status, Account.ID, Account.Name, onlineObserverSessions.Count,
            string.Join(", ", onlineObserverSessions.Select(s => s.Account?.Name ?? "Unknown")));

        ChatBuffer update = new();

        update.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);
        update.WriteInt32(Account.ID); // Client's Account ID
        update.WriteInt8(Convert.ToByte(status, CultureInfo.InvariantCulture)); // Client's Status
        update.WriteInt8(Account.GetChatClientFlags()); // Client's Flags (Chat Client Type)
        update.WriteInt32(Account.Clan?.ID ?? 0); // Client's Clan ID
        update.WriteString(Account.Clan?.Name ?? string.Empty); // Client's Clan Name
        update.WriteString(Account.GetChatSymbolNoPrefixCode()); // Chat Symbol
        update.WriteString(Account.GetNameColourNoPrefixCode()); // Name Colour
        update.WriteString(Account.GetIconNoPrefixCode()); // Account Icon

        if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME ||
            status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
        {
            if (matchServer is null)
            {
                Log.Error(
                    @"[BUG] A Connection Status Update Was Requested For Account Name ""{ClientInformation.Account.Name}"" While Connected To A Match Server, But The Match Server Is NULL",
                    Account.Name);

                return;
            }

            update.WriteString(
                $"{matchServer.IPAddress}:{matchServer.Port}"); // Server Address This Client Is Connected To, In The Form Of "X.X.X.X:P"

            if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            {
                // TODO: Populate With Real Match Name
                update.WriteString(string.Empty); // Match Name
                // TODO: Populate With Real Match ID
                update.WriteInt32(default); // Match ID
                update.WriteBool(false); // Has Extended Server Info

                // TODO: Set Extended Server Info To TRUE And Populate The Following Fields
            }
        }

        update.WriteInt32(Account.AscensionLevel); // Client's Ascension Level

        foreach (ChatSession onlineObserverSession in onlineObserverSessions)
        {
            onlineObserverSession.Send(update);
        }

        // Also send to self so the client knows its status has changed
        Send(update);
    }

    /// <summary>
    ///     Receive the connection status of all friends and clan members that are currently online.
    ///     Does not include invisible clients.
    /// </summary>
    public void ReceiveFriendAndClanMemberConnectionStatus(MatchServer? matchServer = null)
    {
        List<int> clanMemberIDs = [.. Account.Clan?.Members.Select(clanMember => clanMember.ID) ?? []];
        List<int> friendIDs = [.. Account.FriendedPeers.Select(friend => friend.ID)];

        List<ChatSession> onlinePeerSessions = _chatContext.ClientChatSessions.Values
            .Where(chatSession => friendIDs.Any(friendID => friendID == chatSession.Account.ID) ||
                                  clanMemberIDs.Any(clanMemberID => clanMemberID == chatSession.Account.ID))
            .Where(chatSession =>
                chatSession.ClientMetadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
            .Where(chatSession => chatSession.Account.ID != Account.ID)
            .Select(chatSession => chatSession).Distinct()
            .ToList(); // Get All Online Friends And Clan Members That Are Not Invisible

        ChatBuffer update = new();

        update.WriteCommand(ChatProtocol.Command.CHAT_CMD_INITIAL_STATUS);
        update.WriteInt32(onlinePeerSessions.Count); // Number Of Online Peers

        foreach (ChatSession onlinePeerSession in onlinePeerSessions)
        {
            ChatProtocol.ChatClientStatus status = onlinePeerSession.ClientMetadata.LastKnownClientState;

            update.WriteInt32(onlinePeerSession.Account.ID); // Client's Account ID
            update.WriteInt8(Convert.ToByte(status, CultureInfo.InvariantCulture)); // Client's Status
            update.WriteInt8(onlinePeerSession.Account.GetChatClientFlags()); // Client's Flags (Chat Client Type)
            update.WriteString(onlinePeerSession.Account.GetNameColourNoPrefixCode()); // Name Colour
            update.WriteString(onlinePeerSession.Account.GetIconNoPrefixCode()); // Account Icon

            if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME ||
                status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            {
                string? matchServerAddress = onlinePeerSession.ClientMetadata.MatchServerAddress;

                if (string.IsNullOrEmpty(matchServerAddress))
                {
                    // Fallback If Metadata Is Missing (Should Not Happen With Fix)
                    Log.Error(@"[BUG] Peer Account ""{PeerName}"" Is {Status} But MatchServerAddress Is Missing",
                        onlinePeerSession.Account.Name, status);
                    update.WriteString("0.0.0.0:0");
                }
                else
                {
                    update.WriteString(matchServerAddress);
                }

                if (status is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
                {
                    // TODO: Populate With Real Match Name
                    update.WriteString(string.Empty); // Match Name (Legacy: GameName)

                    // Populate With Real Match ID
                    update.WriteInt32(onlinePeerSession.ClientMetadata.MatchID ?? 0); // Match ID
                }
            }

            update.WriteInt32(onlinePeerSession.Account.AscensionLevel); // Client's Ascension Level
        }

        Send(update);
    }

    public ChatSession SendOptionsAndRemoteCommands()
    {
        ChatBuffer options = new();

        string[] commands =
        [
            "cg_24hourClock true",
            "con_showNet true",
            "http_printDebugInfo true",
            "php_printDebugInfo true",
            "sys_dumpOnFatal true"
        ];

        options.WriteCommand(ChatProtocol.Command.CHAT_CMD_OPTIONS);
        options.WriteBool(false); // Upload To FTP On Demand (e.g. replays)
        options.WriteBool(true); // Upload To HTTP On Demand (e.g. replays)
        options.WriteBool(true); // Quests Are Enabled
        options.WriteBool(true); // Quest Ladder Is Enabled
        options.WriteBool(false); // Override Connect Resend Time
        options.WriteBool(true); // Enable Messages
        options.WriteString(string.Join('|', commands)); // Dynamic List Of Console Commands For The Client To Execute

        Send(options);

        return this;
    }
}