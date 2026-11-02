namespace ZORGATH.WebPortal.API.Contracts;

/// <summary>
///     An announcement on the Project KONGOR Announcement Board, as the administration endpoints list and return it.
/// </summary>
public record AnnouncementDTO(Guid ID, string Title, string Body, DateTimeOffset Published, DateTimeOffset Expires);

/// <summary>
///     The request payload for adding or editing an announcement.
///     When <see cref="Published"/> is <see langword="null"/>, a new announcement is published at once, and an edited announcement keeps its published time.
///     Changing the title or the published time makes the announcement pop up again for every account, since the client then sees it as a new one.
/// </summary>
public record SaveAnnouncementDTO(string Title, string Body, DateTimeOffset? Published, DateTimeOffset Expires);
