namespace KONGOR.MasterServer.RequestResponseModels.SRP;

public class SRPAuthenticationResponseStageOne(SRPAuthenticationSessionDataStageOne stageOneData)
{
    /// <summary>
    ///     The SRP salt, used by the client during authentication.
    ///     Not sent in the event of an invalid account name.
    /// </summary>
    [PhpProperty("salt")]
    public string Salt { get; init; } = stageOneData.Salt;

    /// <summary>
    ///     HoN's specific salt, used by the client in the password hashing algorithm.
    ///     Not sent in the event of an invalid account name.
    /// </summary>
    [PhpProperty("salt2")]
    public string Salt2 { get; init; } = stageOneData.PasswordSalt;

    /// <summary>
    ///     The ephemeral SRP value "B", created by the server and sent to the client for use during SRP authentication.
    /// </summary>
    [PhpProperty("B")]
    public string B { get; init; } = stageOneData.ServerPublicEphemeral;

    /// <summary>
    ///     Unknown property which seems to often be set to "5", for some reason.
    /// </summary>
    [PhpProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown property which seems to be set to "true" on a successful response or to "false" if an error occurs.
    ///     If an error occurred, use "SRPAuthenticationFailureResponse" instead.
    /// </summary>
    [PhpProperty(0)]
    public bool Zero => true;
}
