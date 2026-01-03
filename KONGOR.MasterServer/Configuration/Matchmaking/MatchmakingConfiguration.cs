namespace KONGOR.MasterServer.Configuration.Matchmaking;

public class MatchmakingConfiguration
{
    public required MatchmakingMapConfiguration Ranked { get; set; }

    public required MatchmakingMapConfiguration Unranked { get; set; }

    public required MatchmakingMapConfiguration MidWars { get; set; }

    public required MatchmakingMapConfiguration RiftWars { get; set; }
}

public class MatchmakingMapConfiguration
{
    public required string Map { get; set; }

    public required string[] Regions { get; set; }

    public required string[] Modes { get; set; }

    public required MatchConfiguration Match { get; set; }
}

public class MatchConfiguration
{
    public required int MaximumPlayerRatingDifference { get; set; }

    public required bool IsRanked { get; set; }

    public required int TeamSize { get; set; }
}
