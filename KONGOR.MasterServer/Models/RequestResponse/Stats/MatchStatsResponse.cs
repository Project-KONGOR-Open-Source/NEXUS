namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class MatchStatsResponse
{
    /// <summary>
    ///     The amount of gold coins that the account owns.
    /// </summary>
    [PhpProperty("points")]
    public required string GoldCoins { get; init; }

    /// <summary>
    ///     The amount of silver coins that the account owns.
    /// </summary>
    [PhpProperty("mmpoints")]
    public required string SilverCoins { get; init; }

    /// <summary>
    ///     A collection containing the summary of the match.
    ///     This is typically a single-element collection.
    /// </summary>
    [PhpProperty("match_summ")]
    public required List<MatchSummary> MatchSummary { get; init; }

    /// <summary>
    ///     A collection containing player statistics for the match.
    ///     The structure is an array with a single dictionary element, where the dictionary is keyed by player account IDs.
    /// </summary>
    [PhpProperty("match_player_stats")]
    public required List<Dictionary<int, MatchPlayerStatistics>> MatchPlayerStatistics { get; init; }

    /// <summary>
    ///     A collection containing player inventories for the match.
    ///     The structure is an array with a single dictionary element, where the dictionary is keyed by player account IDs.
    /// </summary>
    [PhpProperty("inventory")]
    public required List<Dictionary<int, MatchPlayerInventory>> MatchPlayerInventories { get; init; }

    /// <summary>
    ///     Mastery details for the hero played in the match.
    /// </summary>
    public required MatchMastery MatchMastery { get; init; }

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PhpProperty("dice_tokens")]
    public int DiceTokens { get; init; } = 100;

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    ///     Alternative to permanent "Game Pass" or temporary "Game Access" products (e.g. "m.midwars.pass",
    ///     "m.midwars.access").
    ///     Legacy accounts have full access to all game modes, and so do accounts which own the "m.allmodes.pass" store item.
    /// </summary>
    [PhpProperty("game_tokens")]
    public int GameTokens { get; init; } = 100;

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
    public int SeasonLevel { get; init; } = 100;

    /// <summary>
    ///     Unused.
    ///     <br />
    ///     May have been intended as a seasonal progression system similar to "season_level" but for creep cosmetics.
    ///     For the sake of consistency with "season_level", this property is set to "100", although it most likely has no
    ///     effect.
    /// </summary>
    [PhpProperty("creep_level")]
    public int CreepLevel { get; init; } = 100;

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
    public required List<string> OwnedStoreItems { get; init; }

    /// <summary>
    ///     Detailed information about owned store items including mastery boosts and discount coupons.
    /// </summary>
    [PhpProperty("my_upgrades_info")]
    public required Dictionary<string, object> OwnedStoreItemsData { get; init; }

    /// <summary>
    ///     The collection of selected store items.
    /// </summary>
    [PhpProperty("selected_upgrades")]
    public required List<string> SelectedStoreItems { get; init; }

    /// <summary>
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PhpProperty("slot_id")]
    public required string CustomIconSlotID { get; init; }

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PhpProperty("timestamp")]
    public long ServerTimestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    ///     Used for the quest system, which has been disabled.
    ///     <br />
    ///     While the quest system is disabled, this dictionary contains a single element with a key of "error".
    ///     The object which is the value of this element has the values of all its properties set to "0".
    /// </summary>
    [PhpProperty("quest_system")]
    public Dictionary<string, QuestSystem> QuestSystem { get; init; } = new() { { "error", new QuestSystem() } };

    /// <summary>
    ///     Unused.
    ///     <br />
    ///     Statistics related to the "Event Codex" (otherwise known as "Ascension") seasonal system.
    /// </summary>
    [PhpProperty("season_system")]
    public SeasonSystem SeasonSystem { get; init; } = new();

    /// <summary>
    ///     Statistics related to the Champions Of Newerth seasonal campaign.
    /// </summary>
    [PhpProperty("con_reward")]
    public required CampaignReward CampaignReward { get; init; } = new();

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PhpProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br />
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PhpProperty(0)]
    public bool Zero => true;
}