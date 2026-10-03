namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles the match-started signal from game servers.
///     The game server sends this when an arranged match has started successfully (all players have joined and the banning/picking phase is beginning).
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_STARTED)]
public class MatchStarted : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchStartedRequestData requestData = new (buffer);

        Log.Information(@"Arranged Match Started On Server ID ""{MatchServerID}"": MatchupID={MatchupID}",
            session.Metadata.ServerID, requestData.MatchupID);
    }
}

file class MatchStartedRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The matchup ID of the arranged match that has started, assigned by the matchmaking system.
    /// </summary>
    public int MatchupID { get; init; }

    public MatchStartedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
    }
}
