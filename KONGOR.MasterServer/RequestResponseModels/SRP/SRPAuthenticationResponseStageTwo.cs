
    [PhpProperty("buddy_list")]
    public required Dictionary<int, Dictionary<int, Buddy>> BuddyList { get; set; }
public class Buddy
{
    /// <summary>
    ///     The account ID of the friend.
    /// </summary>
    [PhpProperty("buddy_id")]
    public required string AccountID { get; set; }

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
    ///     The account name of the friend.
    /// </summary>
    [PhpProperty("nickname")]
    public required string AccountName { get; set; }

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
    public string? New { get; set; } = "0";
}
