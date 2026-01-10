namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new(buffer);

        MatchmakingGroup
            .GetByMemberAccountName(requestData.InviteIssuerName)
            .Join(session);
    }
}

file class GroupJoinRequestData
{
    public GroupJoinRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ClientVersion = buffer.ReadString();
        InviteIssuerName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ClientVersion { get; init; }

    public string InviteIssuerName { get; }
}