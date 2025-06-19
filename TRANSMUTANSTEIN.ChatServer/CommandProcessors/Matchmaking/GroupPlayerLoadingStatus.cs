namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus(MerrickContext merrick, ILogger<GroupPlayerLoadingStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupPlayerLoadingStatus> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerLoadingStatusRequestData requestData = new(buffer);

        // Find the player's current group across all group sizes
        MatchmakingGroup? group = null;
        
        group ??= MatchmakingService.SoloPlayerGroups.Values.FirstOrDefault(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
        group ??= MatchmakingService.TwoPlayerGroups.Values.FirstOrDefault(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
        group ??= MatchmakingService.ThreePlayerGroups.Values.FirstOrDefault(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
        group ??= MatchmakingService.FourPlayerGroups.Values.FirstOrDefault(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
        group ??= MatchmakingService.FivePlayerGroups.Values.FirstOrDefault(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));

        if (group != null)
        {
            group.NotifyLoadingStatusChanged(session.ClientInformation.Account.ID, requestData.LoadingStatus);
        }
    }
}

public class GroupPlayerLoadingStatusRequestData(ChatBuffer buffer)
{
    public byte LoadingStatus = buffer.ReadInt8();
}
