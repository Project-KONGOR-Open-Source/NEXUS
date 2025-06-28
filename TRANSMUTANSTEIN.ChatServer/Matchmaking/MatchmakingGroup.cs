namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroup(MatchmakingGroupMember leader)
{
    public MatchmakingGroupMember Leader => Members.Single(member => member.IsLeader);

    public List<MatchmakingGroupMember> Members { get; set; } = [ leader ];

    // public float AverageRating => Members.Average(member => member.Rating);

    // public float RatingDisparity => Members.Max(member => member.Rating) - Members.Min(member => member.Rating);
}
