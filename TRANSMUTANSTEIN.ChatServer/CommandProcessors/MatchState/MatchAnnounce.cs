namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles NET_CHAT_GS_ANNOUNCE_MATCH (0x0503) from game servers.
///     This is sent by the game server after it has successfully set up the match.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH)]
public class MatchAnnounce(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        // Read The Command
        _ = buffer.ReadCommandBytes();

        // Read The Announce Match Data
        int correlationID = buffer.ReadInt32();  // Correlation ID We Sent In CreateMatch
        int challenge = buffer.ReadInt32();      // Challenge/Password
        int groupCount = buffer.ReadInt32();     // Number Of Groups
        int matchID = buffer.ReadInt32();        // The Match ID Assigned By The Game Server

        Log.Information(
            @"Match Announce Received: ServerID={ServerID}, CorrelationID={CorrelationID}, MatchID={MatchID}",
            session.Metadata.ServerID,
            correlationID,
            matchID);

        // Find The Pending Match By Correlation ID
        if (MatchmakingService.ActiveMatches.Values.SingleOrDefault(match => match.CorrelationID == correlationID) is not { } pendingMatch)
        {
            Log.Warning(@"Match Announce Received For Unknown CorrelationID: {CorrelationID}", correlationID);

            return;
        }

        // Update Match State
        pendingMatch.State = MatchmakingMatchState.ServerAllocated;

        // Use The Server Address And Port That Were Set When The Match Was Spawned
        // These Come From The MatchServer Cache Entry And Represent The Game Server's Address
        string serverAddress = pendingMatch.ServerAddress ?? "127.0.0.1";
        ushort serverPort = pendingMatch.ServerPort ?? 11235;

        // If The Match Doesn't Have The Server Address, Try To Get It From The Session's Remote Endpoint
        if (string.IsNullOrEmpty(pendingMatch.ServerAddress) && session.Socket.RemoteEndPoint is IPEndPoint remoteEndPoint)
            serverAddress = remoteEndPoint.Address.ToString();

        // Create MatchInformation Now That We Have The Real Match ID From The Game Server
        MatchServer? server = await distributedCacheStore.GetMatchServerByID(pendingMatch.AssignedServerID ?? 0);

        if (server is not null)
        {
            MatchInformation matchInformation = MatchmakingService.CreateMatchInformation(pendingMatch, server, matchID);

            await distributedCacheStore.SetMatchInformation(matchInformation);

            Log.Information(@"MatchInformation Cached: MatchID={MatchID}, MatchName={MatchName}", matchID, matchInformation.MatchName);
        }
        else
        {
            Log.Warning(@"Could Not Create MatchInformation: Server Not Found For ServerID {ServerID}", pendingMatch.AssignedServerID);
        }

        // Send AutoMatchConnect To All Players (C++ AnnounceMatchReady)
        // MatchFoundUpdate And FoundServerUpdate Were Already Sent From SpawnMatch
        SendAutoMatchConnect(pendingMatch, matchID, serverAddress, serverPort);

        // Mark All Members As In-Game
        foreach (MatchmakingGroup group in pendingMatch.GetAllGroups())
        {
            group.QueueStartTime = null;

            foreach (MatchmakingGroupMember member in group.Members)
                member.IsInGame = true;
        }

        Log.Information(
            @"Match Spawned: GUID={MatchGUID}, MatchID={MatchID}, Server={ServerAddress}:{ServerPort}",
            pendingMatch.GUID,
            matchID,
            serverAddress,
            serverPort);
    }

    /// <summary>
    ///     Sends AutoMatchConnect (0x0062) to all players with server connection details.
    /// </summary>
    private static void SendAutoMatchConnect(MatchmakingMatch match, int matchID, string serverAddress, ushort serverPort)
    {
        byte arrangedMatchType = (byte)match.ArrangedMatchType;

        ChatBuffer connect = new();

        connect.WriteCommand(ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT);
        connect.WriteInt8(arrangedMatchType);
        connect.WriteInt32(matchID);            // The Game Server's Match ID (Used By Client In CHAT_CMD_JOINED_GAME)
        connect.WriteString(serverAddress);
        connect.WriteInt16(Convert.ToInt16(serverPort));
        connect.WriteInt32(Random.Shared.Next());

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(connect);
    }
}
