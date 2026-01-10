namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE)]
public class GroupInvite(MerrickContext merrick) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupInviteRequestData requestData = new(buffer);

        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .Invite(session, merrick, requestData.InviteReceiverName);
    }
}