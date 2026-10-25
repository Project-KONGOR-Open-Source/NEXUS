namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles match announcements from game servers.
///     This is sent by the game server after it has successfully set up the match.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH)]
public class MatchAnnounce(IDatabase distributedCacheStore, MatchLockService matchLockService) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAnnounceRequestData requestData = new (buffer);

        Log.Information(@"Match Announce Received: ServerID={MatchServerID}, CorrelationID={CorrelationID}, MatchID={MatchID}",
            session.Metadata.ServerID, requestData.CorrelationID, requestData.MatchID);

        // Find The Pending Match By Correlation ID
        if (MatchmakingService.ActiveMatches.Values.SingleOrDefault(match => match.CorrelationID == requestData.CorrelationID) is not { } pendingMatch)
        {
            Log.Warning(@"Match Announce Received For Unknown CorrelationID: {CorrelationID}", requestData.CorrelationID);

            return;
        }

        // Update Match State
        pendingMatch.State = MatchmakingMatchState.ServerAllocated;

        // Use The Server Address That Was Set When The Match Was Spawned (From The MatchServer Cache Entry), Falling Back To The Session's Remote Endpoint When It Is Missing
        string? serverAddress = pendingMatch.ServerAddress;

        if (string.IsNullOrEmpty(serverAddress) && session.RemoteEndPoint is IPEndPoint remoteEndPoint)
            serverAddress = remoteEndPoint.Address.ToString();

        if (string.IsNullOrEmpty(serverAddress))
            throw new NullReferenceException("Pending Match Server Address Is NULL");

        ushort serverPort = pendingMatch.ServerPort ?? throw new NullReferenceException("Pending Match Server Port Is NULL");

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
            Log.Warning(@"Could Not Create MatchInformation: Server Not Found For ServerID {MatchServerID}", pendingMatch.AssignedServerID);
        }

        // Send AutoMatchConnect To All Players
        // MatchFoundUpdate And FoundServerUpdate Were Already Sent From SpawnMatch
        foreach (MatchmakingGroupMember member in pendingMatch.GetAllPlayers())
            member.Session.SendAutoMatchConnect(pendingMatch.ArrangedMatchType, requestData.MatchID, serverAddress, serverPort, reminder: false);

        // Mark All Members As In-Game
        foreach (MatchmakingGroup group in pendingMatch.GetAllGroups())
        {
            group.QueueStartTime = null;

            foreach (MatchmakingGroupMember member in group.Members)
                member.IsInGame = true;
        }

        Log.Information(@"Match Spawned: GUID={MatchGUID}, MatchID={MatchID}, Server={MatchServerAddress}:{MatchServerPort}",
            pendingMatch.GUID, requestData.MatchID, serverAddress, serverPort);

        // The Players Are Locked Last, So That A Failure To Write A Lock Cannot Disturb The Announcement
        // The Locks Hold The Address And Port Which The Players Were Sent, So That A Player Is Sent Back Exactly Where They First Connected
        await matchLockService.LockMatchRoster(pendingMatch, requestData.MatchID, session.Metadata.ServerID, serverAddress, serverPort);
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
