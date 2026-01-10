namespace KONGOR.MasterServer.Configuration.Matchmaking;

public class MatchConfiguration
{
    public required int MaximumPlayerRatingDifference { get; set; }

    public required bool IsRanked { get; set; }

    public required int TeamSize { get; set; }
}