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

        Log.Information(@"Match Abandoned On Server ID ""{MatchServerID}"": Failed={Failed}",
            session.Metadata.ServerID, requestData.Failed);

        // The Protocol Does Not Carry A Match ID, So We Use The Session Metadata Which Is Populated By NET_CHAT_GS_STATUS.
        // A Value Of -1 Means No Match Was Ever Announced (e.g. An Abandonment Fired Before NET_CHAT_GS_ANNOUNCE_MATCH), So There Is No Cached Match Information To Remove
        if (requestData.Failed && session.Metadata.MatchID is not -1)
        {
            /*
                Match Information Is Only Removed From The Distributed Cache Here If The Match Failed To Start
                For Normal Match Endings, The Match Server Submits Statistics To The Master Server After This Message, And The Client Requests Match Statistics Shortly After
                Both The Stat Submission And The Match Stats Response Require The Cached Match Information
            */

            await distributedCacheStore.RemoveMatchInformation(session.Metadata.MatchID);
        }

        // This Signal Fires Whenever The Match Server Resets (Both A Normal Ending And A Failed Start)
        // So It Is The Single Point At Which A Match Is Dropped From The Active Matches Registry And Its Groups Are Made Available To Queue Again
        MatchmakingService.CleanUpMatchesForServer(session.Metadata.ServerID);
    }
}

file class MatchAbandonedRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     Whether the match failed to start.
    ///     This is a boolean (not an enum); <see langword="true"/> means the game failed to start, <see langword="false"/> means a normal game reset.
    /// </summary>
    public bool Failed { get; init; }

    public MatchAbandonedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Failed = buffer.ReadBool();
    }
}
