namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

/// <summary>
///     Handles match abandonment from game servers.
///     This is a general "game server reset" signal sent by the game server every time a match ends, regardless of whether the match completed normally or failed to start.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ABANDON_MATCH)]
public class MatchAbandoned(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchAbandonedRequestData requestData = new (buffer);

        Log.Information(@"Match Abandoned On Server ID ""{ServerID}"": Failed={Failed}",
            session.Metadata.ServerID, requestData.Failed);

        // The protocol does not carry a match ID; use the session metadata which is populated by NET_CHAT_GS_STATUS.
        // A value of -1 means no match was ever announced (e.g. the abandon fired before NET_CHAT_GS_ANNOUNCE_MATCH), so there is nothing to clean up.
        if (session.Metadata.MatchID is not -1)
        {
            // Remove Match Information From Distributed Cache.
            await distributedCacheStore.RemoveMatchInformation(session.Metadata.MatchID);
        }

        // TODO: Find The Active Match For This Server And Clean It Up
        // TODO: Notify Players That The Match Has Been Abandoned
        // TODO: Return Players To Available State For Re-Queuing
    }
}

file class MatchAbandonedRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     Whether the match failed to start.
    ///     This is a boolean (not an enum); TRUE means the game failed to start, FALSE means a normal game reset.
    /// </summary>
    public bool Failed { get; init; }

    public MatchAbandonedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Failed = buffer.ReadBool();
    }
}
