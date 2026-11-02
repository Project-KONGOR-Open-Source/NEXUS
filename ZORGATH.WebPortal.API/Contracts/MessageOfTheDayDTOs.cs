namespace ZORGATH.WebPortal.API.Contracts;

/// <summary>
///     A message of the day event, as the administration endpoints list and return it.
///     The picture is a base64-encoded PNG, JPEG, or WebP picture of the card's fixed size.
/// </summary>
public record MessageOfTheDayEventDTO(Guid ID, string Title, string Subtitle, string Body, string Picture, DateTimeOffset Published, DateTimeOffset Expires);

/// <summary>
///     The request payload for adding or editing a message of the day event.
///     When <see cref="Published"/> is <see langword="null"/>, a new event is published at once, and an edited event keeps its published time.
/// </summary>
public record SaveMessageOfTheDayEventDTO(string Title, string Subtitle, string Body, string Picture, DateTimeOffset? Published, DateTimeOffset Expires);
