namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE)]
public class GroupLeave : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupLeaveRequestData requestData = new (buffer);

        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .RemoveMember(session.Account.ID);
    }
}

public class GroupLeaveRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
}
