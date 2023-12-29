namespace KONGOR.MasterServer.RequestResponseModels.SRP;

[PhpClass]
internal class SRPAuthenticationResponseStageOne(SRPAuthenticationSessionData data)
{
    /// <summary>
    ///     The SRP salt, used by the client during authentication.
    ///     Not sent in the event of an invalid account name.
    /// </summary>
    [PhpProperty("salt")]
    internal string Salt { get; init; } = data.Salt;

    /// <summary>
    ///     HoN's specific salt, used by the client in the password hashing algorithm.
    ///     Not sent in the event of an invalid account name.
    /// </summary>
    [PhpProperty("salt2")]
    internal string Salt2 { get; init; } = data.PasswordSalt;

    /// <summary>
    ///     The ephemeral SRP value "B", created by the server and sent to the client for use during SRP authentication.
    /// </summary>
    [PhpProperty("B")]
    internal string B { get; init; } = data.ServerPublicEphemeral;

    /// <summary>
    ///     Unknown property which seems to often be set to "5", for some reason.
    /// </summary>
    [PhpProperty("vested_threshold")]
    internal int VestedThreshold => 5;

    /// <summary>
    ///     Unknown property which seems to be set to "true" on a successful response or to "false" if an error occurs.
    ///     If an error occurred, use "SRPAuthenticationFailureResponse" instead.
    /// </summary>
    [PhpProperty(0)]
    internal bool Zero => true;
}
