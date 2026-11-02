namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.MessageOfTheDay;

/// <summary>
///     Pure-logic tests for which message of the day items show, in which order, and at which picture addresses.
/// </summary>
public sealed class MessageOfTheDayPage_Unit
{
    private static readonly DateTimeOffset Now = new (2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task Visible_Items_Are_Newest_First()
    {
        MessageOfTheDayItem oldest = NewItem("Oldest", Now.AddDays(-3), expires: null);
        MessageOfTheDayItem newest = NewItem("Newest", Now.AddHours(-1), expires: Now.AddDays(1));
        MessageOfTheDayItem middle = NewItem("Middle", Now.AddDays(-1), expires: null);

        List<MessageOfTheDayItem> visible = MessageOfTheDayPage.Visible([oldest, newest, middle], Now);

        await Assert.That(visible.Select(item => item.Title)).IsEquivalentTo(new[] { "Newest", "Middle", "Oldest" }, TUnit.Assertions.Enums.CollectionOrdering.Matching);
    }

    [Test]
    public async Task A_Future_Item_Is_Hidden()
    {
        MessageOfTheDayItem scheduled = NewItem("Scheduled", Now.AddMinutes(1), expires: Now.AddDays(1));

        await Assert.That(MessageOfTheDayPage.Visible([scheduled], Now)).IsEmpty();
    }

    [Test]
    public async Task An_Expired_Item_Is_Hidden()
    {
        MessageOfTheDayItem expired = NewItem("Expired", Now.AddDays(-2), expires: Now);

        await Assert.That(MessageOfTheDayPage.Visible([expired], Now)).IsEmpty();
    }

    [Test]
    public async Task Editing_A_Picture_Changes_Its_Address()
    {
        MessageOfTheDayItem item = NewItem("Weekend Event", Now, expires: null);
        MessageOfTheDayItem edited = item with { Picture = Convert.ToBase64String([0x89, 0x50, 0x4E, 0x47, 0x01]) };

        using (Assert.Multiple())
        {
            await Assert.That(MessageOfTheDayPage.PictureAddress(item)).StartsWith($"/motd/pictures/{item.ID}?version=");
            await Assert.That(MessageOfTheDayPage.PictureAddress(item)).IsEqualTo(MessageOfTheDayPage.PictureAddress(item with { Title = "Renamed" }));
            await Assert.That(MessageOfTheDayPage.PictureAddress(edited)).IsNotEqualTo(MessageOfTheDayPage.PictureAddress(item));
        }
    }

    private static MessageOfTheDayItem NewItem(string title, DateTimeOffset published, DateTimeOffset? expires)
        => new (Guid.CreateVersion7(), title, "a subtitle", "<p>A body.</p>", Convert.ToBase64String([0x89, 0x50, 0x4E, 0x47]), published, expires);
}
