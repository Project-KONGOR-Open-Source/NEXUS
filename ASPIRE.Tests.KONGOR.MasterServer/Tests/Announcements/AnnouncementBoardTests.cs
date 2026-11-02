namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Announcements;

/// <summary>
///     Integration tests for the Project KONGOR Announcement Board: the list the client asks for at login, and each announcement's page.
/// </summary>
public sealed class AnnouncementBoardTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private IDatabase DistributedCache => webApplicationFactory.Services.GetRequiredService<IDatabase>();

    [Before(Test)]
    public async Task InitialiseFactory()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();
    }

    [Test]
    public async Task Live_Announcements_Are_Listed_Newest_First()
    {
        Announcement older = NewAnnouncement("Server Maintenance", DateTimeOffset.UtcNow.AddHours(-2));
        Announcement newer = NewAnnouncement("Weekend Event", DateTimeOffset.UtcNow.AddHours(-1));

        await DistributedCache.SetAnnouncement(older);
        await DistributedCache.SetAnnouncement(newer);

        List<IDictionary<object, object>> messages = await GetSpecialMessages(await SeedSession("board.order@kongor.com", "BoardOrder"));

        await Assert.That(messages.Select(message => message["title"].ToString() ?? string.Empty)).IsEquivalentTo(["Weekend Event", "Server Maintenance"], TUnit.Assertions.Enums.CollectionOrdering.Matching);
    }

    [Test]
    public async Task A_Scheduled_Announcement_Is_Not_Listed()
    {
        await DistributedCache.SetAnnouncement(NewAnnouncement("Next Week's Tournament", DateTimeOffset.UtcNow.AddDays(2)));

        List<IDictionary<object, object>> messages = await GetSpecialMessages(await SeedSession("board.scheduled@kongor.com", "BoardScheduled"));

        await Assert.That(messages).IsEmpty();
    }

    [Test]
    public async Task Each_Address_Carries_The_Identifier_And_The_Published_Time()
    {
        Announcement announcement = NewAnnouncement("Server Maintenance", DateTimeOffset.UtcNow.AddMinutes(-5));

        await DistributedCache.SetAnnouncement(announcement);

        List<IDictionary<object, object>> messages = await GetSpecialMessages(await SeedSession("board.address@kongor.com", "BoardAddress"));

        await Assert.That(messages.Single()["url"].ToString()).EndsWith($"/announcements/{announcement.ID}?published={announcement.Published.ToUnixTimeSeconds()}");
    }

    [Test]
    public async Task Editing_The_Published_Time_Changes_The_Address()
    {
        Announcement announcement = NewAnnouncement("Server Maintenance", DateTimeOffset.UtcNow.AddMinutes(-5));

        string cookie = await SeedSession("board.edit@kongor.com", "BoardEdit");

        await DistributedCache.SetAnnouncement(announcement);

        string before = (await GetSpecialMessages(cookie)).Single()["url"].ToString() ?? string.Empty;

        await DistributedCache.SetAnnouncement(announcement with { Published = announcement.Published.AddMinutes(-1) });

        string after = (await GetSpecialMessages(cookie)).Single()["url"].ToString() ?? string.Empty;

        await Assert.That(after).IsNotEqualTo(before);
    }

    [Test]
    public async Task The_List_Date_Is_The_Same_On_Every_Request()
    {
        string cookie = await SeedSession("board.date@kongor.com", "BoardDate");

        IDictionary<object, object> first = await PostSpecialMessages(cookie);

        await Task.Delay(TimeSpan.FromSeconds(1));

        IDictionary<object, object> second = await PostSpecialMessages(cookie);

        // The Client Fingerprints Each Message With This Date, So It Is The One Constant, Never Today's Date
        using (Assert.Multiple())
        {
            await Assert.That(first["date"].ToString()).IsEqualTo(global::KONGOR.MasterServer.Controllers.ClientRequester.ClientRequesterController.SpecialMessagesListDate);
            await Assert.That(second["date"].ToString()).IsEqualTo(first["date"].ToString());
        }
    }

    [Test]
    public async Task An_Announcement_Page_Is_Served_By_Identifier()
    {
        Announcement announcement = NewAnnouncement("Server Maintenance", DateTimeOffset.UtcNow.AddMinutes(-5));

        await DistributedCache.SetAnnouncement(announcement);

        using HttpClient client = webApplicationFactory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync($"/announcements/{announcement.ID}?published={announcement.Published.ToUnixTimeSeconds()}");

        string html = await response.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(html).Contains("Server Maintenance");
            await Assert.That(html).Contains(@"""Ubuntu Sans""");
            await Assert.That(html).Contains(@".body-content code, .body-content pre { font-family: ""Ubuntu Sans Mono"", monospace;");
        }
    }

    [Test]
    public async Task An_Expired_Or_Unknown_Announcement_Page_Is_Not_Found()
    {
        Announcement expiring = NewAnnouncement("Server Maintenance", DateTimeOffset.UtcNow.AddMinutes(-5)) with { Expires = DateTimeOffset.UtcNow.AddSeconds(2) };
        Announcement scheduled = NewAnnouncement("Next Week's Tournament", DateTimeOffset.UtcNow.AddDays(2));

        await DistributedCache.SetAnnouncement(expiring);
        await DistributedCache.SetAnnouncement(scheduled);

        await Task.Delay(TimeSpan.FromSeconds(3));

        using HttpClient client = webApplicationFactory.CreateClient();

        using (Assert.Multiple())
        {
            await Assert.That((await client.GetAsync($"/announcements/{expiring.ID}")).StatusCode).IsEqualTo(HttpStatusCode.NotFound);
            await Assert.That((await client.GetAsync($"/announcements/{scheduled.ID}")).StatusCode).IsEqualTo(HttpStatusCode.NotFound);
            await Assert.That((await client.GetAsync($"/announcements/{Guid.CreateVersion7()}")).StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        }
    }

    private async Task<string> SeedSession(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        string cookie = Guid.NewGuid().ToString("N");

        await DistributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return cookie;
    }

    private async Task<IDictionary<object, object>> PostSpecialMessages(string cookie)
        => await PlinkoTestsHelper.DeserialisePhpResponse(await PlinkoTestsHelper.PostForm(webApplicationFactory, "/client_requester.php?f=get_special_messages", new Dictionary<string, string> { ["cookie"] = cookie }));

    private async Task<List<IDictionary<object, object>>> GetSpecialMessages(string cookie)
    {
        object messages = (await PostSpecialMessages(cookie))["messages"];

        // A PHP Array Of Messages Reads Back As A Dictionary Keyed By Position, Or As A List
        IEnumerable<object> entries = messages switch
        {
            IDictionary<object, object> byPosition => byPosition.OrderBy(entry => Convert.ToInt64(entry.Key)).Select(entry => entry.Value),
            IEnumerable<object> list                => list,
            _                                       => []
        };

        return [.. entries.OfType<IDictionary<object, object>>()];
    }

    private static Announcement NewAnnouncement(string title, DateTimeOffset published)
        => new (Guid.CreateVersion7(), title, "<p>Details follow.</p>", published, DateTimeOffset.UtcNow.AddDays(7));
}
