namespace ZORGATH.WebPortal.API.Contracts;

public record HostAccountAuthorisationTokenDTO(Guid Token, DateTimeOffset ExpiresAt);

/// <summary>
///     The request payload for broadcasting a system message to every account's inbox.
///     Only <see cref="Subject"/> and <see cref="Body"/> are required; the remaining fields are optional metadata rendered by the in-game message panel.
/// </summary>
public record BroadcastSystemMessageDTO(string Subject, string Body, string? Subtitle, string? BodyTitle, string? Footer);

/// <summary>
///     The result of a system message broadcast, reporting how many accounts received the message.
/// </summary>
public record BroadcastSystemMessageResultDTO(int AccountsMessaged);
