namespace KONGOR.MasterServer.Models.MessageOfTheDay;

/// <summary>
///     One card of the message of the day and the article it opens.
///     Permanent items come from the master server's configuration and never expire; events come from the distributed cache and expire on their own.
/// </summary>
/// <param name="ID">The item's identity, which never changes, not even when the item is edited.</param>
/// <param name="Title">The card's title, in Start Case.</param>
/// <param name="Subtitle">The card's subtitle, starting with a lowercase word unless that word is a proper noun.</param>
/// <param name="Body">The article's text, as HTML within the safe subset the web portal accepts.</param>
/// <param name="Picture">The card's picture, as base64, exactly <see cref="ContentLimits.PictureWidth"/> by <see cref="ContentLimits.PictureHeight"/> pixels.</param>
/// <param name="Published">When the item starts to show; items show newest first.</param>
/// <param name="Expires">When an event stops showing, or <see langword="null"/> for a permanent item.</param>
public record MessageOfTheDayItem(Guid ID, string Title, string Subtitle, string Body, string Picture, DateTimeOffset Published, DateTimeOffset? Expires);
