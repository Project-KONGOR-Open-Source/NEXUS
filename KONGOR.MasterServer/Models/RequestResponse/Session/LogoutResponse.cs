namespace KONGOR.MasterServer.Models.RequestResponse.Session;

/// <summary>
///     Response payload returned to the game client after a successful logout request.
///     The client fires the logout request with <c>SetReleaseOnCompletion(true)</c> and does not read the response, but the response format is retained for protocol fidelity.
/// </summary>
public class LogoutResponse
{
    /// <summary>
    ///     A string denoting the client's disconnect status.
    ///     Hard-coded to "OK" because, in the original PHP implementation, the server would simply not reply in the event of an error.
    /// </summary>
    [PHPProperty("client_disco")]
    public string ClientDisconnectStatus => "OK";

    /// <summary>
    ///     The vested threshold value.
    ///     This is always set to 5, matching the original PHP implementation.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Boolean flag indicating a successful response.
    ///     Set to TRUE on success; in the event of an error, a different response type would be used instead.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
}
