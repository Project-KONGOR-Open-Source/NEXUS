namespace KONGOR.MasterServer.RequestResponseModels.SRP;

/// <summary>
///     Exposes the constants, properties, and methods required for Secure Remote Password protocol authentication, stage two.
/// </summary>
public class SRPAuthenticationSessionDataStageTwo
{
    public SRPAuthenticationSessionDataStageTwo(SRPAuthenticationSessionDataStageOne stageOneData, string clientProof)
    {
        ClientProof = clientProof;

        SrpParameters parameters = SrpParameters.Create<SHA256>(SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        // NOTE: HoN SRP requires a padded `g` (`parameters.Generator`) value for its final `M` calculation. 
        // The rfc5054 specification is unclear as to whether this 'should' be done or not. It is done in the Python SRP library (link below).
        // https://github.com/cocagne/pysrp/blob/4dfb111fffd671b7d97d803309fda2904e3ca1c8/srp/_pysrp.py#L205-L208
        // It's unclear if a similar change can (or should) merge this into the C# SRP library.
        parameters.Generator = parameters.Pad(parameters.Generator); // TODO: Test This !!!

        SrpServer server = new(parameters);

        try
        {
            SrpSession serverSession = server.DeriveSession
            (
                stageOneData.ServerPrivateEphemeral,
                stageOneData.ClientPublicEphemeral,
                stageOneData.SessionSalt,
                stageOneData.LoginIdentifier,
                stageOneData.Verifier,
                clientProof
            );

            ServerProof = serverSession.Proof;
        }

        catch
        {
            ServerProof = null;
        }
    }

    // TODO: Add SRP Tests

    /// <summary>
    ///     M1 : the client's proof; the server should verify this value and use it to compute M2 (the server's proof)
    /// </summary>
    public string ClientProof { get; init; }

    /// <summary>
    ///     M2 : the server's proof; the client should verify this value and use it to complete the SRP challenge exchange
    /// </summary>
    public string? ServerProof { get; init; }
}
