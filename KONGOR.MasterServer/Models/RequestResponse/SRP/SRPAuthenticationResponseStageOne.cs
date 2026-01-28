namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class SRPAuthenticationResponseStageOne(SRPAuthenticationSessionDataStageOne stageOneData)
{
    /// <summary>
    ///     The SRP session salt, used by the client during authentication.
    ///     Not sent in the event of an invalid account name.
    /// </summary>
    [PHPProperty("salt")]
    public string Salt { get; init; } = stageOneData.SessionSalt;

    /// <summary>
    ///     HoN's specific salt, used by the client in the password hashing algorithm.
    ///     Not sent in the event of an invalid account name.
    /// </summary>
    [PHPProperty("salt2")]
    public string Salt2 { get; init; } = stageOneData.PasswordSalt;

    /// <summary>
    ///     The ephemeral SRP value "B", created by the server and sent to the client for use during SRP authentication.
    /// </summary>
    [PHPProperty("B")]
    public string B { get; init; } = stageOneData.ServerPublicEphemeral;

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown property which seems to be set to "true" on a successful response or to "false" if an error occurs.
    ///     If an error occurred, use "SRPAuthenticationFailureResponse" instead.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
}
