namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Handles a change to the group's type by the group leader, switching between a player-versus-player group and a co-operative (bot match) group.
///     The updated type is applied to the group and broadcast to all members.
/// </summary>
[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE)]
public class ChangeGroupType : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ChangeGroupTypeRequestData requestData = new (buffer);

        MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (group is null)
        {
            Log.Error("[BUG] No Matchmaking Group Found For Account ID {AccountID}", session.Account.ID);

            return;
        }

        // Only The Group Leader May Change The Group's Type
        if (group.Leader.Account.ID != session.Account.ID)
        {
            Log.Error("[BUG] Account ID {AccountID} Attempted To Change The Type Of Group {GroupGUID} But Is Not The Leader", session.Account.ID, group.GUID);

            return;
        }

        group.ChangeGroupType(requestData.GroupType);
    }
}

file class ChangeGroupTypeRequestData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.TMMType GroupType { get; init; }

    public ChangeGroupTypeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        GroupType = (ChatProtocol.TMMType) buffer.ReadInt8();
    }
}
