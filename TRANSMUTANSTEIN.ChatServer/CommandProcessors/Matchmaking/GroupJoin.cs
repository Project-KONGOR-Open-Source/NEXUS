namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(MerrickContext merrick, ILogger<GroupJoin> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;

    private ILogger<GroupJoin> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(requestData.InviteIssuerName)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer Name ""{requestData.InviteIssuerName}""");

        MatchmakingGroupMember newMatchmakingGroupMember = new (session)
        {
            Slot = Convert.ToByte(group.Members.Count + 1),
            IsLeader = false,
            IsReady = true,
            IsInGame = false,
            IsEligibleForMatchmaking = true,
            LoadingPercent = 0,
            HasGameModeAccess = true,
            GameModeAccess = group.Leader.GameModeAccess
        };

        if (group.Members.Any(member => member.Account.ID == session.ClientInformation.Account.ID) is false)
        {
            group.Members.Add(newMatchmakingGroupMember);
        }

        else
        {
            Logger.LogWarning("Player {Session.ClientInformation.Account.Name} Tried To Join A Matchmaking Group They Are Already In", session.ClientInformation.Account.Name);

            return;
        }

        // TODO: Create Tentative Group, And Only Create Actual Group When Another Player Joins, Or Create Group As Is But Disband On Invite Refusal/Timeout
        group.MulticastUpdate(session.ClientInformation.Account.ID, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);
    }
}

public class GroupJoinRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();
    public string InviteIssuerName = buffer.ReadString();
}
