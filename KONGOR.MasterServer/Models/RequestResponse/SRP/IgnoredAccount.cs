namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

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
