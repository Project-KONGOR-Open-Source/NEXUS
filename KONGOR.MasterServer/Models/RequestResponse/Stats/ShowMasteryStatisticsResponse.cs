namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response for show_stats with table="mastery" (hero mastery progression data).
/// </summary>
public class ShowMasteryStatisticsResponse(Account account)
{
    /// <summary>
    ///     The unique account identifier.
    /// </summary>
    [PHPProperty("account_id")]
    public string AccountID { get; init; } = account.ID.ToString();

    /// <summary>
    ///     The account name with the clan tag (if applicable).
    /// </summary>
    [PHPProperty("nickname")]
    public string Nickname { get; init; } = account.NameWithClanTag;

    /// <summary>
    ///     The clan name, or an empty string if the account is not in a clan.
    /// </summary>
    [PHPProperty("name")]
    public string ClanName { get; init; } = account.Clan?.Name ?? string.Empty;

    /// <summary>
    ///     The clan rank (tier) name.
    /// </summary>
    [PHPProperty("rank")]
    public string ClanRank { get; init; } = account.ClanTierName;

    /// <summary>
    ///     The account standing (moderation status).
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; init; } = "3"; // TODO: Implement Account Standing/Moderation Status

    /// <summary>
    ///     The date the account was created.
    /// </summary>
    [PHPProperty("create_date")]
    public string CreateDate { get; init; } = account.TimestampCreated.ToString("MM/dd/yyyy");

    /// <summary>
    ///     The date of last activity on the account.
    /// </summary>
    [PHPProperty("last_activity")]
    public string LastActivity { get; init; } = account.TimestampLastActive.ToString("MM/dd/yyyy");

    /// <summary>
    ///     The collection of selected store items.
    /// </summary>
    [PHPProperty("selected_upgrades")]
    public List<string> SelectedStoreItems { get; init; } = account.SelectedStoreItems;

    /// <summary>
    ///     The account level.
    /// </summary>
    [PHPProperty("level")]
    public int Level { get; init; } = account.User.TotalLevel;

    /// <summary>
    ///     The current experience points towards the next level.
    /// </summary>
    [PHPProperty("level_exp")]
    public int LevelExperience { get; init; } = account.User.TotalExperience;

    /// <summary>
    ///     Hero mastery progression data.
    ///     Each entry contains the hero name and accumulated mastery experience.
    /// </summary>
    [PHPProperty("mastery_info")]
    public List<HeroMasteryInfo> MasteryInfo { get; set; } = [];

    /// <summary>
    ///     Mastery reward tiers and their claim status.
    ///     Only populated when viewing your own account.
    /// </summary>
    [PHPProperty("mastery_rewards")]
    public List<MasteryRewardTier> MasteryRewards { get; set; } = [];

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Success indicator. Set to <see langword="true"/> on successful response, <see langword="false"/> if an error occurs.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
}

/// <summary>
///     Hero mastery progression entry.
/// </summary>
public class HeroMasteryInfo
{
    /// <summary>
    ///     The hero identifier (e.g., "Hero_Pyromancer").
    /// </summary>
    [PHPProperty("heroname")]
    public required string HeroName { get; init; }

    /// <summary>
    ///     The accumulated mastery experience for this hero.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; init; }
}

/// <summary>
///     Mastery reward tier entry.
/// </summary>
public class MasteryRewardTier
{
    /// <summary>
    ///     The mastery level this reward is for (1-40).
    /// </summary>
    [PHPProperty("level")]
    public required int Level { get; init; }

    /// <summary>
    ///     Whether this reward has already been claimed.
    /// </summary>
    [PHPProperty("alreadygot")]
    public bool AlreadyClaimed { get; init; }

    /// <summary>
    ///     The reward details for this tier.
    /// </summary>
    [PHPProperty("reward")]
    public required MasteryReward Reward { get; init; }
}

/// <summary>
///     Mastery reward details.
/// </summary>
public class MasteryReward
{
    /// <summary>
    ///     The product ID if this reward is a store item.
    /// </summary>
    [PHPProperty("product_id")]
    public int ProductID { get; init; }

    /// <summary>
    ///     The product name if this reward is a store item.
    /// </summary>
    [PHPProperty("product_name")]
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    ///     The product local content identifier if this reward is a store item.
    /// </summary>
    [PHPProperty("product_local_content")]
    public string ProductLocalContent { get; init; } = string.Empty;

    /// <summary>
    ///     The quantity of the product reward.
    /// </summary>
    [PHPProperty("quantity")]
    public int Quantity { get; init; }

    /// <summary>
    ///     Gold coins awarded at this tier.
    /// </summary>
    [PHPProperty("points")]
    public int GoldCoins { get; init; }

    /// <summary>
    ///     Silver coins awarded at this tier.
    /// </summary>
    [PHPProperty("mmpoints")]
    public int SilverCoins { get; init; }

    /// <summary>
    ///     Game tokens awarded at this tier.
    /// </summary>
    [PHPProperty("tickets")]
    public int GameTokens { get; init; }
}
