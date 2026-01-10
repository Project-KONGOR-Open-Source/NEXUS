namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE)]
public class GroupLeave : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupLeaveRequestData requestData = new (buffer);

        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .RemoveMember(session.Account.ID);
    }
}

file class GroupLeaveRequestData
{
    public byte[] CommandBytes { get; init; }

    public GroupLeaveRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}

