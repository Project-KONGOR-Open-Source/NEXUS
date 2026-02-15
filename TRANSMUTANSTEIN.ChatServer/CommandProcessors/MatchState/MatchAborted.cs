namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles the case when an arranged match is aborted (e.g. player left during loading, timeout, etc.).
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ABORTED)]
public class MatchAborted(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAbortedRequestData requestData = new (buffer);

        // Remove Match Information From Distributed Cache
        await distributedCacheStore.RemoveMatchInformation(requestData.MatchID);

        Log.Information(@"Match {MatchID} Aborted: Reason={Reason}", requestData.MatchID, requestData.Reason);

        // TODO: Notify Players That The Match Has Been Aborted
        // TODO: Return Players To Available State For Re-Queuing
        // TODO: Potentially Apply Leaver Penalties If Reason Indicates Player Left
    }
}

file class MatchAbortedRequestData
{
    public byte[] CommandBytes { get; init; }

    public int MatchID { get; init; }

    public ChatProtocol.MatchAbortedReason Reason { get; init; }

    public MatchAbortedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchID = buffer.ReadInt32();
        Reason = (ChatProtocol.MatchAbortedReason) buffer.ReadInt8();
    }
}
