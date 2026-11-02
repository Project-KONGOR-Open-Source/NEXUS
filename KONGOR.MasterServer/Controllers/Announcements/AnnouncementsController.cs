namespace KONGOR.MasterServer.Controllers.Announcements;

/// <summary>
///     Serves the pages of the Project KONGOR Announcement Board, which the client's embedded web browser loads when an announcement is opened.
///     Announcements are kept in the distributed cache by administrators, through the web portal, and expire on their own.
/// </summary>
[ApiController]
public class AnnouncementsController(IDatabase distributedCache) : ControllerBase
{
    private IDatabase DistributedCache { get; } = distributedCache;

    /// <summary>
    ///     Returns the page of a published, unexpired announcement, or 404 when there is none by that identity.
    ///     The address's "published" query parameter only makes the client's fingerprint change when the published time does, and the client appends a "cookie" one, so both are ignored.
    ///     Every player with an unread announcement loads its page at login, and a refused page would show blank yet still count as read, so the master server's shared rate limit does not apply to it.
    /// </summary>
    [HttpGet("announcements/{id:guid}", Name = "Announcement Page")]
    [DisableRateLimiting]
    public async Task<IActionResult> GetAnnouncementPage(Guid id)
    {
        Announcement? announcement = await DistributedCache.GetAnnouncement(id);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (announcement is null || announcement.Published > now || announcement.Expires <= now)
            return NotFound();

        // The Body Was Held To The Safe HTML Subset When It Was Saved; The Title Is Plain Text
        string title = WebUtility.HtmlEncode(announcement.Title);

        string html =
        $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <title>{{title}}</title>
                <style>
                    @font-face { font-family: "Ubuntu Sans"; src: url("/fonts/ubuntu-regular.ttf"); font-weight: 400; }
                    @font-face { font-family: "Ubuntu Sans"; src: url("/fonts/ubuntu-bold.ttf"); font-weight: 700; }
                    @font-face { font-family: "Ubuntu Sans Mono"; src: url("/fonts/ubuntu-mono-regular.ttf"); font-weight: 400; }
                    @font-face { font-family: "Ubuntu Sans Mono"; src: url("/fonts/ubuntu-mono-bold.ttf"); font-weight: 700; }
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    html, body { width: 100%; background: #1a0000; color: #e0c8a0; font-family: "Ubuntu Sans", sans-serif; }
                    html { height: 100%; }
                    body { min-height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 40px; text-align: center; }
                    h1 { font-size: 28px; color: #f0d8b0; margin-bottom: 24px; text-shadow: 1px 1px 3px #000; }
                    .body-content { font-size: 16px; line-height: 1.6; max-width: 700px; }
                    .body-content p, .body-content pre { margin-bottom: 16px; }
                    .body-content code, .body-content pre { font-family: "Ubuntu Sans Mono", monospace; font-size: 0.92em; }
                    .body-content code { padding: 0.1em 0.35em; border-radius: 4px; background: rgba(0, 0, 0, 0.35); }
                    .body-content pre { padding: 12px 16px; border-radius: 6px; background: rgba(0, 0, 0, 0.4); text-align: left; white-space: pre-wrap; overflow-wrap: anywhere; }
                    .body-content pre code { padding: 0; background: none; font-size: 1em; }
                    a { color: #3abde7; text-decoration: none; font-size: 18px; }
                    a:hover { text-decoration: underline; color: #5dd0f5; }
                    .link { margin-top: 16px; }
                </style>
            </head>
            <body>
                <h1>{{title}}</h1>
                <div class="body-content">
                    {{announcement.Body}}
                </div>
            </body>
            </html>
        """;

        // The Page Has No Script Of Its Own, So The In-Game Browser Is Told To Run None, Whatever A Body Holds
        Response.Headers.ContentSecurityPolicy = "default-src 'self'; script-src 'none'; style-src 'unsafe-inline'";

        return Content(html, "text/html");
    }
}
