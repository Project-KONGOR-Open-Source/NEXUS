namespace KONGOR.MasterServer.Models.RequestResponse.Messaging;

/// <summary>
///     The response payload for the message-get endpoint, containing a single message in full.
/// </summary>
public class MessageGetResponse
{
    [PHPProperty("success")]
    public bool Success { get; init; } = true;

    [PHPProperty("data")]
    public required MessageDetail Data { get; init; }
}
