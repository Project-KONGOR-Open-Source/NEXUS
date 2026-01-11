namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GAME_OPTION_UPDATE)]
public class GroupGameOptionUpdate : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupGameOptionUpdateRequestData requestData = new(buffer);

        MatchmakingGroup? group;

        try
        {
            group = MatchmakingGroup.GetByMemberAccountID(session.Account.ID);
            Log.Information("GroupGameOptionUpdate: Group Found for Account {AccountID}: {GroupGUID}", session.Account.ID, group.GUID);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GroupGameOptionUpdate: Failed to get group for Account {AccountID}", session.Account.ID);
            return;
        }

        if (group is null)
        {
            Log.Warning("GroupGameOptionUpdate: Group is NULL for Account {AccountID}", session.Account.ID);
            return;
        }

        if (group.Leader.Session != session)
        {
             Log.Warning("GroupGameOptionUpdate: Session mismatch. Leader Session ID: {LeaderSessionID}, Current Session ID: {CurrentSessionID}", 
                group.Leader.Session.ID, session.ID);
             // Also check Account ID just in case
             Log.Warning("GroupGameOptionUpdate: Leader Account: {LeaderID}, Current Account: {CurrentID}", 
                group.Leader.Account.ID, session.Account.ID);

            return;
        }

        Log.Information("GroupGameOptionUpdate: Updating Information for Group {GroupGUID}", group.GUID);
        group.UpdateInformation(requestData.ToGroupInformation());
    }
}
