namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class SeasonProgress
{
    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public required int AccountID { get; init; }

    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required int MatchID { get; init; }

    /// <summary>
    ///     Whether the match was a casual ranked match ("1") or competitive ranked match ("0").
    /// </summary>
    [PhpProperty("is_casual")]
    public required string IsCasual { get; init; }

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) before the match.
    /// </summary>
    [PhpProperty("mmr_before")]
    public required string MMRBefore { get; init; }

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) after the match.
    /// </summary>
    [PhpProperty("mmr_after")]
    public required string MMRAfter { get; init; }

    /// <summary>
    ///     The player's medal rank before the match.
    ///     <code>
    ///         00      -> Unranked
    ///         01-05   -> Bronze   (V, IV, III, II, I)
    ///         06-10   -> Silver   (V, IV, III, II, I)
    ///         11-15   -> Gold     (V, IV, III, II, I)
    ///         16-20   -> Diamond  (V, IV, III, II, I)
    ///         21      -> Immortal
    ///     </code>
    /// </summary>
    [PhpProperty("medal_before")]
    public required string MedalBefore { get; init; }

    /// <summary>
    ///     The player's medal rank after the match.
    ///     Uses the same medal ranking system as "medal_before".
    /// </summary>
    [PhpProperty("medal_after")]
    public required string MedalAfter { get; init; }

    /// <summary>
    ///     The seasonal campaign identifier.
    /// </summary>
    [PhpProperty("season")]
    public required string Season { get; init; }

    /// <summary>
    ///     The number of placement matches the player has completed in the current season.
    ///     Players must complete placement matches before receiving their seasonal medal rank.
    /// </summary>
    [PhpProperty("placement_matches")]
    public required int PlacementMatches { get; init; }

    /// <summary>
    ///     The number of placement matches won by the player in the current season.
    /// </summary>
    [PhpProperty("placement_wins")]
    public required string PlacementWins { get; init; }

    /// <summary>
    ///     The player's current ranking position on the Immortal leaderboard.
    ///     Only populated for Immortal rank players (medal 21) with a ranking between 1 and 100.
    ///     Not present in the response for players below Immortal rank or outside the top 100.
    /// </summary>
    [PhpProperty("ranking")]
    public string? Ranking { get; init; }
}