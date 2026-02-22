namespace KONGOR.MasterServer.Controllers.Announcements;

/// <summary>
///     Serves HTML pages for the in-game announcements panel.
///     The HoN client's embedded web browser loads these pages when a special message is clicked.
///     Each page displays the announcement content directly, since most external sites block iframe embedding via X-Frame-Options or CSP headers.
/// </summary>
[ApiController]
public class AnnouncementsController : ControllerBase
{
    /// <summary>
    ///     Returns an HTML page displaying the announcement content.
    ///     If the announcement has a custom body, it is rendered directly.
    ///     Otherwise, a default page with the title and a link to the external URL is shown.
    ///     The game client appends a "cookie" query parameter which is ignored by this endpoint.
    /// </summary>
    [HttpGet("announcements/{index:int}", Name = "Announcement Page")]
    public IActionResult GetAnnouncementPage(int index)
    {
        List<SpecialMessage> messages = JSONConfiguration.AnnouncementsConfiguration.SpecialMessages;

        if (index < 0 || index >= messages.Count)
            return NotFound();

        SpecialMessage message = messages[index];

        string bodyContent = message.Body
            ?? $$"""<p class="link"><a href="{{message.URL}}">{{message.Title}}</a></p>""";

        string html =
        $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <title>{{message.Title}}</title>
                <style>
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    html, body { width: 100%; height: 100%; background: #1a0000; color: #e0c8a0; font-family: Arial, Helvetica, sans-serif; }
                    body { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 40px; text-align: center; }
                    h1 { font-size: 28px; color: #f0d8b0; margin-bottom: 24px; text-shadow: 1px 1px 3px #000; }
                    .body-content { font-size: 16px; line-height: 1.6; max-width: 700px; }
                    .body-content p { margin-bottom: 16px; }
                    a { color: #3abde7; text-decoration: none; font-size: 18px; }
                    a:hover { text-decoration: underline; color: #5dd0f5; }
                    .link { margin-top: 16px; }
                </style>
            </head>
            <body>
                <h1>{{message.Title}}</h1>
                <div class="body-content">
                    {{bodyContent}}
                </div>
            </body>
            </html>
        """;

        return Content(html, "text/html");
    }
}
