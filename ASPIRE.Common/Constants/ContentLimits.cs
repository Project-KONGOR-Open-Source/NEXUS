namespace ASPIRE.Common.Constants;

/// <summary>
///     The limits on the message of the day's items and the announcement board's announcements, in one place, so that the master server's pages, the web portal's API, and the web portal's pages agree with what the client can show.
/// </summary>
public static class ContentLimits
{
    /// <summary>
    ///     A card's picture is exactly this wide, which is the card's shape at twice its size on a 1080p screen.
    /// </summary>
    public const int PictureWidth = 800;

    /// <summary>
    ///     A card's picture is exactly this tall.
    /// </summary>
    public const int PictureHeight = 900;

    /// <summary>
    ///     The largest picture file accepted, in bytes, before it is encoded as base64.
    /// </summary>
    public const int PictureMaximumBytes = 2_097_152;

    public const int TitleMaximumLength = 64;

    public const int SubtitleMaximumLength = 96;

    public const int BodyMaximumLength = 8_000;

    /// <summary>
    ///     The most announcements, live or scheduled, that may exist at once, because the client's board has room for about eight rows and does not scroll.
    /// </summary>
    public const int UnexpiredAnnouncementsMaximum = 8;
}
