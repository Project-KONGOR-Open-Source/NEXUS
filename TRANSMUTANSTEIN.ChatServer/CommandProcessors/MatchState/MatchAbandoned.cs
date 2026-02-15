namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles the case when an arranged match fails to start (e.g. players didn't connect in time).
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ABANDON_MATCH)]
public class MatchAbandoned(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAbandonedRequestData requestData = new (buffer);

        // Remove Match Information From Distributed Cache
        await distributedCacheStore.RemoveMatchInformation(requestData.MatchID);

        Log.Information(@"Match {MatchID} Abandoned: Match Failed To Start", requestData.MatchID);

        // TODO: Notify Players That The Match Has Been Abandoned
        // TODO: Return Players To Available State For Re-Queuing
    }
}

file class MatchAbandonedRequestData
{
    public byte[] CommandBytes { get; init; }

    public int MatchID { get; init; }

    public MatchAbandonedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchID = buffer.ReadInt32();
    }
}
