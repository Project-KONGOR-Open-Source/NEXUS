using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.Services;

public interface IMatchmakingService : IHostedService
{
    ConcurrentDictionary<int, MatchmakingGroup> Groups { get; }
    
    MatchmakingGroup? GetMatchmakingGroup(OneOf<int, string> memberIdentifier);
    MatchmakingGroup? GetMatchmakingGroupByMemberID(int memberID);
    MatchmakingGroup? GetMatchmakingGroupByMemberName(string memberName);
}
