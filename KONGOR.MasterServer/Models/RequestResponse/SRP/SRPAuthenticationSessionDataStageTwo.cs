namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

/// <summary>
///     Exposes the constants, properties, and methods required for Secure Remote Password protocol authentication, stage two.
/// </summary>
public class SRPAuthenticationSessionDataStageTwo
{
    public SRPAuthenticationSessionDataStageTwo(SRPAuthenticationSessionDataStageOne stageOneData, string clientProof)
    {
        ClientProof = clientProof;

        SrpParameters parameters = SrpParameters.Create<SHA256>(SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        // HoN SRP requires a padded "g" (multiplicative group generator) value for its final "M2" (server proof) calculation. 
        // The RFC5054 specification is unclear on whether this should be done or not.
        // It is done by default in the Python "cocagne/pysrp" library which Anton Romanov used, but not in the C# "secure-remote-password/srp.net" library which this solution uses.
        // cocagne/pysrp : https://github.com/cocagne/pysrp/blob/0414166e9dba63c2677414ace2673ccc24208d23/srp/_pysrp.py#L205-L208
        // secure-remote-password/srp.net : https://github.com/secure-remote-password/srp.net/blob/176098e90501659990b12e8ac086018d47f23ccb/src/srp/SrpParameters.cs#L29
        parameters.Generator = parameters.Pad(parameters.Generator);

        SrpServer server = new (parameters);

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
