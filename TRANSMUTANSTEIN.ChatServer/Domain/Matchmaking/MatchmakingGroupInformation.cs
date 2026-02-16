namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

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

    /// <summary>
    ///     The arranged match type derived from the game type and ranked status.
    ///     Maps to the <see cref="MatchType"/> enum values.
    ///     C++ reference: <c>c_group.cpp:962</c> — <c>CGroup::GetArrangedMatchType()</c>.
    /// </summary>
    public MatchType ArrangedMatchType => GameType switch
    {
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL          => Ranked ? MatchType.AM_MATCHMAKING : MatchType.AM_UNRANKED_MATCHMAKING,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL          => Ranked ? MatchType.AM_MATCHMAKING : MatchType.AM_UNRANKED_MATCHMAKING,

        // Reborn variants (including Caldavar Reborn) are intentionally grouped under MIDWARS.
        // The original C++ game server uses this match type to route all reborn/midwars games
        // through a shared "alternative queue" code path for stat submission and leaver handling.
        // Changing this would cause mismatched behaviour on the unmodified game server binary.
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS         => MatchType.AM_MATCHMAKING_MIDWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_REBORN_NORMAL   => MatchType.AM_MATCHMAKING_MIDWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_REBORN_CASUAL   => MatchType.AM_MATCHMAKING_MIDWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS_REBORN  => MatchType.AM_MATCHMAKING_MIDWARS,

        ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS        => MatchType.AM_MATCHMAKING_RIFTWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => MatchType.AM_MATCHMAKING_CAMPAIGN,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => MatchType.AM_MATCHMAKING_CAMPAIGN,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CUSTOM          => MatchType.AM_MATCHMAKING_CUSTOM,

        _                                                      => throw new ArgumentOutOfRangeException(nameof(GameType), $@"Unsupported Game Type ""{GameType}""")
    };

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
