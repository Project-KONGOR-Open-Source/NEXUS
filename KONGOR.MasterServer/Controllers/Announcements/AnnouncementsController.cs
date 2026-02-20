namespace KONGOR.MasterServer.Controllers.Announcements;

/// <summary>
///     Serves HTML pages for the in-game announcements panel.
///     The HoN client's embedded web browser loads these pages when a special message is clicked.
///     Each page wraps the configured external URL in an iframe, allowing modern websites to be displayed within the client's browser component.
/// </summary>
[ApiController]
public class AnnouncementsController : ControllerBase
{
    /// <summary>
    ///     Returns an HTML page that embeds the announcement's external URL in an iframe.
    ///     The game client appends a "cookie" query parameter which is ignored by this endpoint.
    /// </summary>
    [HttpGet("announcements/{index:int}", Name = "Announcement Page")]
    public IActionResult GetAnnouncementPage(int index)
    {
        List<SpecialMessage> messages = JSONConfiguration.AnnouncementsConfiguration.SpecialMessages;

        if (index < 0 || index >= messages.Count)
            return NotFound();

        SpecialMessage message = messages[index];

        string html = 
        $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <title>{{message.Title}}</title>
                <style>
                    * { margin: 0; padding: 0; }
                    html, body { width: 100%; height: 100%; overflow: hidden; background: #1a0000; }
                    iframe { width: 100%; height: 100%; border: none; }
                    .fallback { display: none; color: #e0c8a0; font-family: Arial, sans-serif; text-align: centre; padding: 40px; }
                    .fallback a { color: #3abde7; text-decoration: none; font-size: 18px; }
                    .fallback a:hover { text-decoration: underline; }
                </style>
            </head>
            <body>
                <iframe src="{{message.URL}}" allowfullscreen></iframe>
                <noscript>
                    <div class="fallback" style="display: block;">
                        <p><a href="{{message.URL}}">{{message.Title}}</a></p>
                    </div>
                </noscript>
            </body>
            </html>
        """;

        return Content(html, "text/html");
    }
}
