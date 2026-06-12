namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE)]
public class GroupLeaveQueue : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupLeaveQueueRequestData requestData = new (buffer);

        MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (group is null)
            return;

        // Leave The Queue; The Request Is Refused When The Group Is Not Queued Or Is Already Committed To A Match
        if (group.LeaveQueue() is false)
            return;

        // Unready The Group Leader And Unload All Members, So That Group Readiness Is Again Determined Solely By The Leader
        group.UnloadAndUnreadyMembers();

        // Send Full Group Update To Reflect New Player States
        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
    }
}

file class GroupLeaveQueueRequestData
{
    public byte[] CommandBytes { get; init; }

    public GroupLeaveQueueRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
