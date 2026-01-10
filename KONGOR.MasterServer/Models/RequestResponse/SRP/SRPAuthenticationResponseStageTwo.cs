namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

/// <summary>
///     <para>
///         The model of the response sent to the game client, following a successful SRP authentication.
///         This is by far the most complex data transfer object that the master server and the game client exchange.
///     </para>
///     <para>
///         The online tool https://www.cleancss.com/php-beautify/ was used to format the raw PHP data to a
///         humanly-readable PHP object.
///         This object, represented as string, was then deserialized to a C# object and re-serialized as JSON string.
///         Finally, the raw version of this data model was generated from the JSON string by using the "Edit > Paste
///         Special > Paste JSON As Classes" feature of Visual Studio.
///     </para>
/// </summary>
public class SRPAuthenticationResponseStageTwo
{
    /// <summary>
    ///     M2 : the server's proof; the client should verify this value and use it to complete the SRP challenge exchange
    /// </summary>
    [PhpProperty("proof")]
    public required string ServerProof { get; set; }

    /// <summary>
    ///     The ID of the main account associated with the account attempting to log in.
    ///     This ID will be the same as the ID of the account attempting to log in, when logging in with a main account.
    /// </summary>
    [PhpProperty("super_id")]
    public required string MainAccountID { get; set; }

    /// <summary>
    ///     The ID of the account attempting to log in.
    /// </summary>
    [PhpProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The Garena ID of the account attempting to log in.
    ///     This property only applies to the Garena client.
    /// </summary>
    [PhpProperty("garena_id")]
    public required string GarenaID { get; set; }

    /// <summary>
    ///     The name of the account attempting to log in.
    /// </summary>
    [PhpProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The email address of the user to which the account attempting to log in belongs.
    /// </summary>
    [PhpProperty("email")]
    public required string Email { get; set; }

    /// <summary>
    ///     The type of the account.
    ///     <br />
    ///     0 = Disabled; 1 = Demo; 2 = Server Host; 3 = Regular; 4 = Premium; 5 = Staff; 6 = Game Master; 7 = Tournament
    ///     Moderator; 8 = Tournament Caster
    /// </summary>
    [PhpProperty("account_type")]
    public required string AccountType { get; set; }

    /// <summary>
    ///     Unknown.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("trial")]
    public string Trial { get; set; } = "0";

    /// <summary>
    ///     The ID of the currently active suspension on the account.
    ///     If there is no currently active suspension on the account, then this value is "0".
    /// </summary>
    [PhpProperty("susp_id")]
    public required string SuspensionID { get; set; }

    /// <summary>
    ///     Unknown.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("prepay_only")]
    public string PrePayOnly { get; set; } = "0";

    /// <summary>
    ///     The type of the account.
    ///     <br />
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PhpProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     Whether to automatically download the backup of the game client configuration files from the cloud or not on login.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("use_cloud")]
    public required string UseCloud { get; set; }

    /// <summary>
    ///     Whether the password has expired or not.
    ///     <br />
    ///     NULL = Not Expired; 0 = Require Password Change; Any Non-Zero Positive Integer = Suggest Password Change
    /// </summary>
    [PhpProperty("pass_exp")]
    public string? PasswordExpired { get; set; } = null;

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("is_new")]
    public int IsNew { get; set; } = 0;

    /// <summary>
    ///     The authentication cookie that will be used to authorize the account's session in all subsequent requests.
    /// </summary>
    [PhpProperty("cookie")]
    public required string Cookie { get; set; }

    /// <summary>
    ///     The IP address of the game client which made the authentication request.
    /// </summary>
    [PhpProperty("ip")]
    public required string IPAddress { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("minimum_ranked_level")]
    public string MinimumRankedLevel { get; set; } = "3";

    /// <summary>
    ///     A floating point representation of the percentage at which the account is marked as a leaver.
    ///     The default value is ".05".
    /// </summary>
    [PhpProperty("leaverthreshold")]
    public required string LeaverThreshold { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("is_under_24")]
    public bool IsUnder24 { get; set; } = false;

    /// <summary>
    ///     Whether the account attempting to log in has any sub-accounts or not.
    /// </summary>
    [PhpProperty("is_subaccount")]
    public required bool HasSubAccounts { get; set; }

    /// <summary>
    ///     Whether the account attempting to log in is a sub-account or not.
    /// </summary>
    [PhpProperty("is_current_subaccount")]
    public required bool IsSubAccount { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("icb_url")]
    public required string ICBURL { get; set; }

    /// <summary>
    ///     A hash of some of the account's authentication details.
    /// </summary>
    [PhpProperty("auth_hash")]
    public required string AuthenticationHash { get; set; }

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PhpProperty("host_time")]
    public string HostTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     The IP address of the chat server.
    /// </summary>
    [PhpProperty("chat_url")]
    public required string ChatServerIPAddress { get; set; }

    /// <summary>
    ///     The port of the chat server.
    /// </summary>
    [PhpProperty("chat_port")]
    public required string ChatServerPort { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("commenting_url")]
    public string? CommentingURL { get; set; } = null;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("commenting_port")]
    public int? CommentingPort { get; set; } = null;

    /// <summary>
    ///     The list of chat channels that the account automatically connects to.
    /// </summary>
    [PhpProperty("chatrooms")]
    public required List<string> ChatChannels { get; set; }

    /// <summary>
    ///     All the accounts associated with the account attempting to log in, in registration order.
    ///     Each inner-list is composed of two elements, the account name and the account ID.
    /// </summary>
    [PhpProperty("identities")]
    public required List<List<string>> Accounts { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("cafe_id")]
    public string? CafeID { get; set; } = null;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("gca_regular")]
    public string? GCARegular { get; set; } = null;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("gca_prime")]
    public string? GCAPrime { get; set; } = null;

    /// <summary>
    ///     The amount of gold coins that the account owns.
    /// </summary>
    [PhpProperty("points")]
    public required string GoldCoins { get; set; }

    /// <summary>
    ///     The amount of silver coins that the account owns.
    /// </summary>
    [PhpProperty("mmpoints")]
    public required int SilverCoins { get; set; }

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PhpProperty("dice_tokens")]
    public int DiceTokens { get; set; } = 100;

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    ///     Alternative to permanent "Game Pass" or temporary "Game Access" products (e.g. "m.midwars.pass",
    ///     "m.midwars.access").
    ///     Legacy accounts have full access to all game modes, and so do accounts which own the "m.allmodes.pass" store item.
    /// </summary>
    [PhpProperty("game_tokens")]
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
    [PhpProperty("season_level")]
    public int SeasonLevel { get; set; } = 100;

    /// <summary>
    ///     Unused.
    ///     <br />
    ///     May have been intended as a seasonal progression system similar to "season_level" but for creep cosmetics.
    ///     For the sake of consistency with "season_level", this property is set to "100", although it most likely has no
    ///     effect.
    /// </summary>
    [PhpProperty("creep_level")]
    public int CreepLevel { get; set; } = 100;

    /// <summary>
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PhpProperty("slot_id")]
    public required string CustomIconSlotID { get; set; }

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PhpProperty("timestamp")]
    public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    ///     The current season.
    ///     The last season before the services went offline was 12.
    /// </summary>
    [PhpProperty("campaign_current_season")]
    public required string CurrentSeason { get; set; }

    /// <summary>
    ///     Unknown.
    ///     <br />
    ///     Appears to be the MMR rank thresholds.
    /// </summary>
    [PhpProperty("mmr_rank")]
    public string MMRRankThresholds { get; set; } =
        "1250,1275,1300,1330,1365,1400,1435,1470,1505,1540,1575,1610,1645,1685,1725,1765,1805,1850,1900,1950";

    /// <summary>
    ///     The time (in UTC seconds) at which the account is no longer muted.
    /// </summary>
    [PhpProperty("mute_expiration")]
    public required int MuteExpiration { get; set; }

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

    /// <summary>
    ///     The account's list of friend accounts.
    ///     The outer-dictionary needs to contain a single entry with the key being the owner account's ID.
    ///     The inner-dictionary needs to contain multiple entries, the key of each being the respective friend account's ID.
    /// </summary>
    [PhpProperty("buddy_list")]
    public required Dictionary<string, Dictionary<string, FriendAccount>> FriendAccountList { get; set; }

    /// <summary>
    ///     The account's list of ignored accounts.
    ///     This dictionary needs to contain a single entry with the key being the owner account's ID.
    /// </summary>
    [PhpProperty("ignored_list")]
    public required Dictionary<string, List<IgnoredAccount>> IgnoredAccountsList { get; set; }

    /// <summary>
    ///     The account's list of banned accounts.
    ///     This dictionary needs to contain a single entry with the key being the owner account's ID.
    /// </summary>
    [PhpProperty("banned_list")]
    public required Dictionary<string, List<BannedAccount>> BannedAccountsList { get; set; }

    /// <summary>
    ///     The list of members of the account's clan.
    ///     This dictionary needs to contain multiple entries, the key of each being the respective clan member account's ID.
    /// </summary>
    [PhpProperty("clan_roster")]
    public required Dictionary<string, ClanMemberAccount> ClanRoster { get; set; }

    /// <summary>
    ///     The account's clan membership data if the account is part of a clan, or an error message if the account is not part
    ///     of a clan.
    /// </summary>
    [PhpProperty("clan_member_info")]
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
    public required Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; set; }

    /// <summary>
    ///     The list of heroes with a non-standard-ownership (free/early-access/etc.) model.
    ///     The latest list of heroes sent by the legacy HoN master server before the services shut down is the list of free
    ///     heroes: "Hero_Genesis,Hero_Dorin_Tal,Hero_Adeve".
    ///     <br />
    ///     This property is obsolete.
    /// </summary>
    [PhpProperty("hero_list")]
    public HeroList HeroList { get; set; } = new();

    /// <summary>
    ///     Used for the Tencent anti-DDoS protection component, which does network packet watermarking that gets verified by
    ///     the game server proxy.
    /// </summary>
    [PhpProperty("sec_info")]
    public SecurityInformation SecurityInformation { get; set; } = new();

    /// <summary>
    ///     A set of static values used to generate award-centric data and trigger award-centric events.
    /// </summary>
    [PhpProperty("awards_tooltip")]
    public required AwardsTooltips AwardsTooltips { get; set; }

    /// <summary>
    ///     Account information, in the form of a list that contains a single object of string values.
    /// </summary>
    [PhpProperty("infos")]
    public required List<DataPoint> DataPoints { get; set; }

    /// <summary>
    ///     Used for the quest system, which has been disabled.
    ///     <br />
    ///     While the quest system is disabled, this dictionary contains a single element with a key of "error".
    ///     The object which is the value of this element has the values of all its properties set to "0".
    /// </summary>
    [PhpProperty("quest_system")]
    public Dictionary<string, QuestSystem> QuestSystem { get; set; } = new() { { "error", new QuestSystem() } };

    /// <summary>
    ///     The cloud storage settings of the account.
    ///     The cloud is used for backing up and restoring the game client configuration files.
    /// </summary>
    [PhpProperty("account_cloud_storage_info")]
    public required CloudStorageInformation CloudStorageInformation { get; set; }

    /// <summary>
    ///     The account's list of notifications.
    ///     This list does not include system notifications, which are handled separately.
    /// </summary>
    [PhpProperty("notifications")]
    public required List<Notification> Notifications { get; set; }
}