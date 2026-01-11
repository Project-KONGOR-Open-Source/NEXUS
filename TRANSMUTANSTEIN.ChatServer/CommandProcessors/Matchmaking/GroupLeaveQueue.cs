namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE)]
public class GroupLeaveQueue : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupLeaveQueueRequestData requestData = new(buffer);

        MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (group is null)
        {
            return;
        }

        // Remove Group From Queue
        group.LeaveQueue(session.Account.ID);

        // Send Full Group Update To Reflect New Player States
        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
    }
}