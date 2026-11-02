namespace KONGOR.MasterServer.Models.Announcements;

/// <summary>
///     One announcement of the Project KONGOR Announcement Board, which the client lists at every login while it lives.
/// </summary>
/// <param name="ID">The announcement's identity, which never changes and is what its page's address names.</param>
/// <param name="Title">The title the board lists it by, in Start Case.</param>
/// <param name="Body">The page's text, as HTML within the safe subset the web portal accepts.</param>
/// <param name="Published">When the announcement starts to show; announcements show newest first.</param>
/// <param name="Expires">When the announcement stops showing.</param>
public record Announcement(Guid ID, string Title, string Body, DateTimeOffset Published, DateTimeOffset Expires);
