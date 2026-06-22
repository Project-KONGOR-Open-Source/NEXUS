namespace KONGOR.MasterServer.Models.RequestResponse.Messaging;

/// <summary>
///     The response payload for the message-delete endpoint, indicating whether the message was deleted.
/// </summary>
public class MessageDeleteResponse
{
    [PHPProperty("success")]
    public required bool Success { get; init; }
}
