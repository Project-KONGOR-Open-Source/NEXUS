namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Plinko;

/// <summary>
///     Integration tests for the Plinko ticket-exchange endpoints: <c>/master/ticketexchange/</c> and <c>/master/ticketexchange/purchase/</c>.
/// </summary>
public sealed class TicketExchangeTests
{
    private const string ListRoute      = "/master/ticketexchange/";
    private const string PurchaseRoute  = "/master/ticketexchange/purchase/";

    private const int StatusBadCookie           = 50;
    private const int StatusSuccess             = 51;
    private const int StatusInsufficientTickets = 52;
    private const int StatusAlreadyOwned        = 53;
    private const int StatusInvalidItem         = 54;

    [Test]
    public async Task List_WithInvalidCookie_ReturnsStatusBadCookie()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        HttpClient client = factory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"] = "nope"
        });

        HttpResponseMessage response = await client.PostAsync(ListRoute, form);

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusBadCookie);
    }

    [Test]
    public async Task List_WithValidCookie_ReturnsSuccessAndCatalogue()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, _) = await PlinkoTestsHelper.SeedAuthenticatedSession(factory, "exchange.list@kongor.com", "ExchangeList", goldCoins: 0, plinkoTickets: 1300);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(factory, ListRoute, new Dictionary<string, string>
        {
            ["cookie"] = cookie
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusSuccess);
            await Assert.That(body).ContainsKey("items");
            await Assert.That(body).ContainsKey("user_tickets");
            await Assert.That(Convert.ToInt32(body["user_tickets"])).IsEqualTo(1300);
        }
    }

    [Test]
    public async Task Purchase_HappyPath_DeductsTicketsAndGrantsItem()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(factory, "exchange.buy@kongor.com", "ExchangeBuy", goldCoins: 0, plinkoTickets: 1300);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(factory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "1792"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusSuccess);
            await Assert.That(Convert.ToInt32(body["tickets_remaining"])).IsEqualTo(0);

            // Every Grab-Bag Field Must Be Present Even When The Purchase Is Not A Grab Bag
            await Assert.That(body).ContainsKey("grabBag");
            await Assert.That(body).ContainsKey("grabBagTheme");
            await Assert.That(body).ContainsKey("grabBagIDs");
            await Assert.That(body).ContainsKey("grabBagTypes");
            await Assert.That(body).ContainsKey("grabBagLocalPaths");
            await Assert.That(body).ContainsKey("grabBagProductNames");
        }

        User user = await PlinkoTestsHelper.LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.PlinkoTickets).IsEqualTo(0);
            await Assert.That(user.OwnedStoreItems.Any(code => code.EndsWith("Super-Taunt"))).IsTrue();
        }
    }

    [Test]
    public async Task Purchase_WithInsufficientTickets_ReturnsInsufficientStatusAndDoesNotMutate()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(factory, "exchange.cheap@kongor.com", "ExchangeCheap", goldCoins: 0, plinkoTickets: 10);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(factory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "1792"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusInsufficientTickets);

        User user = await PlinkoTestsHelper.LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.PlinkoTickets).IsEqualTo(10);
            await Assert.That(user.OwnedStoreItems.Any(code => code.EndsWith("Super-Taunt"))).IsFalse();
        }
    }

    [Test]
    public async Task Purchase_AlreadyOwned_ReturnsAlreadyOwnedStatusAndDoesNotMutate()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(factory, "exchange.dupe@kongor.com", "ExchangeDupe", goldCoins: 0, plinkoTickets: 5000);

        // Pre-Grant The Super-Taunt So The Second Purchase Is A Duplicate
        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        User preSeededUser = await databaseContext.Users.SingleAsync(queried => queried.ID == userID);

        StoreItem superTaunt = JSONConfiguration.StoreItemsConfiguration.GetByID(1792)
            ?? throw new InvalidOperationException(@"Store Item ID ""1792"" (Super-Taunt) Is Missing From The Store Catalogue");

        preSeededUser.OwnedStoreItems.Add(superTaunt.PrefixedCode);
        await databaseContext.SaveChangesAsync();

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(factory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "1792"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusAlreadyOwned);

        User user = await PlinkoTestsHelper.LoadUser(factory, userID);

        await Assert.That(user.PlinkoTickets).IsEqualTo(5000);
    }

    [Test]
    public async Task Purchase_UnknownProductID_ReturnsInvalidItem()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(factory, "exchange.unknown@kongor.com", "ExchangeUnknown", goldCoins: 0, plinkoTickets: 5000);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(factory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "99999999"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusInvalidItem);

        User user = await PlinkoTestsHelper.LoadUser(factory, userID);

        await Assert.That(user.PlinkoTickets).IsEqualTo(5000);
    }

    private static async Task<IDictionary<object, object>> DeserialisePhpResponse(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return PhpSerialization.Deserialize(body) as IDictionary<object, object>
            ?? throw new InvalidOperationException($@"Response Body Was Not A PHP Array: ""{body}""");
    }
}

/// <summary>
///     Shared helpers between Plinko test classes for seeding authenticated sessions and HTTP plumbing.
/// </summary>
internal static class PlinkoTestsHelper
{
    public static async Task<(string Cookie, int UserID)> SeedAuthenticatedSession(WebApplicationFactory<KONGORAssemblyMarker> factory, string emailAddress, string accountName, int goldCoins, int plinkoTickets)
    {
        SRPAuthenticationService service = new (factory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(queried => queried.ID == account.User.ID);

        user.GoldCoins = goldCoins;
        user.PlinkoTickets = plinkoTickets;

        await databaseContext.SaveChangesAsync();

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = factory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (cookie, user.ID);
    }

    public static async Task<User> LoadUser(WebApplicationFactory<KONGORAssemblyMarker> factory, int userID)
    {
        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        return await databaseContext.Users.SingleAsync(user => user.ID == userID);
    }

    public static async Task<HttpResponseMessage> PostForm(WebApplicationFactory<KONGORAssemblyMarker> factory, string route, Dictionary<string, string> fields)
    {
        HttpClient client = factory.CreateClient();

        FormUrlEncodedContent form = new (fields);

        return await client.PostAsync(route, form);
    }
}
