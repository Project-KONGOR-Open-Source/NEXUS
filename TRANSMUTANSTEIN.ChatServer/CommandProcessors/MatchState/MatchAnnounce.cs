namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles NET_CHAT_GS_ANNOUNCE_MATCH (0x0503) from game servers.
///     This is sent by the game server after it has successfully set up the match.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH)]
public class MatchAnnounce : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        // Read The Command
        _ = buffer.ReadCommandBytes();

        // Read The Announce Match Data
        int matchup = buffer.ReadInt32();      // Matchup/Challenge ID We Sent In CreateMatch
        int challenge = buffer.ReadInt32();    // Challenge/Password
        int groupCount = buffer.ReadInt32();   // Number Of Groups
        int matchID = buffer.ReadInt32();       // The Match ID Assigned By The Game Server

        Log.Information(
            @"Match Announce Received: ServerID={ServerID}, Matchup={Matchup}, MatchID={MatchID}",
            session.Metadata.ServerID,
            matchup,
            matchID);

        // Find The Pending Match By Matchup Index
        if (MatchmakingService.ActiveMatches.Values.SingleOrDefault(match => match.MatchIndex == matchup) is not { } pendingMatch)
        {
            Log.Warning(@"Match Announce Received For Unknown Matchup: {Matchup}", matchup);

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

        // Send Notifications To All Players
        SendMatchFoundUpdate(pendingMatch);
        SendFoundServerUpdate(pendingMatch);
        SendAutoMatchConnect(pendingMatch, serverAddress, serverPort);

        // Mark All Members As In-Game
        foreach (MatchmakingGroup group in pendingMatch.GetAllGroups())
        {
            group.QueueStartTime = null;

            foreach (MatchmakingGroupMember member in group.Members)
                member.IsInGame = true;
        }

        Log.Information(
            @"Match Spawned: MatchIndex={MatchIndex}, MatchID={MatchID}, Server={ServerAddress}:{ServerPort}",
            pendingMatch.MatchIndex,
            matchID,
            serverAddress,
            serverPort);
    }

    /// <summary>
    ///     Sends MatchFoundUpdate (0x0D09) to all players in a match.
    /// </summary>
    private static void SendMatchFoundUpdate(MatchmakingMatch match)
    {
        ChatBuffer matchFound = new();

        matchFound.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE);
        matchFound.WriteString(match.SelectedMap);
        matchFound.WriteInt8(Convert.ToByte(match.LegionTeam.TeamSize));
        matchFound.WriteInt8(Convert.ToByte(match.GameType));
        matchFound.WriteString(match.SelectedMode);
        matchFound.WriteString(match.SelectedRegion);
        matchFound.WriteString($"Match #{match.MatchIndex}");

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
    private static void SendAutoMatchConnect(MatchmakingMatch match, string serverAddress, ushort serverPort)
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
        connect.WriteInt32(match.MatchIndex);   // Matchup ID (Our Match Index, Not The Game Server's Match ID)
        connect.WriteString(serverAddress);
        connect.WriteInt16(Convert.ToInt16(serverPort));
        connect.WriteInt32(Random.Shared.Next());

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(connect);
    }
}
