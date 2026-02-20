namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles match announcements from game servers.
///     This is sent by the game server after it has successfully set up the match.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH)]
public class MatchAnnounce(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAnnounceRequestData requestData = new (buffer);

        Log.Information(@"Match Announce Received: ServerID={ServerID}, CorrelationID={CorrelationID}, MatchID={MatchID}",
            session.Metadata.ServerID, requestData.CorrelationID, requestData.MatchID);

        // Find The Pending Match By Correlation ID
        if (MatchmakingService.ActiveMatches.Values.SingleOrDefault(match => match.CorrelationID == requestData.CorrelationID) is not { } pendingMatch)
        {
            Log.Warning(@"Match Announce Received For Unknown CorrelationID: {CorrelationID}", requestData.CorrelationID);

            return;
        }

        // Update Match State
        pendingMatch.State = MatchmakingMatchState.ServerAllocated;

        // Use The Server Address And Port That Were Set When The Match Was Spawned
        // These Come From The MatchServer Cache Entry And Represent The Game Server's Address
        string serverAddress = pendingMatch.ServerAddress ?? throw new NullReferenceException("Pending Match Server Address Is NULL");
        ushort serverPort = pendingMatch.ServerPort ?? throw new NullReferenceException("Pending Match Server Port Is NULL");

        // If The Match Doesn't Have The Server Address, Try To Get It From The Session's Remote Endpoint
        if (string.IsNullOrEmpty(pendingMatch.ServerAddress) && session.Socket.RemoteEndPoint is IPEndPoint remoteEndPoint)
            serverAddress = remoteEndPoint.Address.ToString();

        // Create MatchInformation Now That We Have The Real Match ID From The Game Server
        MatchServer? server = await distributedCacheStore.GetMatchServerByID(pendingMatch.AssignedServerID ?? 0);

        if (server is not null)
        {
            MatchInformation matchInformation = MatchmakingService.CreateMatchInformation(pendingMatch, server, requestData.MatchID);

            await distributedCacheStore.SetMatchInformation(matchInformation);

            Log.Information(@"MatchInformation Cached: MatchID={MatchID}, MatchName={MatchName}", requestData.MatchID, matchInformation.MatchName);
        }

        else
        {
            Log.Warning(@"Could Not Create MatchInformation: Server Not Found For ServerID {ServerID}", pendingMatch.AssignedServerID);
        }

        // Send AutoMatchConnect To All Players
        // MatchFoundUpdate And FoundServerUpdate Were Already Sent From SpawnMatch
        SendAutoMatchConnect(pendingMatch, requestData.MatchID, serverAddress, serverPort);

        // Mark All Members As In-Game
        foreach (MatchmakingGroup group in pendingMatch.GetAllGroups())
        {
            group.QueueStartTime = null;

            foreach (MatchmakingGroupMember member in group.Members)
                member.IsInGame = true;
        }

        Log.Information(@"Match Spawned: GUID={MatchGUID}, MatchID={MatchID}, Server={ServerAddress}:{ServerPort}",
            pendingMatch.GUID, requestData.MatchID, serverAddress, serverPort);
    }

    /// <summary>
    ///     Sends AutoMatchConnect (0x0062) to all players with server connection details.
    /// </summary>
    private static void SendAutoMatchConnect(MatchmakingMatch match, int matchID, string serverAddress, ushort serverPort)
    {
        byte arrangedMatchType = (byte)match.ArrangedMatchType;

        ChatBuffer connect = new ();

        connect.WriteCommand(ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT);
        connect.WriteInt8(arrangedMatchType);            // The Arranged Match Type (0 = None, 1 = Ranked, 2 = Tournament)
        connect.WriteInt32(matchID);                     // The Match Server's Match ID
        connect.WriteString(serverAddress);              // The Match Server's IP Address
        connect.WriteInt16(Convert.ToInt16(serverPort)); // The Match Server's Port

        // A Random Integer To Prevent Routers/Firewalls From Dropping Packets As Duplicates When Multiple Players Share The Same IP Address
        // A Value Of 0xFFFFFFFF (uint(-1) Which Wraps Around By Overflow) Indicates A Connection Reminder Rather Than The Initial Connection Request
        connect.WriteInt32(Random.Shared.Next());

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(connect);
    }
}

file class MatchAnnounceRequestData
{
    public MatchAnnounceRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        CorrelationID = buffer.ReadInt32();
        Challenge = buffer.ReadInt32();
        GroupCount = buffer.ReadInt32();
        MatchID = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The correlation ID that was sent to the match server in the create match request.
    ///     This is used to correlate the match announcement with the pending match that was created when the match was spawned.
    /// </summary>
    public int CorrelationID { get; init; }

    /// <summary>
    ///     The challenge (password) for the match.
    /// </summary>
    public int Challenge { get; init; }

    /// <summary>
    ///     The number of groups in the match.
    /// </summary>
    public int GroupCount { get; init; }

    /// <summary>
    ///     The match ID assigned by the match server.
    /// </summary>
    public int MatchID { get; init; }
}
