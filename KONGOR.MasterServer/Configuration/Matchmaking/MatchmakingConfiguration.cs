namespace KONGOR.MasterServer.Configuration.Matchmaking;

public class MatchmakingConfiguration
{
    public required MatchmakingMapConfiguration Ranked { get; set; }

    public required MatchmakingMapConfiguration Unranked { get; set; }

    public required MatchmakingMapConfiguration MidWars { get; set; }

    public required MatchmakingMapConfiguration RiftWars { get; set; }
}