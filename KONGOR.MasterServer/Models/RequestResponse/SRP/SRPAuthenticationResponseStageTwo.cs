namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

/// <summary>
///     <para>
///         The model of the response sent to the game client, following a successful SRP authentication.
///         This is by far the most complex data transfer object that the master server and the game client exchange.
///     </para>
///     <para>
///         The online tool https://www.cleancss.com/php-beautify/ was used to format the raw PHP data to a humanly-readable PHP object.
///         This object, represented as string, was then deserialized to a C# object and re-serialized as JSON string.
///         Finally, the raw version of this data model was generated from the JSON string by using the "Edit > Paste Special > Paste JSON As Classes" feature of Visual Studio.
///     </para>
/// </summary>
public class SRPAuthenticationResponseStageTwo
{
    /// <summary>
    ///     M2 : the server's proof; the client should verify this value and use it to complete the SRP challenge exchange
    /// </summary>
    [PHPProperty("proof")]
    public required string ServerProof { get; set; }

    /// <summary>
    ///     The ID of the main account associated with the account attempting to log in.
    ///     This ID will be the same as the ID of the account attempting to log in, when logging in with a main account.
    /// </summary>
    [PHPProperty("super_id")]
    public required string MainAccountID { get; set; }

    /// <summary>
    ///     The ID of the account attempting to log in.
    /// </summary>
    [PHPProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The Garena ID of the account attempting to log in.
    ///     This property only applies to the Garena client.
    /// </summary>
    [PHPProperty("garena_id")]
    public required string GarenaID { get; set; }

    /// <summary>
    ///     The name of the account attempting to log in.
    /// </summary>
    [PHPProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The email address of the user to which the account attempting to log in belongs.
    /// </summary>
    [PHPProperty("email")]
    public required string Email { get; set; }

    /// <summary>
    ///     The type of the account.
    ///     <br/>
    ///     0 = Disabled; 1 = Demo; 2 = Server Host; 3 = Regular; 4 = Premium; 5 = Staff; 6 = Game Master; 7 = Tournament Moderator; 8 = Tournament Caster
    /// </summary>
    [PHPProperty("account_type")]
    public required string AccountType { get; set; }

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("trial")]
    public string Trial { get; set; } = "0";

    /// <summary>
    ///     The ID of the currently active suspension on the account.
    ///     If there is no currently active suspension on the account, then this value is "0".
    /// </summary>
    [PHPProperty("susp_id")]
    public required string SuspensionID { get; set; }

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("prepay_only")]
    public string PrePayOnly { get; set; } = "0";

    /// <summary>
    ///     The type of the account.
    ///     <br/>
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     Whether to automatically download the backup of the game client configuration files from the cloud or not on login.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("use_cloud")]
    public required string UseCloud { get; set; }

    /// <summary>
    ///     Whether the password has expired or not.
    ///     <br/>
    ///     NULL = Not Expired; 0 = Require Password Change; Any Non-Zero Positive Integer = Suggest Password Change
    /// </summary>
    [PHPProperty("pass_exp")]
    public string? PasswordExpired { get; set; } = null;

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("is_new")]
    public int IsNew { get; set; } = 0;

    /// <summary>
    ///     The authentication cookie that will be used to authorize the account's session in all subsequent requests.
    /// </summary>
    [PHPProperty("cookie")]
    public required string Cookie { get; set; }

    /// <summary>
    ///     The IP address of the game client which made the authentication request.
    /// </summary>
    [PHPProperty("ip")]
    public required string IPAddress { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("minimum_ranked_level")]
    public string MinimumRankedLevel { get; set; } = "3";

    /// <summary>
    ///     A floating point representation of the percentage at which the account is marked as a leaver.
    ///     The default value is ".05".
    /// </summary>
    [PHPProperty("leaverthreshold")]
    public required string LeaverThreshold { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("is_under_24")]
    public bool IsUnder24 { get; set; } = false;

    /// <summary>
    ///     Whether the account attempting to log in has any sub-accounts or not.
    /// </summary>
    [PHPProperty("is_subaccount")]
    public required bool HasSubAccounts { get; set; }

    /// <summary>
    ///     Whether the account attempting to log in is a sub-account or not.
    /// </summary>
    [PHPProperty("is_current_subaccount")]
    public required bool IsSubAccount { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("icb_url")]
    public required string ICBURL { get; set; }

    /// <summary>
    ///     A hash of some of the account's authentication details.
    /// </summary>
    [PHPProperty("auth_hash")]
    public required string AuthenticationHash { get; set; }

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PHPProperty("host_time")]
    public string HostTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     The IP address of the chat server.
    /// </summary>
    [PHPProperty("chat_url")]
    public required string ChatServerIPAddress { get; set; }

    /// <summary>
    ///     The port of the chat server.
    /// </summary>
    [PHPProperty("chat_port")]
    public required string ChatServerPort { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("commenting_url")]
    public string? CommentingURL { get; set; } = null;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("commenting_port")]
    public int? CommentingPort { get; set; } = null;

    /// <summary>
    ///     The list of chat channels that the account automatically connects to.
    /// </summary>
    [PHPProperty("chatrooms")]
    public required List<string> ChatChannels { get; set; }

    /// <summary>
    ///     All the accounts associated with the account attempting to log in, in registration order.
    ///     Each inner-list is composed of two elements, the account name and the account ID.
    /// </summary>
    [PHPProperty("identities")]
    public required List<List<string>> Accounts { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("cafe_id")]
    public string? CafeID { get; set; } = null;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("gca_regular")]
    public string? GCARegular { get; set; } = null;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("gca_prime")]
    public string? GCAPrime { get; set; } = null;

    /// <summary>
    ///     The amount of gold coins that the account owns.
    /// </summary>
    [PHPProperty("points")]
    public required string GoldCoins { get; set; }

    /// <summary>
    ///     The amount of silver coins that the account owns.
    /// </summary>
    [PHPProperty("mmpoints")]
    public required int SilverCoins { get; set; }

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PHPProperty("dice_tokens")]
    public int DiceTokens { get; set; } = 100;

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    ///     Alternative to permanent "Game Pass" or temporary "Game Access" products (e.g. "m.midwars.pass", "m.midwars.access").
    ///     Legacy accounts have full access to all game modes, and so do accounts which own the "m.allmodes.pass" store item.
    /// </summary>
    [PHPProperty("game_tokens")]
    public int GameTokens { get; set; } = 100;

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
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PHPProperty("slot_id")]
    public required string CustomIconSlotID { get; set; }

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PHPProperty("timestamp")]
    public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    ///     The current season.
    ///     The last season before the services went offline was 12.
    /// </summary>
    [PHPProperty("campaign_current_season")]
    public required string CurrentSeason { get; set; }

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Appears to be the MMR rank thresholds.
    /// </summary>
    [PHPProperty("mmr_rank")]
    public string MMRRankThresholds { get; set; } = "1250,1275,1300,1330,1365,1400,1435,1470,1505,1540,1575,1610,1645,1685,1725,1765,1805,1850,1900,1950";

    /// <summary>
    ///     The time (in UTC seconds) at which the account is no longer muted.
    /// </summary>
    [PHPProperty("mute_expiration")]
    public required int MuteExpiration { get; set; }

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;

    /// <summary>
    ///     The account's list of friend accounts.
    ///     The outer-dictionary needs to contain a single entry with the key being the owner account's ID.
    ///     The inner-dictionary needs to contain multiple entries, the key of each being the respective friend account's ID.
    /// </summary>
    [PHPProperty("buddy_list")]
    public required Dictionary<string, Dictionary<string, FriendAccount>> FriendAccountList { get; set; }

    /// <summary>
    ///     The account's list of ignored accounts.
    ///     This dictionary needs to contain a single entry with the key being the owner account's ID.
    /// </summary>
    [PHPProperty("ignored_list")]
    public required Dictionary<string, List<IgnoredAccount>> IgnoredAccountsList { get; set; }

    /// <summary>
    ///     The account's list of banned accounts.
    ///     This dictionary needs to contain a single entry with the key being the owner account's ID.
    /// </summary>
    [PHPProperty("banned_list")]
    public required Dictionary<string, List<BannedAccount>> BannedAccountsList { get; set; }

    /// <summary>
    ///     The list of members of the account's clan.
    ///     This dictionary needs to contain multiple entries, the key of each being the respective clan member account's ID.
    /// </summary>
    [PHPProperty("clan_roster")]
    public required Dictionary<string, ClanMemberAccount> ClanRoster { get; set; }

    /// <summary>
    ///     The account's clan membership data if the account is part of a clan, or an error message if the account is not part of a clan.
    /// </summary>
    [PHPProperty("clan_member_info", isDiscriminatedUnion: true)]
    public required OneOf<ClanMemberData, ClanMemberDataError> ClanMembershipData { get; set; }

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
    [PHPProperty("my_upgrades_info", isDiscriminatedUnion: true)]
    public required Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; set; }

    /// <summary>
    ///     The list of heroes with a non-standard-ownership (free/early-access/etc.) model.
    ///     The latest list of heroes sent by the legacy HoN master server before the services shut down is the list of free heroes: "Hero_Genesis,Hero_Dorin_Tal,Hero_Adeve".
    ///     <br/>
    ///     This property is obsolete.
    /// </summary>
    [PHPProperty("hero_list")]
    public HeroList HeroList { get; set; } = new ();

    /// <summary>
    ///     Used for the Tencent anti-DDoS protection component, which does network packet watermarking that gets verified by the game server proxy.
    /// </summary>
    [PHPProperty("sec_info")]
    public SecurityInformation SecurityInformation { get; set; } = new ();

    /// <summary>
    ///     A set of static values used to generate award-centric data and trigger award-centric events.
    /// </summary>
    [PHPProperty("awards_tooltip")]
    public required AwardsTooltips AwardsTooltips { get; set; }

    /// <summary>
    ///     Account information, in the form of a list that contains a single object of string values.
    /// </summary>
    [PHPProperty("infos")]
    public required List<DataPoint> DataPoints { get; set; }

    /// <summary>
    ///     Used for the quest system, which has been disabled.
    ///     <br/>
    ///     While the quest system is disabled, this dictionary contains a single element with a key of "error".
    ///     The object which is the value of this element has the values of all its properties set to "0".
    /// </summary>
    [PHPProperty("quest_system")]
    public Dictionary<string, QuestSystem> QuestSystem { get; set; } = new () { { "error", new QuestSystem() } };

    /// <summary>
    ///     The cloud storage settings of the account.
    ///     The cloud is used for backing up and restoring the game client configuration files.
    /// </summary>
    [PHPProperty("account_cloud_storage_info")]
    public required CloudStorageInformation CloudStorageInformation { get; set; }

    /// <summary>
    ///     The account's list of notifications.
    ///     This list does not include system notifications, which are handled separately.
    /// </summary>
    [PHPProperty("notifications")]
    public required List<Notification> Notifications { get; set; }
}

public class FriendAccount
{
    /// <summary>
    ///     The account ID of the friend.
    /// </summary>
    [PHPProperty("buddy_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The account name of the friend.
    /// </summary>
    [PHPProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The name of the friend group that the friend is in.
    ///     A friend list can have multiple friend lists.
    ///     Additionally, a friend does not need to be in a group.
    /// </summary>
    [PHPProperty("group")]
    public required string Group { get; set; }

    /// <summary>
    ///     The clan tag of the friend.
    /// </summary>
    [PHPProperty("clan_tag")]
    public required string ClanTag { get; set; }

    /// <summary>
    ///     Unknown.
    ///     Seems to be set to "2" in all the network packet dumps.
    /// </summary>
    [PHPProperty("status")]
    public string Status { get; set; } = "2";

    /// <summary>
    ///     The account type of the friend.
    ///     <br/>
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     Whether the referral system status of the friend is inactive or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("inactive")]
    public string Inactive { get; set; } = "0";

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("new")]
    public string New { get; set; } = "0";
}

public class IgnoredAccount
{
    /// <summary>
    ///     The ID of the ignored account.
    /// </summary>
    [PHPProperty("ignored_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The name of the ignored account.
    /// </summary>
    [PHPProperty("nickname")]
    public required string Name { get; set; }
}

public class BannedAccount
{
    /// <summary>
    ///     The ID of the banned account.
    /// </summary>
    [PHPProperty("banned_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The name of the banned account.
    /// </summary>
    [PHPProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The reason for banning the account.
    ///     This is provided when using the "/banlist add {account} {reason}" command.
    /// </summary>
    [PHPProperty("reason")]
    public required string Reason { get; set; }
}

public class ClanMemberAccount
{
    /// <summary>
    ///     The ID of the clan member account.
    /// </summary>
    [PHPProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The name of the clan member account.
    /// </summary>
    [PHPProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The ID of the clan.
    /// </summary>
    [PHPProperty("clan_id")]
    public required string ClanID { get; set; }

    /// <summary>
    ///     The clan rank of the clan member account.
    ///     <br/>
    ///     None; Member; Officer; Leader
    /// </summary>
    [PHPProperty("rank")]
    public required string Rank { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     The datetime that the account joined the clan, in the format "yyyy-MM-dd HH:mm:ss".
    /// </summary>
    [PHPProperty("join_date")]
    public required string JoinDate { get; set; }

    /// <summary>
    ///     The account type of the friend.
    ///     <br/>
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; set; } = "3";
}

public class ClanMemberData
{
    /// <summary>
    ///     The ID of the clan.
    /// </summary>
    [PHPProperty("clan_id")]
    public required string ClanID { get; set; }

    /// <summary>
    ///     The name of the clan.
    /// </summary>
    [PHPProperty("name")]
    public required string ClanName { get; set; }

    /// <summary>
    ///     The tag of the clan.
    /// </summary>
    [PHPProperty("tag")]
    public required string ClanTag { get; set; }

    /// <summary>
    ///     The ID of the account which owns the clan.
    /// </summary>
    [PHPProperty("creator")]
    public required string ClanOwnerAccountID { get; set; }

    /// <summary>
    ///     The ID of the clan member account.
    /// </summary>
    [PHPProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The clan rank of the clan member account.
    ///     <br/>
    ///     None; Member; Officer; Leader
    /// </summary>
    [PHPProperty("rank")]
    public required string Rank { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("message")]
    public string Message { get; set; } = "TODO: [MESSAGE] See Whether This Does Anything";

    /// <summary>
    ///     The datetime that the account joined the clan, in the format "yyyy-MM-dd HH:mm:ss".
    /// </summary>
    [PHPProperty("join_date")]
    public required string JoinDate { get; set; }

    /// <summary>
    ///     The clan channel title.
    /// </summary>
    [PHPProperty("title")]
    public required string Title { get; set; }

    /// <summary>
    ///     Whether the account is active in the clan or not.
    ///     It is unknown what "active" means.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("active")]
    public string Active { get; set; } = "1";

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("logo")]
    public string Logo { get; set; } = "TODO: [LOGO] See Whether This Does Anything";

    /// <summary>
    ///     Whether the clan has been flagged as idle (no active users) or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("idleWarn")]
    public string ClanIsInactive { get; set; } = "0";

    /// <summary>
    ///     Unknown.
    ///     Seems to be set to "0" in all the network packet dumps.
    /// </summary>
    [PHPProperty("activeIndex")]
    public string ActiveIndex { get; set; } = "0";
}

public class ClanMemberDataError
{
    /// <summary>
    ///     This replaces the account's clan member data when the account is not part of a clan.
    /// </summary>
    [PHPProperty("error")]
    public string Error { get; set; } = "No Clan Member Found";
}

public class StoreItemData
{
    /// <summary>
    ///     Unknown.
    ///     Seems to be an empty string in all the network packet dumps.
    /// </summary>
    [PHPProperty("data")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    ///     The availability start time (in UTC seconds) of a limited-time store item (e.g. early-access hero).
    /// </summary>
    [PHPProperty("start_time")]
    public string AvailableFrom { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     The availability end time (in UTC seconds) of a limited-time store item (e.g. early-access hero).
    /// </summary>
    [PHPProperty("end_time")]
    public string AvailableUntil { get; set; } = DateTimeOffset.UtcNow.AddYears(1000).ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     Unknown.
    ///     If "score" is not set, then this property is not set either.
    ///     But if "score" is set, then this property is set to "0".
    /// </summary>
    [PHPProperty("used")]
    public int Used { get; set; } = 0;

    /// <summary>
    ///     Used for numeric counters and visual effects for specific hero avatars.
    ///     This value represents the number of hero kills achieved with those respective avatars.
    ///     <br/>
    ///     We don't currently keep track of these scores, so setting this value to "0" on every login will cause the counter to be equal to the number of hero kills each game.
    /// </summary>
    [PHPProperty("score")]
    public string Score { get; set; } = "0";
}

public class HeroList
{
    /// <summary>
    ///     The list of free heroes in the rotation, from when HoN became free-to-play.
    ///     The assigned value is the latest list of heroes sent by the legacy HoN master server before the services shut down.
    ///     <br/>
    ///     This property is obsolete.
    /// </summary>
    [PHPProperty("free")]
    public string FreeHeroes { get; set; } = "Hero_Genesis,Hero_Dorin_Tal,Hero_Adeve";
}

public class SecurityInformation
{
    /// <summary>
    ///     Used for the Tencent anti-DDoS protection component, which does network packet watermarking that gets verified by the game server proxy.
    /// </summary>
    [PHPProperty("initial_vector")]
    public string InitialVector { get; set; } = "73088db5e71cfb6d";

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("hash_code")]
    public string HashCode { get; set; } = "73088db5e71cfb6d43ae0bb4abf095dd43862200";

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("key_version")]
    public string KeyVersion { get; set; } = "3e2d";
}

public class AwardsTooltips
{
    /// <summary>
    ///     Milestones award.
    /// </summary>
    [PHPProperty("milestones")]
    public MilestonesAwardTooltip Milestones { get; set; } = new ();

    /// <summary>
    ///     Leveling award.
    /// </summary>
    [PHPProperty("leveling")]
    public LevelingAwardTooltip Leveling { get; set; } = new ();

    /// <summary>
    ///     Bloodlust award.
    /// </summary>
    [PHPProperty("bloodlust")]
    public BloodlustAwardTooltip Bloodlust { get; set; } = new ();

    /// <summary>
    ///     Annihilation award.
    /// </summary>
    [PHPProperty("annihilation")]
    public AnnihilationAwardTooltip Annihilation { get; set; } = new ();

    /// <summary>
    ///     Immortal award.
    /// </summary>
    [PHPProperty("immortal")]
    public ImmortalAwardTooltip Immortal { get; set; } = new ();

    /// <summary>
    ///     Victory award.
    /// </summary>
    [PHPProperty("victory")]
    public VictoryAwardTooltip Victory { get; set; } = new ();

    /// <summary>
    ///     Loss award.
    /// </summary>
    [PHPProperty("loss")]
    public LossAwardTooltip Loss { get; set; } = new ();

    /// <summary>
    ///     Disconnect award.
    /// </summary>
    [PHPProperty("disco")]
    public DisconnectAwardTooltip Disconnect { get; set; } = new ();

    /// <summary>
    ///     Quick match award.
    /// </summary>
    [PHPProperty("quick")]
    public QuickMatchAwardTooltip QuickMatch { get; set; } = new ();

    /// <summary>
    ///     First blood award.
    /// </summary>
    [PHPProperty("first")]
    public FirstBloodAwardTooltip FirstBlood { get; set; } = new ();

    /// <summary>
    ///     Consecutive wins award.
    /// </summary>
    [PHPProperty("consec_win")]
    public ConsecutiveWinAwardTooltip ConsecutiveWins { get; set; } = new ();

    /// <summary>
    ///     Consecutive losses award.
    /// </summary>
    [PHPProperty("consec_loss")]
    public ConsecutiveLossAwardTooltip ConsecutiveLosses { get; set; } = new ();
}

public class MilestonesAwardTooltip
{
    /// <summary>
    ///     Awarded for hero assists.
    /// </summary>
    [PHPProperty("heroassists")]
    public MilestoneAwardTooltip HeroAssists { get; set; } = new ()
    {
        AwardName = "heroassists",
        Experience = "100",
        GoblinCoins = "5",
        Modulo = "250"
    };

    /// <summary>
    ///     Awarded for hero kills.
    /// </summary>
    [PHPProperty("herokills")]
    public MilestoneAwardTooltip HeroKills { get; set; } = new ()
    {
        AwardName = "herokills",
        Experience = "100",
        GoblinCoins = "5",
        Modulo = "250"
    };

    /// <summary>
    ///     Awarded for killing heroes after taunting them.
    /// </summary>
    [PHPProperty("smackdown")]
    public MilestoneAwardTooltip Smackdown { get; set; } = new ()
    {
        AwardName = "smackdown",
        Experience = "50",
        GoblinCoins = "1",
        Modulo = "10"
    };

    /// <summary>
    ///     Awarded for placing wards.
    /// </summary>
    [PHPProperty("wards")]
    public MilestoneAwardTooltip Wards { get; set; } = new ()
    {
        AwardName = "wards",
        Experience = "100",
        GoblinCoins = "5",
        Modulo = "50"
    };

    /// <summary>
    ///     Awarded for winning matches.
    /// </summary>
    [PHPProperty("wins")]
    public MilestoneAwardTooltip Wins { get; set; } = new ()
    {
        AwardName = "wins",
        Experience = "200",
        GoblinCoins = "10",
        Modulo = "50"
    };
}

public class MilestoneAwardTooltip
{
    /// <summary>
    ///     The name of the milestone.
    /// </summary>
    [PHPProperty("aname")]
    public required string AwardName { get; set; }

    /// <summary>
    ///     The value of the milestone in experience.
    /// </summary>
    [PHPProperty("exp")]
    public required string Experience { get; set; }

    /// <summary>
    ///     The value of the milestone in goblin coins.
    /// </summary>
    [PHPProperty("gc")]
    public required string GoblinCoins { get; set; }

    /// <summary>
    ///     The modulus used to determine the frequency of reaching the milestone, e.g. "10" would mean that the milestone is reached every 10 ticks.
    /// </summary>
    [PHPProperty("modulo")]
    public required string Modulo { get; set; }
}

public class LevelingAwardTooltip
{
    /// <summary>
    ///     Awarded for reaching hero levels 2 to 5.
    /// </summary>
    [PHPProperty("2-5")]
    public int TwoToFive { get; set; } = 6;

    /// <summary>
    ///     Awarded for reaching hero levels 6 to 10.
    /// </summary>
    [PHPProperty("6-10")]
    public int SixToTen { get; set; } = 12;

    /// <summary>
    ///     Awarded for reaching hero levels 11 to 15.
    /// </summary>
    [PHPProperty("11-15")]
    public int ElevenToFifteen { get; set; } = 18;
}

public class BloodlustAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 10;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 2;
}

public class AnnihilationAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 75;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 15;

    /// <summary>
    ///     Team experience reward.
    /// </summary>
    [PHPProperty("tm_exp")]
    public int TeamExperience { get; set; } = 25;

    /// <summary>
    ///     Team goblin coins reward.
    /// </summary>
    [PHPProperty("tm_gc")]
    public int TeamGoblinCoins { get; set; } = 5;
}

public class ImmortalAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 50;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 10;

    /// <summary>
    ///     Team experience reward.
    /// </summary>
    [PHPProperty("tm_exp")]
    public int TeamExperience { get; set; } = 15;

    /// <summary>
    ///     Team goblin coins reward.
    /// </summary>
    [PHPProperty("tm_gc")]
    public int TeamGoblinCoins { get; set; } = 3;
}

public class VictoryAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 30;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 6;
}

public class LossAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 10;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 2;
}

public class DisconnectAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 0;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 0;
}

public class QuickMatchAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 0;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 2;
}

public class FirstBloodAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 20;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 4;
}

public class ConsecutiveWinAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 0;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public string GoblinCoins { get; set; } = "2-6";
}

public class ConsecutiveLossAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 0;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 1;
}

public class DataPoint
{
    /// <summary>
    ///     The ID of the account.
    /// </summary>
    [PHPProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The account type.
    ///     <br/>
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     The level of the account.
    /// </summary>
    [PHPProperty("level")]
    public required string Level { get; set; }

    /// <summary>
    ///     The experience of the account.
    /// </summary>
    [PHPProperty("level_exp")]
    public required string Experience { get; set; }

    /// <summary>
    ///     The total number of disconnects, including game modes which are not tracked on the statistics page (e.g. custom maps).
    /// </summary>
    [PHPProperty("discos")]
    public required string Disconnects { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("possible_discos")]
    public string PossibleDisconnects { get; set; } = "0";

    /// <summary>
    ///     The total number of matches played, including game modes which are not tracked on the statistics page (e.g. custom maps).
    /// </summary>
    [PHPProperty("games_played")]
    public required string MatchesPlayed { get; set; }

    /// <summary>
    ///     The number of bot matches won.
    /// </summary>
    [PHPProperty("num_bot_games_won")]
    public required string BotMatchesWon { get; set; }

    /// <summary>
    ///     The PSR (public skill rating) of the account.
    ///     PSR only applies to public matches.
    /// </summary>
    [PHPProperty("acc_pub_skill")]
    public required string PSR { get; set; }

    /// <summary>
    ///     The number of public matches won.
    /// </summary>
    [PHPProperty("acc_wins")]
    public required string PublicMatchesWon { get; set; }

    /// <summary>
    ///     The number of public matches lost.
    /// </summary>
    [PHPProperty("acc_losses")]
    public required string PublicMatchesLost { get; set; }

    /// <summary>
    ///     The number of public matches played.
    /// </summary>
    [PHPProperty("acc_games_played")]
    public required string PublicMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from public matches.
    /// </summary>
    [PHPProperty("acc_discos")]
    public required string PublicMatchDisconnects { get; set; }

    /// <summary>
    ///     The MMR (match making rating) of the account.
    ///     MMR only applies to ranked matches.
    /// </summary>
    [PHPProperty("rnk_amm_team_rating")]
    public required string MMR { get; set; }

    /// <summary>
    ///     The number of ranked matches won.
    /// </summary>
    [PHPProperty("rnk_wins")]
    public required string RankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of ranked matches lost.
    /// </summary>
    [PHPProperty("rnk_losses")]
    public required string RankedMatchesLost { get; set; }

    /// <summary>
    ///     The number of ranked matches played.
    /// </summary>
    [PHPProperty("rnk_games_played")]
    public required string RankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked matches.
    /// </summary>
    [PHPProperty("rnk_discos")]
    public required string RankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The casual MMR (match making rating) of the account.
    ///     Casual MMR only applies to casual ranked matches.
    /// </summary>
    [PHPProperty("cs_amm_team_rating")]
    public required string CasualMMR { get; set; }

    /// <summary>
    ///     The number of casual ranked matches won.
    /// </summary>
    [PHPProperty("cs_wins")]
    public required string CasualRankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of casual ranked matches lost.
    /// </summary>
    [PHPProperty("cs_losses")]
    public required string CasualRankedMatchesLost { get; set; }

    /// <summary>
    ///     The number of casual ranked matches played.
    /// </summary>
    [PHPProperty("cs_games_played")]
    public required string CasualRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from casual ranked matches.
    /// </summary>
    [PHPProperty("cs_discos")]
    public required string CasualRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The MidWars MMR (match making rating) of the account.
    ///     MidWars MMR only applies to ranked MidWars matches.
    /// </summary>
    [PHPProperty("mid_amm_team_rating")]
    public required string MidWarsMMR { get; set; }

    /// <summary>
    ///     The number of ranked MidWars matches played.
    /// </summary>
    [PHPProperty("mid_games_played")]
    public required string RankedMidWarsMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked MidWars matches.
    /// </summary>
    [PHPProperty("mid_discos")]
    public required string RankedMidWarsMatchDisconnects { get; set; }

    /// <summary>
    ///     The RiftWars MMR (match making rating) of the account.
    ///     RiftWars MMR only applies to ranked RiftWars matches.
    /// </summary>
    [PHPProperty("rift_amm_team_rating")]
    public required string RiftWarsMMR { get; set; }

    /// <summary>
    ///     The number of ranked RiftWars matches played.
    /// </summary>
    [PHPProperty("rift_games_played")]
    public required string RankedRiftWarsMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked RiftWars matches.
    /// </summary>
    [PHPProperty("rift_discos")]
    public required string RankedRiftWarsMatchDisconnects { get; set; }

    /// <summary>
    ///     The number of seasonal ranked matches played.
    /// </summary>
    [PHPProperty("cam_games_played")]
    public required int SeasonalRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from seasonal ranked matches.
    /// </summary>
    [PHPProperty("cam_discos")]
    public required int SeasonalRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The number of casual seasonal ranked matches played.
    /// </summary>
    [PHPProperty("cam_cs_games_played")]
    public required int CasualSeasonalRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from casual seasonal ranked matches.
    /// </summary>
    [PHPProperty("cam_cs_discos")]
    public required int CasualSeasonalRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("is_new")]
    public int IsNew { get; set; } = 0;
}

public class QuestSystem
{
    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("quest_status")]
    public int QuestStatus { get; set; } = 0;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("leaderboard_status")]
    public int LeaderboardStatus { get; set; } = 0;
}

public class CloudStorageInformation
{
    /// <summary>
    ///     The ID of the account.
    /// </summary>
    [PHPProperty("account_id")]
    public required string AccountID { get; set; }

    /// <summary>
    ///     Whether to automatically download the backup of the game client configuration files from the cloud or not on login.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("use_cloud")]
    public required string UseCloud { get; set; }

    /// <summary>
    ///     Whether to automatically upload the backup of the game client configuration files to the cloud or not after making changes to the settings.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("cloud_autoupload")]
    public required string AutomaticCloudUpload { get; set; }

    /// <summary>
    ///     The timestamp in "yyyy-MM-dd HH:mm:ss" format of when "cloud.zip" was last modified.
    ///     This value is extracted from "cloud.zip", which is the local copy of the backup of the game client configuration files.
    /// </summary>
    [PHPProperty("file_modify_time")]
    public required string BackupLastUpdatedTime { get; set; }
}

public class Notification
{
    /// <summary>
    ///     A pipe-separated set of notification data.
    ///     <br/>
    ///     The format is: "{SenderAccountName}|{Unknown}|{NotificationStatus}|{NotificationType}|{NotificationDisplayType}|{NotificationAction}|{NotificationTimestamp}|{NotificationID}".
    ///     <br/>
    ///     The notification status can be either 0 = Removable, 1 = Not Seen, 2 = Seen. The other data points are exemplified below.
    ///     <code>
    ///         Examples (the spaces are only added for readability, but they are not needed):
    ///             "KONGOR||23|notify_buddy_requested_added|notification_generic_action|action_friend_request|01/18 00:21 AM|5000001"
    ///             "KONGOR|| 2|notify_buddy_added          |notification_generic_info  |                     |01/18 00:22 AM|5000002"
    ///             "KONGOR|| 2|notify_buddy_requested_adder|notification_generic_info  |                     |01/18 00:23 AM|5000003"
    ///             "KONGOR|| 2|notify_replay_available     |notification_generic_info  |                     |01/18 00:24 AM|5000004"
    ///     </code>
    /// </summary>
    [PHPProperty("notification")]
    public required string PipeSeparatedNotificationData { get; set; }

    /// <summary>
    ///     The ID of the notification.
    ///     This value matches the last data point in the pipe-separated notification data set.
    /// </summary>
    [PHPProperty("notify_id")]
    public required string NotificationID { get; set; }
}
