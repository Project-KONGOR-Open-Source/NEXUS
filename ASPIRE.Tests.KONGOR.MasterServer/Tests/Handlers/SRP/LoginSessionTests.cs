namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.SRP;

/// <summary>
///     Tests for how a login retires the earlier logins of its user in the distributed cache: the earlier login of the same account, and the logins of the other accounts which it gives, so that a user holds exactly one live login, even when the user's logins race.
/// </summary>
public sealed class LoginSessionTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private IDatabase DistributedCache => webApplicationFactory.Services.GetRequiredService<IDatabase>();

    [Before(Test)]
    public async Task InitialiseFactory()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();
    }

    [Test]
    public async Task Concurrent_Logins_Of_Two_Accounts_Of_One_User_Leave_Exactly_One_Session_Cookie()
    {
        int firstAccountID = NewAccountID();
        int secondAccountID = NewAccountID();

        List<string> cookies = [.. Enumerable.Range(0, DistributedCacheExtensions.SessionCookieSupersessionAttempts).Select(index => Guid.CreateVersion7().ToString())];

        // Half Of The Logins Are Of Each Account, And Each Gives The Other Account As The User's Other Account
        await Task.WhenAll(cookies.Select((cookie, index) => index % 2 is 0
            ? DistributedCache.SupersedeSessionCookies(firstAccountID, "First", cookie, [secondAccountID])
            : DistributedCache.SupersedeSessionCookies(secondAccountID, "Second", cookie, [firstAccountID])));

        List<string> liveCookies = [];

        foreach (string cookie in cookies)
        {
            if (await DistributedCache.GetAccountNameForSessionCookie(cookie) is not null)
                liveCookies.Add(cookie);
        }

        List<string> records = [.. new[] { await DistributedCache.GetLatestSessionCookie(firstAccountID), await DistributedCache.GetLatestSessionCookie(secondAccountID) }.OfType<string>()];

        using (Assert.Multiple())
        {
            await Assert.That(liveCookies.Count).IsEqualTo(1);
            await Assert.That(records).IsEquivalentTo(liveCookies);
        }
    }

    [Test]
    public async Task A_Login_Retires_The_Earlier_Session_Cookie_Of_The_Account()
    {
        int accountID = NewAccountID();

        string firstCookie = Guid.CreateVersion7().ToString();
        string secondCookie = Guid.CreateVersion7().ToString();

        await DistributedCache.SupersedeSessionCookies(accountID, "Account", firstCookie, []);

        List<string> retiredCookies = await DistributedCache.SupersedeSessionCookies(accountID, "Account", secondCookie, []);

        using (Assert.Multiple())
        {
            await Assert.That(retiredCookies).IsEquivalentTo([firstCookie]);
            await Assert.That(await DistributedCache.GetAccountNameForSessionCookie(firstCookie)).IsNull();
            await Assert.That(await DistributedCache.GetAccountNameForSessionCookie(secondCookie)).IsEqualTo("Account");
            await Assert.That(await DistributedCache.GetLatestSessionCookie(accountID)).IsEqualTo(secondCookie);
        }
    }

    [Test]
    public async Task A_First_Login_Retires_Nothing_And_Lives_For_The_Session_Cookie_Lifetime()
    {
        int accountID = NewAccountID();

        string cookie = Guid.CreateVersion7().ToString();

        List<string> retiredCookies = await DistributedCache.SupersedeSessionCookies(accountID, "Account", cookie, []);

        TimeSpan? cookieTimeToLive = await DistributedCache.KeyTimeToLiveAsync(DistributedCacheExtensions.ConstructAccountSessionCookieKey(cookie));
        TimeSpan? recordTimeToLive = await DistributedCache.KeyTimeToLiveAsync(DistributedCacheExtensions.ConstructAccountLatestSessionCookieKey(accountID));

        TimeSpan lifetime = DistributedCacheExtensions.SessionCookieLifetime;

        using (Assert.Multiple())
        {
            await Assert.That(retiredCookies).IsEmpty();
            await Assert.That(cookieTimeToLive > lifetime - TimeSpan.FromMinutes(1) && cookieTimeToLive <= lifetime).IsTrue();
            await Assert.That(recordTimeToLive > lifetime - TimeSpan.FromMinutes(1) && recordTimeToLive <= lifetime).IsTrue();
        }
    }

    [Test]
    public async Task A_Login_Retires_The_Session_Cookies_Of_The_Other_Given_Accounts()
    {
        int accountID = NewAccountID();
        int otherAccountID = NewAccountID();

        string otherCookie = Guid.CreateVersion7().ToString();
        string cookie = Guid.CreateVersion7().ToString();

        await DistributedCache.SupersedeSessionCookies(otherAccountID, "Other", otherCookie, [accountID]);

        List<string> retiredCookies = await DistributedCache.SupersedeSessionCookies(accountID, "Account", cookie, [otherAccountID]);

        using (Assert.Multiple())
        {
            await Assert.That(retiredCookies).IsEquivalentTo([otherCookie]);
            await Assert.That(await DistributedCache.GetAccountNameForSessionCookie(otherCookie)).IsNull();
            await Assert.That(await DistributedCache.GetLatestSessionCookie(otherAccountID)).IsNull();
            await Assert.That(await DistributedCache.GetAccountNameForSessionCookie(cookie)).IsEqualTo("Account");
        }
    }

    [Test]
    public async Task A_Login_Leaves_Accounts_Which_Are_Not_Given_Alone()
    {
        int accountID = NewAccountID();
        int givenAccountID = NewAccountID();
        int unrelatedAccountID = NewAccountID();

        string unrelatedCookie = Guid.CreateVersion7().ToString();

        await DistributedCache.SupersedeSessionCookies(unrelatedAccountID, "Unrelated", unrelatedCookie, []);

        List<string> retiredCookies = await DistributedCache.SupersedeSessionCookies(accountID, "Account", Guid.CreateVersion7().ToString(), [givenAccountID]);

        using (Assert.Multiple())
        {
            await Assert.That(retiredCookies).IsEmpty();
            await Assert.That(await DistributedCache.GetAccountNameForSessionCookie(unrelatedCookie)).IsEqualTo("Unrelated");
            await Assert.That(await DistributedCache.GetLatestSessionCookie(unrelatedAccountID)).IsEqualTo(unrelatedCookie);
        }
    }

    [Test]
    public async Task Concurrent_Logins_Of_One_Account_Leave_Exactly_One_Session_Cookie()
    {
        int accountID = NewAccountID();

        List<string> cookies = [.. Enumerable.Range(0, DistributedCacheExtensions.SessionCookieSupersessionAttempts).Select(index => Guid.CreateVersion7().ToString())];

        // Awaiting Every Login Fails The Test If Any Of Them Throws
        await Task.WhenAll(cookies.Select(cookie => DistributedCache.SupersedeSessionCookies(accountID, "Account", cookie, [])));

        List<string> liveCookies = [];

        foreach (string cookie in cookies)
        {
            if (await DistributedCache.GetAccountNameForSessionCookie(cookie) is not null)
                liveCookies.Add(cookie);
        }

        List<string> records = [.. new[] { await DistributedCache.GetLatestSessionCookie(accountID) }.OfType<string>()];

        using (Assert.Multiple())
        {
            await Assert.That(liveCookies.Count).IsEqualTo(1);
            await Assert.That(records).IsEquivalentTo(liveCookies);
        }
    }

    /// <summary>
    ///     Draws an account ID at random, which needs no account in the database, because a login's supersession only reads and writes the distributed cache.
    /// </summary>
    private static int NewAccountID() => Random.Shared.Next(1, int.MaxValue);
}
