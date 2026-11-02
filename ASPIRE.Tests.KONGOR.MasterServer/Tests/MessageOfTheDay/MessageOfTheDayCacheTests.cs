namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.MessageOfTheDay;

/// <summary>
///     Tests for how the message of the day's events are kept in the distributed cache: each lives until its expiry, under one identity, and nothing needs to clean up after it.
/// </summary>
public sealed class MessageOfTheDayCacheTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
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
        MessageOfTheDayItem item = NewEvent(DateTimeOffset.UtcNow.AddSeconds(2));

        await DistributedCache.SetMessageOfTheDayEvent(item);

        List<MessageOfTheDayItem> beforeExpiry = await DistributedCache.GetMessageOfTheDayEvents();

        await Task.Delay(TimeSpan.FromSeconds(3));

        List<MessageOfTheDayItem> afterExpiry = await DistributedCache.GetMessageOfTheDayEvents();

        using (Assert.Multiple())
        {
            await Assert.That(beforeExpiry.Select(listed => listed.ID)).Contains(item.ID);
            await Assert.That(afterExpiry.Select(listed => listed.ID)).DoesNotContain(item.ID);
            await Assert.That(await DistributedCache.GetMessageOfTheDayEvent(item.ID)).IsNull();
        }
    }

    [Test]
    public async Task Storing_Under_The_Same_Identity_Replaces_The_Item()
    {
        MessageOfTheDayItem item = NewEvent(DateTimeOffset.UtcNow.AddHours(1));

        await DistributedCache.SetMessageOfTheDayEvent(item);
        await DistributedCache.SetMessageOfTheDayEvent(item with { Title = "Weekend Event Extended" });

        List<MessageOfTheDayItem> listed = await DistributedCache.GetMessageOfTheDayEvents();

        using (Assert.Multiple())
        {
            await Assert.That(listed.Count(candidate => candidate.ID == item.ID)).IsEqualTo(1);
            await Assert.That((await DistributedCache.GetMessageOfTheDayEvent(item.ID))?.Title).IsEqualTo("Weekend Event Extended");
        }
    }

    [Test]
    public async Task A_Removed_Item_Is_Not_Listed()
    {
        MessageOfTheDayItem item = NewEvent(DateTimeOffset.UtcNow.AddHours(1));

        await DistributedCache.SetMessageOfTheDayEvent(item);

        bool removed = await DistributedCache.RemoveMessageOfTheDayEvent(item.ID);

        using (Assert.Multiple())
        {
            await Assert.That(removed).IsTrue();
            await Assert.That((await DistributedCache.GetMessageOfTheDayEvents()).Select(listed => listed.ID)).DoesNotContain(item.ID);
            await Assert.That(await DistributedCache.RemoveMessageOfTheDayEvent(item.ID)).IsFalse();
        }
    }

    [Test]
    public async Task An_Item_Without_A_Future_Expiry_Is_Refused()
    {
        using (Assert.Multiple())
        {
            await Assert.That(() => DistributedCache.SetMessageOfTheDayEvent(NewEvent(DateTimeOffset.UtcNow.AddMinutes(-1)))).Throws<ArgumentException>();
            await Assert.That(() => DistributedCache.SetMessageOfTheDayEvent(NewEvent(DateTimeOffset.UtcNow.AddHours(1)) with { Expires = null })).Throws<ArgumentException>();
        }
    }

    [Test]
    public async Task A_Time_With_An_Offset_Is_Kept_As_The_Same_Instant()
    {
        DateTimeOffset published = DateTimeOffset.UtcNow.AddHours(-1).ToOffset(TimeSpan.FromHours(2));

        MessageOfTheDayItem item = NewEvent(DateTimeOffset.UtcNow.AddHours(1)) with { Published = published };

        await DistributedCache.SetMessageOfTheDayEvent(item);

        MessageOfTheDayItem? stored = await DistributedCache.GetMessageOfTheDayEvent(item.ID);

        using (Assert.Multiple())
        {
            await Assert.That(stored?.Published.UtcTicks).IsEqualTo(published.UtcTicks);
            await Assert.That(stored?.Published <= DateTimeOffset.UtcNow).IsTrue();
        }
    }

    private static MessageOfTheDayItem NewEvent(DateTimeOffset expires)
        => new (Guid.CreateVersion7(), "Weekend Event", "double experience, Saturday and Sunday", "<p>All matches give double experience.</p>", Convert.ToBase64String([0x89, 0x50, 0x4E, 0x47]), DateTimeOffset.UtcNow, expires);
}
