namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

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
    ///     <br />
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
    ///     <br />
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
    ///     <br />
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
