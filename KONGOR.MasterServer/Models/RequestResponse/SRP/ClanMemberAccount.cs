namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

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
    ///     <br />
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
    ///     <br />
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; set; } = "3";
}
