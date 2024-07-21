namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroupMember
{
    public required float Rating { get; set; }

    public required bool IsLeader { get; set; }

    public required bool IsReady { get; set; }
}
