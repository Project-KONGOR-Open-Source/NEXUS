namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Messaging;

/// <summary>
///     Integration tests for the per-account message inbox: system message delivery, permanent deletion, and read tracking.
/// </summary>
public sealed class MessageInboxTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string WelcomeSubject = "Welcome To Project KONGOR !";

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Welcome_System_Message_Is_Delivered_On_First_List()
    {
        (string cookie, int accountID) = await SeedSession("inbox.welcome@kongor.com", "InboxWelcome");

        IReadOnlyList<IDictionary<object, object>> messages = await ListMessages(cookie, accountID);

        await Assert.That(messages.Any(message => message["subject"].ToString() == WelcomeSubject)).IsTrue();
    }

    [Test]
    public async Task Deleted_System_Message_Is_Not_Redelivered()
    {
        (string cookie, int accountID) = await SeedSession("inbox.delete@kongor.com", "InboxDelete");

        IReadOnlyList<IDictionary<object, object>> messages = await ListMessages(cookie, accountID);

        string crc = messages.Single(message => message["subject"].ToString() == WelcomeSubject)["crc"].ToString() ?? string.Empty;

        await PlinkoTestsHelper.PostForm(webApplicationFactory, $"/message/delete/{accountID}/{crc}", new Dictionary<string, string> { ["cookie"] = cookie });

        // The Welcome Message Must Not Reappear After The Account Has Deleted It
        IReadOnlyList<IDictionary<object, object>> messagesAfterDeletion = await ListMessages(cookie, accountID);

        await Assert.That(messagesAfterDeletion.Any(message => message["subject"].ToString() == WelcomeSubject)).IsFalse();
    }

    [Test]
    public async Task Message_Get_Returns_The_Body_And_Marks_It_Read()
    {
        (string cookie, int accountID) = await SeedSession("inbox.read@kongor.com", "InboxRead");

        IReadOnlyList<IDictionary<object, object>> messages = await ListMessages(cookie, accountID);

        string crc = messages.Single(message => message["subject"].ToString() == WelcomeSubject)["crc"].ToString() ?? string.Empty;

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, $"/message/get/{accountID}/{crc}", new Dictionary<string, string> { ["cookie"] = cookie });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToBoolean(body["success"])).IsTrue();

        // Fetching A Message Marks It As Read, Which Is Reflected In The Subsequent Manifest
        IReadOnlyList<IDictionary<object, object>> messagesAfterRead = await ListMessages(cookie, accountID);

        IDictionary<object, object> welcome = messagesAfterRead.Single(message => message["subject"].ToString() == WelcomeSubject);

        await Assert.That(Convert.ToBoolean(welcome["read"])).IsTrue();
    }

    private async Task<IReadOnlyList<IDictionary<object, object>>> ListMessages(string cookie, int accountID)
    {
        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, $"/message/list/{accountID}", new Dictionary<string, string> { ["cookie"] = cookie });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        // The Manifest Is An Integer-Keyed PHP Array, Which Deserialises To A List Rather Than A Dictionary
        if (body.TryGetValue("data", out object? data) is false || data is not System.Collections.IList entries)
            return [];

        return entries.OfType<IDictionary<object, object>>().ToList();
    }

    private async Task<(string Cookie, int AccountID)> SeedSession(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (cookie, account.ID);
    }
}
