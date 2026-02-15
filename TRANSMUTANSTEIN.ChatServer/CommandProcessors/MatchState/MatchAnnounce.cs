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

        // Send Notifications To All Players
        SendMatchFoundUpdate(pendingMatch, matchID);
        SendFoundServerUpdate(pendingMatch);
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
    ///     Sends MatchFoundUpdate (0x0D09) to all players in a match.
    /// </summary>
    private static void SendMatchFoundUpdate(MatchmakingMatch match, int matchID)
    {
        ChatBuffer matchFound = new();

        matchFound.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE);
        matchFound.WriteString(match.SelectedMap);
        matchFound.WriteInt8(Convert.ToByte(match.LegionTeam.TeamSize));
        matchFound.WriteInt8(Convert.ToByte(match.GameType));
        matchFound.WriteString(match.SelectedMode);
        matchFound.WriteString(match.SelectedRegion);
        matchFound.WriteString($"Match #{matchID}");

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(matchFound);
    }

    /// <summary>
    ///     Sends GroupQueueUpdate type=16 (TMM_GROUP_FOUND_SERVER) to all players.
    /// </summary>
    private static void SendFoundServerUpdate(MatchmakingMatch match)
    {
        ChatBuffer found = new();

        found.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        found.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_FOUND_SERVER));

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(found);
    }

    /// <summary>
    ///     Sends AutoMatchConnect (0x0062) to all players with server connection details.
    /// </summary>
    private static void SendAutoMatchConnect(MatchmakingMatch match, int matchID, string serverAddress, ushort serverPort)
    {
        byte arrangedMatchType = match.GameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL          => (byte)MatchType.AM_MATCHMAKING,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL          => (byte)MatchType.AM_UNRANKED_MATCHMAKING,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS         => (byte)MatchType.AM_MATCHMAKING_MIDWARS,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS        => (byte)MatchType.AM_MATCHMAKING_RIFTWARS,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => (byte)MatchType.AM_MATCHMAKING_CAMPAIGN,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => (byte)MatchType.AM_MATCHMAKING_CAMPAIGN,
            _                                                      => (byte)MatchType.AM_MATCHMAKING
        };

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
