namespace KONGOR.MasterServer.Configuration.Matchmaking;

public class MatchmakingMapConfiguration
{
    public required string Map { get; set; }

    public required string[] Regions { get; set; }

    public required string[] Modes { get; set; }

    public required MatchConfiguration Match { get; set; }
}