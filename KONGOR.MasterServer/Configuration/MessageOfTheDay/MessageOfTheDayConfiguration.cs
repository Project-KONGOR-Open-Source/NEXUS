namespace KONGOR.MasterServer.Configuration.MessageOfTheDay;

/// <summary>
///     The message of the day's permanent items, which never expire; administrators add timed events beside them through the web portal.
/// </summary>
public class MessageOfTheDayConfiguration
{
    public required List<MessageOfTheDayItem> Items { get; set; }
}
