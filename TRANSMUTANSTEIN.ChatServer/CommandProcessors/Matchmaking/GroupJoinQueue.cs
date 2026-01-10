namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Handles explicit queue join requests from the group leader.
///     This path complements <see cref="GroupPlayerLoadingStatus" /> which auto-joins the queue when all players reach
///     100% loading status.
///     Both paths validate the same conditions.
/// </summary>
[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE)]
public class GroupJoinQueue : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinQueueRequestData requestData = new(buffer);

        MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (group is null)
        {
            Log.Error("[BUG] No Matchmaking Group Found For Account ID {AccountID}", session.Account.ID);

            return;
        }

        // Validate That The Requesting Player Is The Group Leader
        if (group.Leader.Account.ID != session.Account.ID)
        {
            Log.Error(
                "[BUG] Account ID {AccountID} Attempted To Join Queue For Group {GroupGUID} But Is Not The Leader",
                session.Account.ID, group.GUID);

            return;
        }

        // Call The Group's Queue Join Method Which Validates All Conditions
        group.JoinQueue();
    }
}

file class GroupJoinQueueRequestData
{
    public GroupJoinQueueRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}