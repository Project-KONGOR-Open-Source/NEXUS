namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response model for the "get_upgrades" endpoint.
///     Returns account field statistics, owned store items, selected store items, currency balances, and other metadata.
///     The "get_upgrades" endpoint is called during gameplay to refresh the client's account data.
/// </summary>
public class GetUpgradesResponse
{
    /// <summary>
    ///     Account field statistics keyed by account ID.
    ///     Contains level, experience, skill ratings, games played, disconnections, and other per-mode aggregate data.
    /// </summary>
    [PHPProperty("field_stats")]
    public required Dictionary<int, FieldStatisticsEntry> FieldStatistics { get; init; }

    /// <summary>
    ///     The collection of owned store items.
    /// </summary>
    [PHPProperty("my_upgrades")]
    public required List<string> OwnedStoreItems { get; init; }

    /// <summary>
    ///     Detailed information about owned store items including mastery boosts and discount coupons.
    /// </summary>
    [PHPProperty("my_upgrades_info", isDiscriminatedUnion: true)]
    public required Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; init; }

    /// <summary>
    ///     The collection of selected store items.
    /// </summary>
    [PHPProperty("selected_upgrades")]
    public required List<string> SelectedStoreItems { get; init; }

    /// <summary>
    ///     The amount of gold coins that the account owns.
    /// </summary>
    [PHPProperty("points")]
    public required int GoldCoins { get; init; }

    /// <summary>
    ///     The amount of silver coins that the account owns.
    /// </summary>
    [PHPProperty("mmpoints")]
    public required int SilverCoins { get; init; }

    /// <summary>
    ///     Game mode access tokens for free-to-play players.
    /// </summary>
    [PHPProperty("game_tokens")]
    public int GameTokens { get; init; } = 100;

    /// <summary>
    ///     Account standing.
    ///     <list type="bullet">
    ///         <item>0 = None</item>
    ///         <item>1 = Normal</item>
    ///         <item>2 = Verified</item>
    ///         <item>3 = Legacy</item>
    ///     </list>
    /// </summary>
    [PHPProperty("standing")]
    public int Standing { get; init; } = 3;

    /// <summary>
    ///     Controls the visual appearance of tournament/seasonal buildings in matches.
    /// </summary>
    [PHPProperty("season_level")]
    public int SeasonLevel { get; init; } = 100;

    /// <summary>
    ///     Unused seasonal creep cosmetics level.
    /// </summary>
    [PHPProperty("creep_level")]
    public int CreepLevel { get; init; } = 0;

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PHPProperty("timestamp")]
    public int ServerTimestamp { get; init; } = Convert.ToInt32(Math.Min(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Convert.ToInt64(int.MaxValue)));

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Success indicator. Must be TRUE for the client to accept the response.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
}

/// <summary>
///     Per-account field statistics entry returned in the "field_stats" dictionary.
///     Maps to the database columns queried by the original PHP "addFieldStatsSelect" method.
/// </summary>
public class FieldStatisticsEntry
{
    [PHPProperty("account_id")]
    public required string AccountID { get; init; }

    [PHPProperty("level")]
    public required int Level { get; init; }

    [PHPProperty("level_exp")]
    public required string LevelExperience { get; init; }

    [PHPProperty("acc_pub_skill")]
    public required string PublicSkillRating { get; init; }

    [PHPProperty("rnk_amm_team_rating")]
    public required string RankedMatchmakingRating { get; init; }

    [PHPProperty("cs_amm_team_rating")]
    public required string CasualMatchmakingRating { get; init; }

    [PHPProperty("acc_games_played")]
    public required int PublicGamesPlayed { get; init; }

    [PHPProperty("acc_discos")]
    public required int PublicDisconnections { get; init; }

    [PHPProperty("rnk_games_played")]
    public required int RankedGamesPlayed { get; init; }

    [PHPProperty("rnk_discos")]
    public required int RankedDisconnections { get; init; }

    [PHPProperty("cs_games_played")]
    public required int CasualGamesPlayed { get; init; }

    [PHPProperty("cs_discos")]
    public required int CasualDisconnections { get; init; }

    [PHPProperty("mid_games_played")]
    public required int MidWarsGamesPlayed { get; init; }

    [PHPProperty("mid_discos")]
    public required int MidWarsDisconnections { get; init; }

    [PHPProperty("rift_games_played")]
    public required int RiftWarsGamesPlayed { get; init; }

    [PHPProperty("rift_discos")]
    public required int RiftWarsDisconnections { get; init; }

    [PHPProperty("cam_games_played")]
    public required int CampaignGamesPlayed { get; init; }

    [PHPProperty("cam_discos")]
    public required int CampaignDisconnections { get; init; }

    [PHPProperty("cam_cs_games_played")]
    public required int CampaignCasualGamesPlayed { get; init; }

    [PHPProperty("cam_cs_discos")]
    public required int CampaignCasualDisconnections { get; init; }

    /// <summary>
    ///     Number of trial (unverified) games played.
    /// </summary>
    [PHPProperty("acc_trial_games_played")]
    public int TrialGamesPlayed { get; init; } = 0;

    /// <summary>
    ///     Account standing.
    ///     <list type="bullet">
    ///         <item>0 = None</item>
    ///         <item>1 = Normal</item>
    ///         <item>2 = Verified</item>
    ///         <item>3 = Legacy</item>
    ///     </list>
    /// </summary>
    [PHPProperty("standing")]
    public int Standing { get; init; } = 3;

    /// <summary>
    ///     Whether this is a new account (for the in-game referral system).
    /// </summary>
    [PHPProperty("is_new")]
    public int IsNew { get; init; } = 0;

    /// <summary>
    ///     Creates a <see cref="FieldStatisticsEntry"/> from the account and its aggregate statistics.
    /// </summary>
    public static FieldStatisticsEntry FromAccount(Account account, AggregateStatistics aggregates, IReadOnlyDictionary<AccountStatisticsType, AccountStatistics> statisticsByType)
    {
        double GetRating(AccountStatisticsType type) => statisticsByType.TryGetValue(type, out AccountStatistics? statistics) ? statistics.SkillRating : 1500.0;

        return new FieldStatisticsEntry
        {
            AccountID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience.ToString("F1"),
            PublicSkillRating = GetRating(AccountStatisticsType.Public).ToString("F3"),
            RankedMatchmakingRating = GetRating(AccountStatisticsType.Matchmaking).ToString("F3"),
            CasualMatchmakingRating = GetRating(AccountStatisticsType.MatchmakingCasual).ToString("F3"),
            PublicGamesPlayed = aggregates.PublicGamesPlayed,
            PublicDisconnections = aggregates.PublicDisconnections,
            RankedGamesPlayed = aggregates.RankedGamesPlayed,
            RankedDisconnections = aggregates.RankedDisconnections,
            CasualGamesPlayed = aggregates.CasualGamesPlayed,
            CasualDisconnections = aggregates.CasualDisconnections,
            MidWarsGamesPlayed = aggregates.MidWarsGamesPlayed,
            MidWarsDisconnections = aggregates.MidWarsDisconnections,
            RiftWarsGamesPlayed = aggregates.RiftWarsGamesPlayed,
            RiftWarsDisconnections = aggregates.RiftWarsDisconnections,
            CampaignGamesPlayed = aggregates.CampaignGamesPlayed,
            CampaignDisconnections = aggregates.CampaignDisconnections,
            CampaignCasualGamesPlayed = aggregates.CampaignCasualGamesPlayed,
            CampaignCasualDisconnections = aggregates.CampaignCasualDisconnections
        };
    }
}
