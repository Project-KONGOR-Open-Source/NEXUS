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

    public byte TeamSize => GameType switch
    {
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL          => 5, // caldavar
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL          => 5, // caldavar_old
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS         => 5, // midwars
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS        => 5, // riftwars
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => 5, // caldavar
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => 5, // caldavar_old
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_REBORN_NORMAL   => 5, // caldavar_reborn
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_REBORN_CASUAL   => 5, // caldavar_reborn
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS_REBORN  => 5, // midwars
        _                                                      => throw new ArgumentOutOfRangeException(nameof(GameType), $@"Unsupported Game Type: ""{GameType}""")
    };
}
