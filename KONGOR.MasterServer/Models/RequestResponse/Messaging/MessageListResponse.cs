namespace KONGOR.MasterServer.Models.RequestResponse.Messaging;

/// <summary>
///     The response payload for the message-list endpoint, containing the manifest of the account's messages.
/// </summary>
public class MessageListResponse
{
    [PHPProperty("success")]
    public bool Success { get; init; } = true;

    [PHPProperty("data")]
    public required List<MessageManifestEntry> MessageManifest { get; init; }
}
