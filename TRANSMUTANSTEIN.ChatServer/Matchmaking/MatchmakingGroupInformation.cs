namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public record MatchmakingGroupInformation
{
    public required string ClientVersion { get; init; }

    public required ChatProtocol.TMMType GroupType { get; init; }

    public required ChatProtocol.TMMGameType GameType { get; init; }

    public required string MapName { get; init; }

    public required string[] GameModes { get; init; }

    public required string[] GameRegions { get; init; }

    public required bool Ranked { get; init; }

    public required byte MatchFidelity { get; init; }

    public required byte BotDifficulty { get; init; }

    public required bool RandomizeBots { get; init; }
}
