namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE)]
public class GroupJoinQueue(MerrickContext merrick, ILogger<GroupJoinQueue> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupJoinQueue> Logger { get; set; } = logger;    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinQueueRequestData requestData = new(buffer);

        // Find the player's current group
        ConcurrentDictionary<int, MatchmakingGroup>? playerGroupDict = FindPlayerGroup(session.ClientInformation.Account.ID);

        if (playerGroupDict != null)
        {
            var group = playerGroupDict.Values.FirstOrDefault(g => g.Participants.Any(p => p.AccountId == session.ClientInformation.Account.ID));
            
            if (group != null)
            {
                if (requestData.ReadyStatus == 0)
                {
                    // Leave the queue
                    group.LeaveQueue();
                }
                else
                {
                    // Join the queue if leader
                    if (group.Leader?.AccountId == session.ClientInformation.Account.ID)
                    {
                        group.EnterQueue();
                    }
                }
            }
        }
    }

    private static ConcurrentDictionary<int, MatchmakingGroup>? FindPlayerGroup(int accountId)
    {
        if (MatchmakingService.SoloPlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == accountId)))
            return MatchmakingService.SoloPlayerGroups;
        
        if (MatchmakingService.TwoPlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == accountId)))
            return MatchmakingService.TwoPlayerGroups;
        
        if (MatchmakingService.ThreePlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == accountId)))
            return MatchmakingService.ThreePlayerGroups;
        
        if (MatchmakingService.FourPlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == accountId)))
            return MatchmakingService.FourPlayerGroups;
        
        if (MatchmakingService.FivePlayerGroups.Values.Any(g => g.Participants.Any(p => p.AccountId == accountId)))
            return MatchmakingService.FivePlayerGroups;
        
        return null;
    }
}

public class GroupJoinQueueRequestData(ChatBuffer buffer)
{
    public byte ReadyStatus = buffer.ReadInt8();
    public byte GameType = buffer.ReadInt8();
}
