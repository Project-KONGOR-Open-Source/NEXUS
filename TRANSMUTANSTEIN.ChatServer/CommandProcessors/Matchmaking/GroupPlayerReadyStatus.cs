namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus(ILogger<GroupPlayerReadyStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerReadyStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerReadyStatusRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(session.ClientInformation.Account.ID)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer ID ""{session.ClientInformation.Account.ID}""");

        group.Information.GameType = requestData.GameType;

        MatchmakingGroupMember groupMember = group.Members.Single(member => member.Account.ID == session.ClientInformation.Account.ID);

        if (groupMember.IsLeader is false)
            return; // Non-Leader Group Members Are Implicitly Ready (By Means Of Joining The Group In A Ready State) And Do Not Need To Emit Readiness Status Updates

        if (groupMember.IsReady is false)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                if (member.IsReady is false)
                {
                    if (member.IsLeader is false)
                        Logger.LogError(@"[BUG] Non-Leader Group Member ""{Member.Account.Name}"" With ID ""{Member.Account.ID}"" Was Not Ready", member.Account.Name, member.Account.ID);

                    member.IsReady = true;
                }
            }

            group.MulticastUpdate(session.ClientInformation.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);
        }

        if (group.Members.All(member => member.IsReady))
        {
            ChatBuffer load = new ();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_START_LOADING);
            load.PrependBufferSize();

            Parallel.ForEach(group.Members, member => member.Session.SendAsync(load.Data));
        }
    }
}

public class GroupPlayerReadyStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public byte ReadyStatus = buffer.ReadInt8();

    public ChatProtocol.TMMGameType GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
}
