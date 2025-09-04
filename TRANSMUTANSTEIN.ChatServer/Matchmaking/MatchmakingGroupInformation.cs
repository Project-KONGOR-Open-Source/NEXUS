namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroupInformation
{
    public required string ClientVersion { get; set; }

    public required ChatProtocol.TMMType GroupType { get; set; }

    public required ChatProtocol.TMMGameType GameType { get; set; }

    public required string MapName { get; set; }

    public required string[] GameModes { get; set; }

    public required string[] GameRegions { get; set; }

    public required bool Ranked { get; set; }

    public required byte MatchFidelity { get; set; }

    public required byte BotDifficulty { get; set; }

    public required bool RandomizeBots { get; set; }
}
