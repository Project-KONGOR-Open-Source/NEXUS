namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new (buffer);

        MatchmakingGroup
            .GetByMemberAccountName(requestData.InviteIssuerName)
            .Join(session);
    }
}

public class GroupJoinRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string ClientVersion = buffer.ReadString();

    public string InviteIssuerName = buffer.ReadString();
}
