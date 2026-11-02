namespace KONGOR.MasterServer.Controllers.MessageOfTheDay;

/// <summary>
///     Serves the message of the day, which the client's home page panel loads from the address it is given with "-webserver", and the pictures of its cards.
///     The items are the permanent ones from the master server's configuration together with the events administrators keep in the distributed cache.
/// </summary>
[ApiController]
public class MessageOfTheDayController(IDatabase distributedCache) : ControllerBase
{
    private IDatabase DistributedCache { get; } = distributedCache;

    /// <summary>
    ///     Returns the message of the day page with every published, unexpired item, newest first.
    ///     The client appends "lang" and "cookie" query parameters, which are ignored, because every item is in English.
    ///     Every player's home page loads this page, so the master server's shared rate limit does not apply to it.
    /// </summary>
    [HttpGet("motd", Name = "Message Of The Day Page")]
    [DisableRateLimiting]
    public async Task<IActionResult> GetPage()
    {
        List<MessageOfTheDayItem> items = MessageOfTheDayPage.Visible([.. JSONConfiguration.MessageOfTheDayConfiguration.Items, .. await DistributedCache.GetMessageOfTheDayEvents()], DateTimeOffset.UtcNow);

        return Content(MessageOfTheDayPage.Render(items), "text/html");
    }

    /// <summary>
    ///     Returns an item's picture, decoded from its stored base64, with the media type its header names.
    ///     The address carries a version of the picture, so a changed picture gets a new address and this one can be cached for good.
    ///     Every player's home page loads the pictures, so the master server's shared rate limit does not apply to them.
    /// </summary>
    [HttpGet("motd/pictures/{id:guid}", Name = "Message Of The Day Picture")]
    [DisableRateLimiting]
    public async Task<IActionResult> GetPicture(Guid id)
    {
        MessageOfTheDayItem? item = JSONConfiguration.MessageOfTheDayConfiguration.Items.SingleOrDefault(candidate => candidate.ID == id)
            ?? await DistributedCache.GetMessageOfTheDayEvent(id);

        if (item is null)
            return NotFound();

        byte[] picture;

        try
        {
            picture = Convert.FromBase64String(item.Picture);
        }

        catch (FormatException)
        {
            return NotFound();
        }

        // Only A Picture Whose Header Names A Known Type Is Served, So Nothing Else Can Be Passed Off As One
        if (PictureFormat.Read(picture) is not { } information)
            return NotFound();

        Response.Headers.CacheControl = "public, max-age=31536000, immutable";

        return File(picture, information.ContentType);
    }
}
