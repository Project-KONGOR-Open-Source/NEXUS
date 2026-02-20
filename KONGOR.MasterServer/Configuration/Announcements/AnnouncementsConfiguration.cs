namespace KONGOR.MasterServer.Configuration.Announcements;

/// <summary>
///     Configuration for in-game announcements displayed to players via the special messages system.
///     Messages appear in the HoN Event notification panel and open a URL in the client's embedded web browser when clicked.
/// </summary>
public class AnnouncementsConfiguration
{
    public required List<SpecialMessage> SpecialMessages { get; set; }
}

/// <summary>
///     A special message to be displayed in the client's notification panel.
///     The client generates an MD5 hash from the title, URL, and date to track which messages have been seen.
/// </summary>
public class SpecialMessage
{
    public required string Title { get; set; }

    public required string URL { get; set; }
}
