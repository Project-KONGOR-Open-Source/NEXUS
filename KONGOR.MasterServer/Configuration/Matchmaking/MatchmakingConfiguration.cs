namespace KONGOR.MasterServer.Configuration.Matchmaking;

/// <summary>
///     Root matchmaking configuration loaded from "MatchmakingConfiguration.json".
///     Provides the data needed for both the popularity update protocol and the match broker.
/// </summary>
public class MatchmakingConfiguration
{
    /// <summary>
    ///     The global set of game modes available across all maps.
    ///     Each entry is a short code (e.g. "ap", "sd", "rb") matching the <see cref="ChatProtocol.TMMGameMode"/> enum.
    /// </summary>
    public required string[] GameModes { get; set; }

    /// <summary>
    ///     The global set of game types sent in the popularity update.
    ///     These are the game type identifiers that the client uses to enable game type buttons in the matchmaking UI.
    ///     Matches the original "teamfinder_availableGameTypes" CVAR (default "1|2|3|4|5").
    /// </summary>
    public required int[] GameTypes { get; set; }

    /// <summary>
    ///     The global set of regions available for matchmaking.
    ///     Each entry is a region code (e.g. "EU", "USE") matching the <see cref="ChatProtocol.TMMGameRegion"/> enum.
    /// </summary>
    public required string[] Regions { get; set; }

    /// <summary>
    ///     The per-map matchmaking configurations, keyed by display name.
    /// </summary>
    public required MatchmakingMapConfiguration[] Maps { get; set; }
}

/// <summary>
///     Configuration for a single matchmaking map (e.g. Forests of Caldavar, MidWars, RiftWars).
/// </summary>
public class MatchmakingMapConfiguration
{
    /// <summary>
    ///     The internal map identifier sent in the popularity update protocol (e.g. "caldavar", "midwars", "riftwars").
    /// </summary>
    public required string Map { get; set; }

    /// <summary>
    ///     The game type identifiers for this map, matching <see cref="ChatProtocol.TMMGameType"/>.
    ///     A map can support multiple game types (e.g. Caldavar supports both Campaign Normal and Campaign Casual).
    /// </summary>
    public required int[] GameTypes { get; set; }

    /// <summary>
    ///     The game modes available on this map (subset of <see cref="MatchmakingConfiguration.GameModes"/>).
    /// </summary>
    public required string[] Modes { get; set; }

    /// <summary>
    ///     The regions available on this map (subset of <see cref="MatchmakingConfiguration.Regions"/>).
    /// </summary>
    public required string[] Regions { get; set; }

    /// <summary>
    ///     The match settings for this map.
    /// </summary>
    public required MatchConfiguration Match { get; set; }
}

public class MatchConfiguration
{
    public required int MaximumPlayerRatingDifference { get; set; }

    public required bool IsRanked { get; set; }
}
