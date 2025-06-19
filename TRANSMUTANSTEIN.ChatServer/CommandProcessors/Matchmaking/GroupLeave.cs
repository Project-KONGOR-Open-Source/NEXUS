namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE)]
public class GroupLeave(MerrickContext merrick, ILogger<GroupLeave> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupLeave> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        // Find and remove the player from their current group
        MatchmakingGroup? group = null;
        ConcurrentDictionary<int, MatchmakingGroup>? groupDict = null;

        // Check all group dictionaries
        if (MatchmakingService.SoloPlayerGroups.TryGetValue(session.ClientInformation.Account.ID, out group))
        {
            groupDict = MatchmakingService.SoloPlayerGroups;
        }
        else if (MatchmakingService.TwoPlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID)))
        {
            group = MatchmakingService.TwoPlayerGroups.Values.First(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
            groupDict = MatchmakingService.TwoPlayerGroups;
        }
        else if (MatchmakingService.ThreePlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID)))
        {
            group = MatchmakingService.ThreePlayerGroups.Values.First(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
            groupDict = MatchmakingService.ThreePlayerGroups;
        }
        else if (MatchmakingService.FourPlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID)))
        {
            group = MatchmakingService.FourPlayerGroups.Values.First(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
            groupDict = MatchmakingService.FourPlayerGroups;
        }
        else if (MatchmakingService.FivePlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID)))
        {
            group = MatchmakingService.FivePlayerGroups.Values.First(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
            groupDict = MatchmakingService.FivePlayerGroups;
        }

        if (group != null && groupDict != null)
        {
            group.RemoveParticipant(session);
            
            // If group is now empty or the participant was the leader, remove the group
            if (group.ParticipantCount == 0 || (group.Leader?.AccountId == session.ClientInformation.Account.ID))
            {
                groupDict.TryRemove(group.Leader?.AccountId ?? session.ClientInformation.Account.ID, out _);
            }
        }
    }
}
