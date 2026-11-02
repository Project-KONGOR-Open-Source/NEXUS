namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.MessageOfTheDay;

/// <summary>
///     Integration tests for the message of the day page and its pictures, which the client's home page loads from the master server.
/// </summary>
public sealed class MessageOfTheDayPageTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private IDatabase DistributedCache => webApplicationFactory.Services.GetRequiredService<IDatabase>();

    [Before(Test)]
    public async Task InitialiseFactory()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();
    }

    [Test]
    public async Task The_Page_Lists_Permanent_Items_And_Events_Newest_First()
    {
        MessageOfTheDayItem newestEvent = NewEvent("Tonight's Tournament", DateTimeOffset.UtcNow.AddMinutes(-1));
        MessageOfTheDayItem scheduledEvent = NewEvent("Next Week's Tournament", DateTimeOffset.UtcNow.AddDays(2));

        await DistributedCache.SetMessageOfTheDayEvent(newestEvent);
        await DistributedCache.SetMessageOfTheDayEvent(scheduledEvent);

        using HttpClient client = webApplicationFactory.CreateClient();

        string html = await client.GetStringAsync("/motd");

        List<string> titles = [.. EmbeddedItems(html).Select(item => item["title"]?.GetValue<string>() ?? string.Empty)];

        List<string> expected = [newestEvent.Title, .. JSONConfiguration.MessageOfTheDayConfiguration.Items.OrderByDescending(item => item.Published).Select(item => item.Title)];

        await Assert.That(titles).IsEquivalentTo(expected, TUnit.Assertions.Enums.CollectionOrdering.Matching);
    }

    [Test]
    public async Task The_Page_Ignores_Its_Language_And_Cookie()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/motd?lang=en&cookie=0123456789abcdef");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("text/html");
        }
    }

    [Test]
    public async Task The_Page_Loads_Nothing_From_Other_Sites()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        string html = await client.GetStringAsync("/motd");

        // The Articles' Own Links Live In The Embedded Data, So Everything Outside It Must Name No Other Site
        string pageWithoutData = Regex.Replace(html, @"<script type=""application/json"" id=""motd-items"">.*?</script>", string.Empty, RegexOptions.Singleline);

        await Assert.That(pageWithoutData).DoesNotContain("http");
    }

    [Test]
    public async Task A_Picture_Is_Served_Decoded_With_Its_Type()
    {
        byte[] picture = [0x89, .. "PNG"u8, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .. "IHDR"u8, 0x00, 0x00, 0x03, 0x20, 0x00, 0x00, 0x03, 0x84, 0x08, 0x06, 0x00, 0x00, 0x00];

        MessageOfTheDayItem item = NewEvent("Weekend Event", DateTimeOffset.UtcNow.AddMinutes(-1)) with { Picture = Convert.ToBase64String(picture) };

        await DistributedCache.SetMessageOfTheDayEvent(item);

        using HttpClient client = webApplicationFactory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(MessageOfTheDayPage.PictureAddress(item));

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await response.Content.ReadAsByteArrayAsync()).IsEquivalentTo(picture, TUnit.Assertions.Enums.CollectionOrdering.Matching);
            await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("image/png");
            await Assert.That(response.Headers.CacheControl?.ToString()).IsEqualTo("public, max-age=31536000, immutable");
        }
    }

    [Test]
    public async Task A_Missing_Picture_Is_Not_Found()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync($"/motd/pictures/{Guid.CreateVersion7()}?version=000000000000");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task The_Page_And_Pictures_Are_Exempt_From_Rate_Limiting()
    {
        // Every Player's Home Page Loads The Page And Its Pictures, So The Master Server's Shared Limit Must Not Refuse Them
        using (Assert.Multiple())
        {
            await Assert.That(typeof(MessageOfTheDayController).GetMethod(nameof(MessageOfTheDayController.GetPage))?.GetCustomAttribute<DisableRateLimitingAttribute>()).IsNotNull();
            await Assert.That(typeof(MessageOfTheDayController).GetMethod(nameof(MessageOfTheDayController.GetPicture))?.GetCustomAttribute<DisableRateLimitingAttribute>()).IsNotNull();
        }
    }

    private static IEnumerable<JsonNode> EmbeddedItems(string html)
    {
        Match data = Regex.Match(html, @"<script type=""application/json"" id=""motd-items"">(.*?)</script>", RegexOptions.Singleline);

        return JsonNode.Parse(data.Groups[1].Value)?.AsArray().OfType<JsonNode>() ?? [];
    }

    private static MessageOfTheDayItem NewEvent(string title, DateTimeOffset published)
        => new (Guid.CreateVersion7(), title, "sign-ups open on Discord", "<p>Bring your team.</p>", Convert.ToBase64String([0x89, 0x50, 0x4E, 0x47]), published, DateTimeOffset.UtcNow.AddDays(7));
}
