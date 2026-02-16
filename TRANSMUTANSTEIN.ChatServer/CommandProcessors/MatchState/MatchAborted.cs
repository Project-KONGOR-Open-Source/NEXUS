namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles NET_CHAT_GS_MATCH_ABORTED (0x0509) from game servers.
///     This is sent only for arranged matches that were aborted (e.g. player left during loading, connection timeout).
///     C++ reference: <c>c_hostserver.cpp:3362-3366</c> — sent after <c>SendAbandonMatch</c>, only when <c>IsArrangedMatch()</c> and <c>bAborted</c>.
///     C++ reference: <c>c_serverchatconnection.cpp:605-613</c> — <c>SendMatchAborted(EMatchAbortedReason eReason)</c> sends <c>GetMatchupID()</c> + <c>byte(eReason)</c>.
///     C++ reference: <c>c_gameserver.cpp:857-909</c> — <c>ProcessMatchAborted</c> reads <c>ReadInt()</c> + <c>ReadByte()</c>.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ABORTED)]
public class MatchAborted(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAbortedRequestData requestData = new (buffer);

        // Remove Match Information From Distributed Cache
        await distributedCacheStore.RemoveMatchInformation(requestData.MatchupID);

        Log.Information(@"Arranged Match {MatchupID} Aborted On Server ID ""{ServerID}"": Reason={Reason}",
            requestData.MatchupID, session.Metadata.ServerID, requestData.Reason);

        // TODO: Check What MatchupID Is Or Whether It Can Just Be Called MatchID Instead

        // TODO: Notify Players That The Match Has Been Aborted
        // TODO: Return Players To Available State For Re-Queuing
        // TODO: Potentially Apply Leaver Penalties If Reason Indicates Player Left
    }
}

file class MatchAbortedRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The matchup ID assigned by the matchmaking system when the match was arranged.
    ///     This is the matchup/arrangement ID, not the match ID assigned when stats recording starts.
    ///     C++ reference: <c>c_serverchatconnection.cpp:611</c> — <c>m_pHostServer->GetMatchupID()</c>.
    ///     C++ reference: <c>c_gameserver.cpp:859</c> — <c>pkt.ReadInt()</c>.
    /// </summary>
    public int MatchupID { get; init; }

    /// <summary>
    ///     The reason the match was aborted.
    ///     C++ reference: <c>chatserver_protocol.h:472-480</c> — <c>EMatchAbortedReason</c>.
    ///     C++ reference: <c>c_gameserver.cpp:860</c> — <c>EMatchAbortedReason(pkt.ReadByte())</c>.
    /// </summary>
    public ChatProtocol.MatchAbortedReason Reason { get; init; }

    public MatchAbortedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
        Reason = (ChatProtocol.MatchAbortedReason) buffer.ReadInt8();
    }
}
