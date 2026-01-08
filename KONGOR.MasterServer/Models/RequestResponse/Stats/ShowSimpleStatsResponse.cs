namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class ShowSimpleStatsResponse
{
    /// <summary>
    ///     The name and clan tag of the account.
    /// </summary>
    [PhpProperty("nickname")]
    public required string NameWithClanTag { get; set; }

    /// <summary>
    ///     The ID of the account.
    /// </summary>
    [PhpProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The level of the account.
    /// </summary>
    [PhpProperty("level")]
    public required int Level { get; set; }

    /// <summary>
    ///     The total experience of the account.
    /// </summary>
    [PhpProperty("level_exp")]
    public required int LevelExperience { get; set; }

    /// <summary>
    ///     The total number of avatars that the account owns.
    /// </summary>
    [PhpProperty("avatar_num")]
    public required int NumberOfAvatarsOwned { get; set; }

    /// <summary>
    ///     The total number of heroes that the account owns.
    ///     There are currently 139 total heroes.
    /// </summary>
    [PhpProperty("hero_num")]
    public int NumberOfHeroesOwned { get; set; } = 139;

    /// <summary>
    ///     The total number of matches that the account has played.
    /// </summary>
    [PhpProperty("total_played")]
    public required int TotalMatchesPlayed { get; set; }

    /// <summary>
    ///     The current season.
    ///     The last season before the services went offline was 12.
    /// </summary>
    [PhpProperty("season_id")]
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
    [PhpProperty("season_level")]
    public int SeasonLevel { get; set; } = 100;

    /// <summary>
    ///     Unused.
    ///     <br/>
    ///     May have been intended as a seasonal progression system similar to "season_level" but for creep cosmetics.
    ///     For the sake of consistency with "season_level", this property is set to "100", although it most likely has no effect.
    /// </summary>
    [PhpProperty("creep_level")]
    public int CreepLevel { get; set; } = 100;

    /// <summary>
    ///     Simple current season statistics.
    /// </summary>
    [PhpProperty("season_normal")]
    public required SimpleSeasonStats SimpleSeasonStats { get; set; }

    /// <summary>
    ///     Simple current casual season statistics.
    /// </summary>
    [PhpProperty("season_casual")]
    public required SimpleSeasonStats SimpleCasualSeasonStats { get; set; }

    /// <summary>
    ///     The total number of MVP awards of the account.
    /// </summary>
    [PhpProperty("mvp_num")]
    public required int MVPAwardsCount { get; set; }

    /// <summary>
    ///     The names of the account's top 4 awards.
    /// </summary>
    [PhpProperty("award_top4_name")]
    public required List<string> Top4AwardNames { get; set; }

    /// <summary>
    ///     The counts of the account's top 4 awards.
    /// </summary>
    [PhpProperty("award_top4_num")]
    public required List<int> Top4AwardCounts { get; set; }

    /// <summary>
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PhpProperty("slot_id")]
    public required string CustomIconSlotID { get; set; }

    /// <summary>
    ///     The collection of owned store items.
    ///     <code>
    ///         Chat Name Colour       =>   "cc"
    ///         Chat Symbol            =>   "cs"
    ///         Account Icon           =>   "ai"
    ///         Alternative Avatar     =>   "aa"
    ///         Announcer Voice        =>   "av"
    ///         Taunt                  =>   "t"
    ///         Courier                =>   "c"
    ///         Hero                   =>   "h"
    ///         Early-Access Product   =>   "eap"
    ///         Status                 =>   "s"
    ///         Miscellaneous          =>   "m"
    ///         Ward                   =>   "w"
    ///         Enhancement            =>   "en"
    ///         Coupon                 =>   "cp"
    ///         Mastery                =>   "ma"
    ///         Creep                  =>   "cr"
    ///         Building               =>   "bu"
    ///         Taunt Badge            =>   "tb"
    ///         Teleportation Effect   =>   "te"
    ///         Selection Circle       =>   "sc"
    ///         Bundle                 =>   string.Empty
    ///     </code>
    /// </summary>
    /// <summary>
    ///     The collection of owned store items.
    ///     <code>
    ///         Chat Name Colour       =>   "cc"
    ///         Chat Symbol            =>   "cs"
    ///         Account Icon           =>   "ai"
    ///         Alternative Avatar     =>   "aa"
    ///         Announcer Voice        =>   "av"
    ///         Taunt                  =>   "t"
    ///         Courier                =>   "c"
    ///         Hero                   =>   "h"
    ///         Early-Access Product   =>   "eap"
    ///         Status                 =>   "s"
    ///         Miscellaneous          =>   "m"
    ///         Ward                   =>   "w"
    ///         Enhancement            =>   "en"
    ///         Coupon                 =>   "cp"
    ///         Mastery                =>   "ma"
    ///         Creep                  =>   "cr"
    ///         Building               =>   "bu"
    ///         Taunt Badge            =>   "tb"
    ///         Teleportation Effect   =>   "te"
    ///         Selection Circle       =>   "sc"
    ///         Bundle                 =>   string.Empty
    ///     </code>
    /// </summary>
    [PhpProperty("my_upgrades")]
    public required List<string> OwnedStoreItems { get; set; }

    /// <summary>
    ///     The collection of selected store items.
    /// </summary>
    [PhpProperty("selected_upgrades")]
    public required List<string> SelectedStoreItems { get; set; }

    /// <summary>
    ///     Metadata attached to each of the account's owned store items.
    /// </summary>
    [PhpProperty("my_upgrades_info")]
    public required Dictionary<string, OneOf<global::KONGOR.MasterServer.Models.RequestResponse.SRP.StoreItemData, global::KONGOR.MasterServer.Models.RequestResponse.Store.StoreItemDiscountCoupon>> OwnedStoreItemsData { get; set; }

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PhpProperty("dice_tokens")]
    public int DiceTokens { get; set; } = 100;

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    ///     Alternative to permanent "Game Pass" or temporary "Game Access" products (e.g. "m.midwars.pass", "m.midwars.access").
    ///     Legacy accounts have full access to all game modes, and so do accounts which own the "m.allmodes.pass" store item.
    /// </summary>
    [PhpProperty("game_tokens")]
    public int GameTokens { get; set; } = 100;

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PhpProperty("timestamp")]
    public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PhpProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PhpProperty(0)]
    public bool Zero => true;
}

public class SimpleSeasonStats
{
    /// <summary>
    ///     The number of ranked matches won.
    /// </summary>
    [PhpProperty("wins")]
    public required int  RankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of ranked matches lost.
    /// </summary>
    [PhpProperty("losses")]
    public required int RankedMatchesLost { get; set; }

    /// <summary>
    ///     The current number of consecutive ranked matches won.
    /// </summary>
    [PhpProperty("win_streak")]
    public required int WinStreak { get; set; }

    /// <summary>
    ///     Whether the account needs to play placement matches or not.
    ///     A value of "1" means TRUE, and a value of "0" means FALSE.
    /// </summary>
    [PhpProperty("is_placement")]
    public required int InPlacementPhase { get; set; }

    /// <summary>
    ///     Unknown.
    ///     Potentially, the number of account levels gained during the season.
    /// </summary>
    [PhpProperty("current_level")]
    public required int LevelsGainedThisSeason { get; set; }
}
