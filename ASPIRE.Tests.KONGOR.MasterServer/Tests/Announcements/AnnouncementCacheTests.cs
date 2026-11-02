namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Announcements;

/// <summary>
///     Tests for how the announcement board's announcements are kept in the distributed cache: each lives until its expiry, under one identity, and nothing needs to clean up after it.
/// </summary>
public sealed class AnnouncementCacheTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private IDatabase DistributedCache => webApplicationFactory.Services.GetRequiredService<IDatabase>();

    [Before(Test)]
    public async Task InitialiseFactory()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();
    }

    [Test]
    public async Task A_Stored_Item_Is_Listed_Until_It_Expires()
    {
        Announcement announcement = NewAnnouncement(DateTimeOffset.UtcNow.AddSeconds(2));

        await DistributedCache.SetAnnouncement(announcement);

        List<Announcement> beforeExpiry = await DistributedCache.GetAnnouncements();

        await Task.Delay(TimeSpan.FromSeconds(3));

        List<Announcement> afterExpiry = await DistributedCache.GetAnnouncements();

        using (Assert.Multiple())
        {
            await Assert.That(beforeExpiry.Select(listed => listed.ID)).Contains(announcement.ID);
            await Assert.That(afterExpiry.Select(listed => listed.ID)).DoesNotContain(announcement.ID);
            await Assert.That(await DistributedCache.GetAnnouncement(announcement.ID)).IsNull();
        }
    }

    [Test]
    public async Task Storing_Under_The_Same_Identity_Replaces_The_Item()
    {
        Announcement announcement = NewAnnouncement(DateTimeOffset.UtcNow.AddHours(1));

        await DistributedCache.SetAnnouncement(announcement);
        await DistributedCache.SetAnnouncement(announcement with { Title = "Server Maintenance Postponed" });

        List<Announcement> listed = await DistributedCache.GetAnnouncements();

        using (Assert.Multiple())
        {
            await Assert.That(listed.Count(candidate => candidate.ID == announcement.ID)).IsEqualTo(1);
            await Assert.That((await DistributedCache.GetAnnouncement(announcement.ID))?.Title).IsEqualTo("Server Maintenance Postponed");
        }
    }

    [Test]
    public async Task A_Removed_Item_Is_Not_Listed()
    {
        Announcement announcement = NewAnnouncement(DateTimeOffset.UtcNow.AddHours(1));

        await DistributedCache.SetAnnouncement(announcement);

        bool removed = await DistributedCache.RemoveAnnouncement(announcement.ID);

        using (Assert.Multiple())
        {
            await Assert.That(removed).IsTrue();
            await Assert.That((await DistributedCache.GetAnnouncements()).Select(listed => listed.ID)).DoesNotContain(announcement.ID);
            await Assert.That(await DistributedCache.RemoveAnnouncement(announcement.ID)).IsFalse();
        }
    }

    [Test]
    public async Task An_Item_Without_A_Future_Expiry_Is_Refused()
    {
        await Assert.That(() => DistributedCache.SetAnnouncement(NewAnnouncement(DateTimeOffset.UtcNow.AddMinutes(-1)))).Throws<ArgumentException>();
    }

    [Test]
    public async Task A_Time_With_An_Offset_Is_Kept_As_The_Same_Instant()
    {
        DateTimeOffset published = DateTimeOffset.UtcNow.AddHours(-1).ToOffset(TimeSpan.FromHours(2));

        Announcement announcement = NewAnnouncement(DateTimeOffset.UtcNow.AddHours(1)) with { Published = published };

        await DistributedCache.SetAnnouncement(announcement);

        Announcement? stored = await DistributedCache.GetAnnouncement(announcement.ID);

        using (Assert.Multiple())
        {
            await Assert.That(stored?.Published.UtcTicks).IsEqualTo(published.UtcTicks);
            await Assert.That(stored?.Published <= DateTimeOffset.UtcNow).IsTrue();
        }
    }

    private static Announcement NewAnnouncement(DateTimeOffset expires)
        => new (Guid.CreateVersion7(), "Server Maintenance", "<p>The servers restart on Sunday morning.</p>", DateTimeOffset.UtcNow, expires);
}
