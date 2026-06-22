namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles match abortion from game servers.
///     This is sent only for arranged matches that were aborted (e.g. player left during loading, connection timeout).
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ABORTED)]
public class MatchAborted(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAbortedRequestData requestData = new (buffer);

        // Remove Match Information From Distributed Cache
        await distributedCacheStore.RemoveMatchInformation(requestData.MatchupID);

        Log.Information(@"Arranged Match {MatchupID} Aborted On Server ID ""{MatchServerID}"": Reason={Reason}",
            requestData.MatchupID, session.Metadata.ServerID, requestData.Reason);

        // TODO: Check What MatchupID Is Or Whether It Can Just Be Called MatchID Instead (They Are Potentially Different Things, Though)

        // Make The Aborted Match's Groups Available To Queue Again And Drop The Match From The Active Matches Registry
        // This Is Harmless If The Subsequent MatchAbandoned "Server Reset" Signal Has Already Cleaned The Match Up
        MatchmakingService.CleanUpMatchesForServer(session.Metadata.ServerID);

        // TODO: Apply Leaver Penalties (Not Yet Implemented)
        //
        // There Are Two Distinct Leaver Signals From The Game Server:
        //   1. This Match-Aborted "Reason" Field, Which Can Indicate That A Player Left During Loading Or Timed Out
        //   2. The Separate NET_CHAT_GS_REPORT_LEAVER (0x0518) Command, Which Carries A Single Int32 (The Leaver's Account ID) And Is Sent To Flag An In-Match Leaver
        //
        // In The Original HoN Chat Server, NET_CHAT_GS_REPORT_LEAVER Bans The Reported Account From Team Matchmaking
        // NEXUS Has No Matchmaking Penalty Subsystem Yet, So Neither Signal Currently Results In A Penalty, And 0x0518 Is Deliberately Left Unmapped (It Logs A "Missing Type Mapping" Error Until This Feature Is Built; Adding A Do-Nothing Handler Would Be A Stub)
        //
        // A Minimal Implementation Would:
        //   - Record A Per-Account Leaver Ban As A Distributed-Cache Entry With An Expiry (The Ban Duration And Any Escalation Is A Policy Decision)
        //   - Check For An Active Ban On The Matchmaking Group Queue-Join Path And Reject Or Hold The Affected Player
        //   - Add A NET_CHAT_GS_REPORT_LEAVER Command Processor That Reads The Int32 Account ID And Records The Ban
    }
}

file class MatchAbortedRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The matchup ID assigned by the matchmaking system when the match was arranged.
    ///     This is the matchup/arrangement ID, not the match ID assigned when stats recording starts.
    /// </summary>
    public int MatchupID { get; init; }

    /// <summary>
    ///     The reason the match was aborted.
    /// </summary>
    public ChatProtocol.MatchAbortedReason Reason { get; init; }

    public MatchAbortedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
        Reason = (ChatProtocol.MatchAbortedReason) buffer.ReadInt8();
    }
}
