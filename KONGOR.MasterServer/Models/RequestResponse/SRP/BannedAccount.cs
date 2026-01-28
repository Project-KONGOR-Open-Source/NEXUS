namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

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
