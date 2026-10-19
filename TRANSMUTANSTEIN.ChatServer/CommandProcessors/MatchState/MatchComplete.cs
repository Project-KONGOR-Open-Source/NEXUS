namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ENDED)]
public class MatchComplete(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchCompleteRequestData requestData = new (buffer);

        Log.Information(@"Arranged Match {MatchupID} Ended On Server ID ""{MatchServerID}"": Reason={Reason}, WinningTeam={WinningTeam}, PlayerCount={PlayerCount}",
            requestData.MatchupID, session.Metadata.ServerID, requestData.Reason, requestData.WinningTeam, requestData.PlayerCount);

        // The Match Server Names The Ended Match Only By Its Matchup ID, So Only The Locks Whose Correlation ID Matches It Are Lifted
        await distributedCacheStore.RemoveMatchLocksForServer(session.Metadata.ServerID, matchLock => matchLock.CorrelationID == requestData.MatchupID);

        /*
            Match Information Is Not Removed From The Distributed Cache Here
            The Match Server Submits Statistics To The Master Server After This Message, And The Client Requests Match Statistics Shortly After
            Both The Stat Submission And The Match Stats Response Require The Cached Match Information
        */

        // TODO: Update Player Availability States (Mark Players As Available After Match Ends)
        // TODO: Mark Server As Available For New Match Allocation
        // TODO: Notify Players That Match Has Ended
        // TODO: Clean Up Match-Related Session State
    }
}

file class MatchCompleteRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The matchup ID of the arranged match, which is the correlation ID that NEXUS sent to the match server in NET_CHAT_GS_CREATE_MATCH and the match server echoes back.
    ///     This is not the match ID assigned when stats recording starts.
    /// </summary>
    public int MatchupID { get; init; }

    public ChatProtocol.MatchEndedReason Reason { get; init; }

    public byte WinningTeam { get; init; }

    public byte PlayerCount { get; init; }

    public int[] AccountIDs { get; init; }

    public MatchCompleteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
        Reason = (ChatProtocol.MatchEndedReason) buffer.ReadInt8();
        WinningTeam = buffer.ReadInt8();
        PlayerCount = buffer.ReadInt8();
        AccountIDs = new int[PlayerCount];

        for (int i = 0; i < PlayerCount; i++)
        {
            AccountIDs[i] = buffer.ReadInt32();
        }
    }
}
