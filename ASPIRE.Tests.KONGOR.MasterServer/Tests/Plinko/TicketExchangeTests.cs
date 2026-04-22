namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Plinko;

/// <summary>
///     Integration tests for the Plinko ticket-exchange endpoints: <c>/master/ticketexchange/</c> and <c>/master/ticketexchange/purchase/</c>.
/// </summary>
public sealed class TicketExchangeTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string ListRoute      = "/master/ticketexchange/";
    private const string PurchaseRoute  = "/master/ticketexchange/purchase/";

    private const int StatusBadCookie           = 50;
    private const int StatusSuccess             = 51;
    private const int StatusInsufficientTickets = 52;
    private const int StatusAlreadyOwned        = 53;
    private const int StatusInvalidItem         = 54;

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

    [Test]
    public async Task List_WithInvalidCookie_ReturnsStatusBadCookie()
    {
        HttpClient client = webApplicationFactory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"] = "nope"
        });

        HttpResponseMessage response = await client.PostAsync(ListRoute, form);

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusBadCookie);
    }

    [Test]
    public async Task List_WithValidCookie_ReturnsSuccessAndCatalogue()
    {
        (string cookie, _) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "exchange.list@kongor.com", "ExchangeList", goldCoins: 0, plinkoTickets: 1300);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, ListRoute, new Dictionary<string, string>
        {
            ["cookie"] = cookie
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

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
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "exchange.buy@kongor.com", "ExchangeBuy", goldCoins: 0, plinkoTickets: 1300);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "1792"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusSuccess);
            await Assert.That(Convert.ToInt32(body["tickets_remaining"])).IsEqualTo(0);

            await Assert.That(body).ContainsKey("grabBag");
            await Assert.That(body).ContainsKey("grabBagTheme");
            await Assert.That(body).ContainsKey("grabBagIDs");
            await Assert.That(body).ContainsKey("grabBagTypes");
            await Assert.That(body).ContainsKey("grabBagLocalPaths");
            await Assert.That(body).ContainsKey("grabBagProductNames");
        }

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.PlinkoTickets).IsEqualTo(0);
            await Assert.That(user.OwnedStoreItems.Any(code => code.EndsWith("Super-Taunt"))).IsTrue();
        }
    }

    [Test]
    public async Task Purchase_WithInsufficientTickets_ReturnsInsufficientStatusAndDoesNotMutate()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "exchange.cheap@kongor.com", "ExchangeCheap", goldCoins: 0, plinkoTickets: 10);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "1792"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusInsufficientTickets);

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.PlinkoTickets).IsEqualTo(10);
            await Assert.That(user.OwnedStoreItems.Any(code => code.EndsWith("Super-Taunt"))).IsFalse();
        }
    }

    [Test]
    public async Task Purchase_AlreadyOwned_ReturnsAlreadyOwnedStatusAndDoesNotMutate()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "exchange.dupe@kongor.com", "ExchangeDupe", goldCoins: 0, plinkoTickets: 5000);

        StoreItem superTaunt = JSONConfiguration.StoreItemsConfiguration.GetByID(1792)
            ?? throw new InvalidOperationException(@"Store Item ID ""1792"" (Super-Taunt) Is Missing From The Store Catalogue");

        await PlinkoTestsHelper.MutateUser(webApplicationFactory, userID, user => user.OwnedStoreItems.Add(superTaunt.PrefixedCode));

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "1792"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusAlreadyOwned);

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.PlinkoTickets).IsEqualTo(5000);
    }

    [Test]
    public async Task Purchase_UnknownProductID_ReturnsInvalidItem()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "exchange.unknown@kongor.com", "ExchangeUnknown", goldCoins: 0, plinkoTickets: 5000);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PurchaseRoute, new Dictionary<string, string>
        {
            ["cookie"]  = cookie,
            ["id"]      = "99999999"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(StatusInvalidItem);

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.PlinkoTickets).IsEqualTo(5000);
    }
}
