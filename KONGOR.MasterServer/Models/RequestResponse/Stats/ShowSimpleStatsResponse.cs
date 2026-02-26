namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class ShowSimpleStatsResponse
{
    /// <summary>
    ///     The name and clan tag of the account.
    /// </summary>
    [PHPProperty("nickname")]
    public required string NameWithClanTag { get; set; }

    /// <summary>
    ///     The ID of the account.
    /// </summary>
    [PHPProperty("account_id")]
    public required OneOf<int, string> ID { get; set; }

    /// <summary>
    ///     The level of the account.
    /// </summary>
    [PHPProperty("level")]
    public required int Level { get; set; }

    /// <summary>
    ///     The total experience of the account.
    /// </summary>
    [PHPProperty("level_exp")]
    public required int LevelExperience { get; set; }

    /// <summary>
    ///     The total number of avatars that the account owns.
    /// </summary>
    [PHPProperty("avatar_num")]
    public required int NumberOfAvatarsOwned { get; set; }

    /// <summary>
    ///     The total number of heroes that the account owns.
    ///     There are currently 139 total heroes.
    /// </summary>
    [PHPProperty("hero_num")]
    public int NumberOfHeroesOwned { get; set; } = 139;

    /// <summary>
    ///     The total number of matches that the account has played.
    /// </summary>
    [PHPProperty("total_played")]
    public required int TotalMatchesPlayed { get; set; }

    /// <summary>
    ///     Legacy field for "total_games_played" expected by some client scripts (e.g., player_stats_v2.lua).
    ///     This duplicates "total_played" to ensure compatibility.
    /// </summary>
    [PHPProperty("total_games_played")]
    public required int TotalGamesPlayedLegacy { get; set; }

    [PHPProperty("create_date")]
    public required string AccountCreationDate { get; set; }

    [PHPProperty("last_activity")]
    public required string LastActivityDate { get; set; }

    [PHPProperty("total_discos")]
    public required int TotalDisconnects { get; set; }

    [PHPProperty("cam_wins")]
    public required int RankedWins { get; set; }

    [PHPProperty("cam_losses")]
    public required int RankedLosses { get; set; }

    // Favorite Heroes
    [PHPProperty("favHero1")]
    public required string FavHero1 { get; set; }

    [PHPProperty("favHero1Time")]
    public required int FavHero1Time { get; set; }

    [PHPProperty("favHero2")]
    public required string FavHero2 { get; set; }

    [PHPProperty("favHero2Time")]
    public required int FavHero2Time { get; set; }

    [PHPProperty("favHero3")]
    public required string FavHero3 { get; set; }

    [PHPProperty("favHero3Time")]
    public required int FavHero3Time { get; set; }

    [PHPProperty("favHero4")]
    public required string FavHero4 { get; set; }

    [PHPProperty("favHero4Time")]
    public required int FavHero4Time { get; set; }

    [PHPProperty("favHero5")]
    public required string FavHero5 { get; set; }

    [PHPProperty("favHero5Time")]
    public required int FavHero5Time { get; set; }
    
    // Full Identifiers for Display Name (e.g. "Hero_Jereziah")
    
    [PHPProperty("favHero1_2")]
    public required string FavHero1_2 { get; set; }
    
    [PHPProperty("favHero2_2")]
    public required string FavHero2_2 { get; set; }
    
    [PHPProperty("favHero3_2")]
    public required string FavHero3_2 { get; set; }
    
    [PHPProperty("favHero4_2")]
    public required string FavHero4_2 { get; set; }
    
    [PHPProperty("favHero5_2")]
    public required string FavHero5_2 { get; set; }

    /// <summary>
    ///     The current season.
    ///     The last season before the services went offline was 12.
    /// </summary>
    [PHPProperty("season_id")]
    public required int CurrentSeason { get; set; }

    /// <summary>
    ///     Controls the visual appearance of tournament/seasonal buildings (towers, barracks, etc.) in matches.
    ///     <code>
    ///         Level 0     -> default appearance
    ///         Level 01-09 -> tier 01 appearance
    ///         Level 10-24 -> tier 02 appearance
    ///         Level 25-49 -> tier 03 appearance
    ///         Level 50-74 -> tier 04 appearance
    ///         Level 75-99 -> tier 05 appearance
    ///         Level 100+  -> tier 06 appearance
    ///     </code>
    /// </summary>
    [PHPProperty("season_level")]
    public int SeasonLevel { get; set; } = 100;

    /// <summary>
    ///     Unused.
    ///     <br/>
    ///     May have been intended as a seasonal progression system similar to "season_level" but for creep cosmetics.
    ///     For the sake of consistency with "season_level", this property is set to "100", although it most likely has no effect.
    /// </summary>
    [PHPProperty("creep_level")]
    public int CreepLevel { get; set; } = 100;

    /// <summary>
    ///     Simple current season statistics.
    /// </summary>
    [PHPProperty("season_normal")]
    public required SimpleSeasonStats SimpleSeasonStats { get; set; }

    /// <summary>
    ///     Simple current casual season statistics.
    /// </summary>
    [PHPProperty("season_casual")]
    public required SimpleSeasonStats SimpleCasualSeasonStats { get; set; }

    /// <summary>
    ///     Simple current midwars season statistics.
    /// </summary>
    [PHPProperty("season_midwars")]
    public required SimpleSeasonStats SimpleMidWarsSeasonStats { get; set; }

    /// <summary>
    ///     The total number of MVP awards of the account.
    /// </summary>
    [PHPProperty("mvp_num")]
    public required string MVPAwardsCount { get; set; }

    /// <summary>
    ///     The names of the account's top 4 awards.
    /// </summary>
    [PHPProperty("award_top4_name")]
    public required List<string> Top4AwardNames { get; set; }

    /// <summary>
    ///     The counts of the account's top 4 awards.
    /// </summary>
    [PHPProperty("award_top4_num")]
    public required List<string> Top4AwardCounts { get; set; }

    /// <summary>
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PHPProperty("slot_id")]
    public required string CustomIconSlotID { get; set; }

    /// <summary>
    ///     The collection of owned store items.
    /// </summary>
    [PHPProperty("my_upgrades")]
    public required List<string> OwnedStoreItems { get; set; }

    /// <summary>
    ///     The collection of selected store items.
    /// </summary>
    [PHPProperty("selected_upgrades")]
    public required List<string> SelectedStoreItems { get; set; }

    /// <summary>
    ///     Metadata attached to each of the account's owned store items.
    /// </summary>
    [PHPProperty("my_upgrades_info", IsDiscriminatedUnion = true)]
    public required Dictionary<string, OneOf<object, Dictionary<string, object>>> OwnedStoreItemsData { get; set; }

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PHPProperty("dice_tokens")]
    public string DiceTokens { get; set; } = "1";

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    /// </summary>
    [PHPProperty("game_tokens")]
    public int GameTokens { get; set; } = 0;

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PHPProperty("timestamp")]
    public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public OneOf<int, string> VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
    
    // ==================================================================================
    // TOP LEVEL RANK FIELDS (Required for Profile UI)
    // ==================================================================================
    
    /// <summary>Legacy rank name (e.g. "Diamond 1") or ID.</summary>
    [PHPProperty("rank")]
    public string? Rank { get; set; }

    /// <summary>Current Badge ID (0-21) as string.</summary>
    [PHPProperty("current_level")]
    public string? CurrentRankTop { get; set; }

    /// <summary>Matchmaking Rating (MMR) as string.</summary>
    [PHPProperty("smr")]
    public string? RankedRatingTop { get; set; }
    
    /// <summary>Highest Badge ID achieved as string.</summary>
    [PHPProperty("highest_level_current")]
    public string? HighestRankTop { get; set; }

    [PHPProperty("con_reward")]
    public string? ConReward { get; set; }
    
    // ==================================================================================
    // TOP LEVEL STATISTICS FIELDS (Required for Profile UI -> Statistics Tab)
    // ==================================================================================
    
    [PHPProperty("herokills")]
    public string? HeroKills { get; set; }

    [PHPProperty("deaths")]
    public string? Deaths { get; set; }

    [PHPProperty("heroassists")]
    public string? HeroAssists { get; set; }

    [PHPProperty("k_d_a")]
    public string? KDA { get; set; }

    [PHPProperty("avgGameLength")]
    public string? AvgGameLength { get; set; }

    [PHPProperty("avgXP_min")]
    public string? AvgXPMin { get; set; }

    [PHPProperty("avgDenies")]
    public string? AvgDenies { get; set; }

    [PHPProperty("avgCreepKills")]
    public string? AvgCreepKills { get; set; }

    [PHPProperty("avgNeutralKills")]
    public string? AvgNeutralKills { get; set; }

    [PHPProperty("avgActions_min")]
    public string? AvgActionsMin { get; set; }

    [PHPProperty("avgWardsUsed")]
    public string? AvgWardsUsed { get; set; }

    [PHPProperty("gold")]
    public string? Gold { get; set; }

    // Smackdowns Panel
    [PHPProperty("humiliation")]
    public string? Humiliation { get; set; }

    [PHPProperty("smackdown")]
    public string? Smackdown { get; set; }

    [PHPProperty("nemesis")]
    public string? Nemesis { get; set; }

    [PHPProperty("retribution")]
    public string? Retribution { get; set; }

    // ==================================================================================
    // MASTERY FIELDS
    // ==================================================================================
    
    /// <summary>
    ///     Per-hero mastery experience.
    ///     Key: Index (0,1,2...), Value: Object { "heroname": "Hero_X", "exp": "12345" }
    ///     Changed to object to allow List&lt;object&gt; (PHP Vector) or Dictionary (PHP Hashtable).
    /// </summary>
    [PHPProperty("mastery_info")]
    public object? MasteryInfo { get; set; }

    /// <summary>
    ///     Mastery rewards status.
    ///     Changed to object to allow List&lt;object&gt;.
    /// </summary>
    [PHPProperty("mastery_rewards")]
    public object? MasteryRewards { get; set; }
}
