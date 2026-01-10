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

        // Validate That The Group Is Actually Queued
        if (group.QueueStartTime is null)
        {
            return;
        }

        // Remove Group From Queue
        group.QueueStartTime = null;

        // Unready The Group Leader And Unload All Members
        // Non-Leader Members Should Always Be Ready So That Group Readiness Is Determined Solely By The Leader
        foreach (MatchmakingGroupMember member in group.Members)
        {
            member.IsReady = member.IsLeader is false;
            member.LoadingPercent = 0;
        }

        // Broadcast Leave Queue To All Group Members
        ChatBuffer leaveQueueBroadcast = new();

        leaveQueueBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE);

        foreach (MatchmakingGroupMember member in group.Members)
        {
            member.Session.Send(leaveQueueBroadcast);
        }

        // Send Full Group Update To Reflect New Player States
        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
    }
}

file class GroupLeaveQueueRequestData
{
    public GroupLeaveQueueRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}