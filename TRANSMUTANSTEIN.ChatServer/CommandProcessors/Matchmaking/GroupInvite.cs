namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE)]
public class GroupInvite(MerrickContext merrick) : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupInviteRequestData requestData = new (buffer);
        
        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .Invite(session, merrick, requestData.InviteReceiverName);
    }
}

public class GroupInviteRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string InviteReceiverName = buffer.ReadString();
}
