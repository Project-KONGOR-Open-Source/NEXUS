namespace DAWNBRINGER.WebPortal.UI.Contracts;

public record AnnouncementResponse(Guid ID, string Title, string Body, DateTimeOffset Published, DateTimeOffset Expires);

public record SaveAnnouncementRequest(string Title, string Body, DateTimeOffset? Published, DateTimeOffset Expires);
