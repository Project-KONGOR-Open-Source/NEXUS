namespace KONGOR.MasterServer.RequestResponseModels.SRP;

public class SRPAuthenticationResponseStageTwo
{


    /// <summary>
    ///     The account's list of friend accounts.
    ///     The outer-dictionary needs to contain a single entry with the key being the owner account's ID.
    ///     The inner-dictionary needs to contain multiple entries, the key of each being the respective friend account's ID.
    /// </summary>
    [PhpProperty("buddy_list")]
    public required Dictionary<int, Dictionary<int, FriendAccount>> FriendAccountList { get; set; }

    /// <summary>
    ///     The account's list of ignored accounts.
    ///     This dictionary needs to contain a single entry with the key being the owner account's ID.
    /// </summary>
    [PhpProperty("ignored_list")]
    public required Dictionary<int, List<IgnoredAccount>> IgnoredAccountsList { get; set; }

    /// <summary>
    ///     The account's list of banned accounts.
    ///     This dictionary needs to contain a single entry with the key being the owner account's ID.
    /// </summary>
    [PhpProperty("banned_list")]
    public required Dictionary<int, List<BannedAccount>> BannedAccountsList { get; set; }

    /// <summary>
    ///     The list of members of the account's clan.
    ///     This dictionary needs to contain multiple entries, the key of each being the respective clan member account's ID.
    /// </summary>
    [PhpProperty("clan_roster")]
    public required Dictionary<int, ClanMemberAccount> ClanRoster { get; set; }

    /// <summary>
    ///     The account's clan membership data if the account is part of a clan, or an error message if the account is not part of a clan.
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
}



public class FriendAccount
{
    /// <summary>
    ///     The account ID of the friend.
    /// </summary>
    [PhpProperty("buddy_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The account name of the friend.
    /// </summary>
    [PhpProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The name of the friend group that the friend is in.
    ///     A friend list can have multiple friend lists.
    ///     Additionally, a friend does not need to be in a group.
    /// </summary>
    [PhpProperty("group")]
    public string Group { get; set; } = string.Empty;

    /// <summary>
    ///     The clan tag of the friend.
    /// </summary>
    [PhpProperty("clan_tag")]
    public string? ClanTag { get; set; } = default;

    /// <summary>
    ///     Unknown.
    ///     Seems to be set to "2" in all the network packet dumps.
    /// </summary>
    [PhpProperty("status")]
    public string Status { get; set; } = "2";

    /// <summary>
    ///     The account type of the friend.
    ///     <br/>
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PhpProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     Whether the referral system status of the friend is inactive or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("inactive")]
    public string Inactive { get; set; } = "0";

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("new")]
    public string New { get; set; } = "0";
}

public class IgnoredAccount
{
    /// <summary>
    ///     The ID of the ignored account.
    /// </summary>
    [PhpProperty("ignored_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The name of the ignored account.
    /// </summary>
    [PhpProperty("nickname")]
    public required string Name { get; set; }
}

public class BannedAccount
{
    /// <summary>
    ///     The ID of the banned account.
    /// </summary>
    [PhpProperty("banned_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The name of the banned account.
    /// </summary>
    [PhpProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The reason for banning the account.
    ///     This is provided when using the "/banlist add {account} {reason}" command.
    /// </summary>
    [PhpProperty("reason")]
    public required string Reason { get; set; }
}

public class ClanMemberAccount
{
    /// <summary>
    ///     The ID of the clan member account.
    /// </summary>
    [PhpProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The name of the clan member account.
    /// </summary>
    [PhpProperty("nickname")]
    public required string Name { get; set; }

    /// <summary>
    ///     The ID of the clan.
    /// </summary>
    [PhpProperty("clan_id")]
    public required string ClanID { get; set; }

    /// <summary>
    ///     The clan rank of the clan member account.
    ///     <br/>
    ///     None; Member; Officer; Leader
    /// </summary>
    [PhpProperty("rank")]
    public required string Rank { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     The datetime that the account joined the clan, in the format "yyyy-MM-dd HH:mm:ss".
    /// </summary>
    [PhpProperty("join_date")]
    public required string JoinDate { get; set; }

    /// <summary>
    ///     The account type of the friend.
    ///     <br/>
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PhpProperty("standing")]
    public string Standing { get; set; } = "3";
}

public class ClanMemberData
{
    /// <summary>
    ///     The ID of the clan.
    /// </summary>
    [PhpProperty("clan_id")]
    public required string ClanID { get; set; }

    /// <summary>
    ///     The name of the clan.
    /// </summary>
    [PhpProperty("name")]
    public required string ClanName { get; set; }

    /// <summary>
    ///     The tag of the clan.
    /// </summary>
    [PhpProperty("tag")]
    public required string ClanTag { get; set; }

    /// <summary>
    ///     The ID of the account which owns the clan.
    /// </summary>
    [PhpProperty("creator")]
    public required string ClanOwnerAccountID { get; set; }

    /// <summary>
    ///     The ID of the clan member account.
    /// </summary>
    [PhpProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The clan rank of the clan member account.
    ///     <br/>
    ///     None; Member; Officer; Leader
    /// </summary>
    [PhpProperty("rank")]
    public required string Rank { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("message")]
    public string Message { get; set; } = "TODO: [MESSAGE] See Whether This Does Anything";

    /// <summary>
    ///     The datetime that the account joined the clan, in the format "yyyy-MM-dd HH:mm:ss".
    /// </summary>
    [PhpProperty("join_date")]
    public required string JoinDate { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("title")]
    public string Title { get; set; } = "TODO: [TITLE] See Whether This Does Anything";

    /// <summary>
    ///     Whether the account is active in the clan or not.
    ///     It is unknown what "active" means.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("active")]
    public string Active { get; set; } = "1";

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("logo")]
    public string Logo { get; set; } = "TODO: [LOGO] See Whether This Does Anything";

    /// <summary>
    ///     Whether the clan has been flagged as idle (no active users) or not.
    ///     <br/>
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("idleWarn")]
    public string ClanIsInactive { get; set; } = "0";

    /// <summary>
    ///     Unknown.
    ///     Seems to be set to "0" in all the network packet dumps.
    /// </summary>
    [PhpProperty("activeIndex")]
    public string ActiveIndex { get; set; } = "0";
}

public class ClanMemberDataError
{
    /// <summary>
    ///     This replaces the account's clan member data when the account is not part of a clan.
    /// </summary>
    [PhpProperty("error")]
    public string Error { get; set; } = "No Clan Member Found";
}

public class StoreItemData
{
    /// <summary>
    ///     Unknown.
    ///     Seems to be an empty string in all the network packet dumps.
    /// </summary>
    [PhpProperty("data")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    ///     The availability start time (in UTC seconds) of a limited-time store item (e.g. early-access hero).
    /// </summary>
    [PhpProperty("start_time")]
    public string AvailableFrom { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     The availability end time (in UTC seconds) of a limited-time store item (e.g. early-access hero).
    /// </summary>
    [PhpProperty("end_time")]
    public string AvailableUntil { get; set; } = DateTimeOffset.UtcNow.AddYears(1000).ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     Unknown.
    ///     If "score" is not set, then this property is not set either.
    ///     But if "score" is set, then this property is set to "0".
    /// </summary>
    [PhpProperty("used")]
    public int Used { get; set; } = 0;

    /// <summary>
    ///     Used for numeric counters and visual effects for specific hero avatars.
    ///     This value represents the number of hero kills achieved with those respective avatars.
    ///     <br/>
    ///     We don't currently keep track of these scores, so setting this value to "0" on every login will cause the counter to be equal to the number of hero kills each game.
    /// </summary>
    [PhpProperty("score")]
    public string Score { get; set; } = "0";
}
