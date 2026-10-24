namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(MerrickContext merrick, MatchLockService matchLockService) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new (buffer);

        // The Group Is Measured Before Enforcing, Because Its Members Can Change While The Lock Is Checked
        bool soloGroup = MatchmakingService.GetMatchmakingGroup(session.Account.ID)?.Members.Count is 1;

        if (await matchLockService.Enforce(session, matchmaking: true, soloGroup: soloGroup))
            return;

        await MatchmakingGroup
            .GetByMemberAccountName(requestData.InviteIssuerName)
            .Join(session, merrick);
    }
}

file class GroupJoinRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ClientVersion { get; init; }

    public string InviteIssuerName { get; init; }

    public GroupJoinRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ClientVersion = buffer.ReadString();
        InviteIssuerName = buffer.ReadString();
    }
}
