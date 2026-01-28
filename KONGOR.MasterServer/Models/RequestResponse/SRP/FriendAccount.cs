namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

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
    ///     <br />
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     Whether the referral system status of the friend is inactive or not.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("inactive")]
    public string Inactive { get; set; } = "0";

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("new")]
    public string New { get; set; } = "0";
}
