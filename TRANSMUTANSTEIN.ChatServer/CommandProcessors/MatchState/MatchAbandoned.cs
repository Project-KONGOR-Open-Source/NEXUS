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

        // The Protocol Does Not Carry A Match ID, So We Use The Session Metadata Which Is Populated By NET_CHAT_GS_STATUS.
        // A Value Of -1 Means No Match Was Ever Announced (e.g. An Abandonment Fired Before NET_CHAT_GS_ANNOUNCE_MATCH), So There Is Nothing To Clean Up
        if (requestData.Failed && session.Metadata.MatchID is not -1)
        {
            /*
                Match Information Is Only Removed From The Distributed Cache Here If The Match Failed To Start
                For Normal Match Endings, The Match Server Submits Statistics To The Master Server After This Message, And The Client Requests Match Statistics Shortly After
                Both The Stat Submission And The Match Stats Response Require The Cached Match Information
            */

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
