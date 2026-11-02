namespace DAWNBRINGER.WebPortal.UI.Contracts;

public record MessageOfTheDayEventResponse(Guid ID, string Title, string Subtitle, string Body, string Picture, DateTimeOffset Published, DateTimeOffset Expires);

public record SaveMessageOfTheDayEventRequest(string Title, string Subtitle, string Body, string Picture, DateTimeOffset? Published, DateTimeOffset Expires);
