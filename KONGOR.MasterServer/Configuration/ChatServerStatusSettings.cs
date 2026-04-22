namespace KONGOR.MasterServer.Configuration;

/// <summary>
///     Configuration for the client that polls the TRANSMUTANSTEIN chat server's <c>/health</c> endpoint.
/// </summary>
public class ChatServerStatusSettings
{
    /// <summary>
    ///     The configuration section name that this settings class binds to.
    /// </summary>
    public const string SectionName = "ChatServerStatus";

    /// <summary>
    ///     The base URL of the chat server, including the scheme and trailing slash.
    ///     The client appends the <c>/health</c> path segment when issuing its probe request.
    /// </summary>
    public required string BaseURL { get; set; }
}
