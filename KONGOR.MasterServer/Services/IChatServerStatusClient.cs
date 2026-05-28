namespace KONGOR.MasterServer.Services;

/// <summary>
///     Polls the TRANSMUTANSTEIN chat server's <c>/health</c> endpoint and returns a normalised <see cref="ChatServerStatus"/> for callers that need to report on chat server availability without knowing the transport details.
/// </summary>
public interface IChatServerStatusClient
{
    /// <summary>
    ///     Issues a health probe against the configured chat server.
    /// </summary>
    Task<ChatServerStatus> GetStatus(CancellationToken cancellationToken = default);
}
