namespace KINESIS.Matchmaking;

public class MatchmakingConfiguration
{
    public MatchmakingMapConfiguration Caldavar { get; set; } = new();

    public MatchmakingMapConfiguration MidWars { get; set; } = new();
}

public class MatchmakingMapConfiguration
{
    public string[] Regions { get; set; } = { };
    public string[] Modes { get; set; } = { };
    public RankedMatchConfiguration Match { get; set; } = new();
}

public class RankedMatchConfiguration
{
    public int ExcellentTeamDisparity { get; set; } = default;
    public int StrictMaximumTeamDisparity { get; set; } = default;
    public int RelaxedMaximumTeamDisparity { get; set; } = default;
    public int MaximumPlayerRatingDifference { get; set; } = default;
    public int TeamSize { get; set; } = default;
}
