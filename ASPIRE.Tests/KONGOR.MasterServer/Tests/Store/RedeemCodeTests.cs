namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Store;

/// <summary>
///     Integration tests for the <c>request_code = 10</c> (promo code redemption) branch of the store controller at <c>/store_requester.php</c>.
///     The client's UI in <c>store.lua</c> treats redemption as successful only when <c>responseCode = 0</c> and <c>popupCode = 6</c>, and parses the <c>redeemed</c> field as a three-part comma-separated string of the form <c>"gold,silver,productID~productPath"</c>.
/// </summary>
public sealed class RedeemCodeTests
{
    private const string StoreRoute = "/store_requester.php";

    private const int RequestCodeRedeem = 10;

    private const int PopupSuccess              = 6;
    private const int PopupError                = 1;
    private const int ResponseCodeNoResponse    = 0;

    private const int ErrorCodeAlreadyUsed      = 45;
    private const int ErrorCodeInvalid          = 47;

    private const int SuperTauntProductID       = 1792;

    [Test]
    public async Task Redeem_WithCurrencyOnlyCode_GrantsCurrencyAndReturnsSuccess()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.currency@kongor.com", "RedeemCurrency", goldCoins: 100, silverCoins: 500, plinkoTickets: 10);

        await RedeemCodeTestsHelper.SeedCode(factory, "WELCOME2026", goldCoinsReward: 1000, silverCoinsReward: 5000, plinkoTicketsReward: 50, productID: null);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(factory, cookie, accountID, "WELCOME2026");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["responseCode"])).IsEqualTo(ResponseCodeNoResponse);
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupSuccess);
            await Assert.That(Convert.ToInt32(body["errorCode"])).IsEqualTo(0);
            await Assert.That(Convert.ToInt32(body["totalPoints"])).IsEqualTo(100 + 1000);
            await Assert.That(Convert.ToInt32(body["totalMMPoints"])).IsEqualTo(500 + 5000);
            await Assert.That(body["redeemed"]?.ToString()).IsEqualTo("1000,5000,");
        }

        User user = await RedeemCodeTestsHelper.LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(1100);
            await Assert.That(user.SilverCoins).IsEqualTo(5500);
            await Assert.That(user.PlinkoTickets).IsEqualTo(60);
        }
    }

    [Test]
    public async Task Redeem_WithProductOnlyCode_GrantsProductAndReturnsRedeemedWithProductSegment()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.product@kongor.com", "RedeemProduct", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(factory, "SUPERTAUNT", goldCoinsReward: 0, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: SuperTauntProductID);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(factory, cookie, accountID, "SUPERTAUNT");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["responseCode"])).IsEqualTo(ResponseCodeNoResponse);
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupSuccess);
            await Assert.That(body["redeemed"]?.ToString()).IsEqualTo($",,{SuperTauntProductID}~/ui/fe2/store/icons/taunt_super.tga");
        }

        User user = await RedeemCodeTestsHelper.LoadUser(factory, userID);

        await Assert.That(user.OwnedStoreItems.Any(code => code.EndsWith("Super-Taunt"))).IsTrue();
    }

    [Test]
    public async Task Redeem_WithFullBundle_GrantsEverythingAndEmitsAllThreeSegments()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.bundle@kongor.com", "RedeemBundle", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(factory, "MEGABUNDLE", goldCoinsReward: 250, silverCoinsReward: 1000, plinkoTicketsReward: 25, productID: SuperTauntProductID);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(factory, cookie, accountID, "MEGABUNDLE");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(body["redeemed"]?.ToString()).IsEqualTo($"250,1000,{SuperTauntProductID}~/ui/fe2/store/icons/taunt_super.tga");

        User user = await RedeemCodeTestsHelper.LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(250);
            await Assert.That(user.SilverCoins).IsEqualTo(1000);
            await Assert.That(user.PlinkoTickets).IsEqualTo(25);
            await Assert.That(user.OwnedStoreItems.Any(code => code.EndsWith("Super-Taunt"))).IsTrue();
        }
    }

    [Test]
    public async Task Redeem_IsCaseInsensitive()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int accountID, _) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.case@kongor.com", "RedeemCase", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(factory, "MIXEDCASE", goldCoinsReward: 100, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: null);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(factory, cookie, accountID, "   mixedcase   ");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupSuccess);
    }

    [Test]
    public async Task Redeem_WithUnknownCode_ReturnsInvalidErrorAndDoesNotMutate()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.unknown@kongor.com", "RedeemUnknown", goldCoins: 100, silverCoins: 500, plinkoTickets: 10);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(factory, cookie, accountID, "DEFINITELY-NOT-A-REAL-CODE");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupError);
            await Assert.That(Convert.ToInt32(body["errorCode"])).IsEqualTo(ErrorCodeInvalid);
        }

        User user = await RedeemCodeTestsHelper.LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(100);
            await Assert.That(user.SilverCoins).IsEqualTo(500);
            await Assert.That(user.PlinkoTickets).IsEqualTo(10);
        }
    }

    [Test]
    public async Task Redeem_WithBlankCode_ReturnsInvalidError()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int accountID, _) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.blank@kongor.com", "RedeemBlank", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(factory, cookie, accountID, string.Empty);

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupError);
            await Assert.That(Convert.ToInt32(body["errorCode"])).IsEqualTo(ErrorCodeInvalid);
        }
    }

    [Test]
    public async Task Redeem_WithAlreadyUsedCode_ReturnsAlreadyUsedErrorAndDoesNotDoubleGrant()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string firstCookie, int firstAccountID, int firstUserID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.first@kongor.com", "RedeemFirst", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);
        (string secondCookie, int secondAccountID, int secondUserID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.second@kongor.com", "RedeemSecond", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(factory, "SINGLESHOT", goldCoinsReward: 500, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: null);

        HttpResponseMessage firstResponse = await RedeemCodeTestsHelper.PostStore(factory, firstCookie, firstAccountID, "SINGLESHOT");
        IDictionary<object, object> firstBody = await RedeemCodeTestsHelper.DeserialisePhpResponse(firstResponse);
        await Assert.That(Convert.ToInt32(firstBody["popupCode"])).IsEqualTo(PopupSuccess);

        HttpResponseMessage secondResponse = await RedeemCodeTestsHelper.PostStore(factory, secondCookie, secondAccountID, "SINGLESHOT");
        IDictionary<object, object> secondBody = await RedeemCodeTestsHelper.DeserialisePhpResponse(secondResponse);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(secondBody["popupCode"])).IsEqualTo(PopupError);
            await Assert.That(Convert.ToInt32(secondBody["errorCode"])).IsEqualTo(ErrorCodeAlreadyUsed);
        }

        User secondUser = await RedeemCodeTestsHelper.LoadUser(factory, secondUserID);

        await Assert.That(secondUser.GoldCoins).IsEqualTo(0);
    }

    [Test]
    public async Task Redeem_WithProductCodeWhenAlreadyOwned_StillConsumesCodeButDoesNotDuplicateOwnership()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(factory, "redeem.own@kongor.com", "RedeemOwn", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();
        User preSeededUser = await databaseContext.Users.SingleAsync(queried => queried.ID == userID);
        StoreItem superTaunt = JSONConfiguration.StoreItemsConfiguration.GetByID(SuperTauntProductID)!;
        preSeededUser.OwnedStoreItems.Add(superTaunt.PrefixedCode);
        await databaseContext.SaveChangesAsync();

        await RedeemCodeTestsHelper.SeedCode(factory, "ALREADYOWNED", goldCoinsReward: 0, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: SuperTauntProductID);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(factory, cookie, accountID, "ALREADYOWNED");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupSuccess);

        User user = await RedeemCodeTestsHelper.LoadUser(factory, userID);

        await Assert.That(user.OwnedStoreItems.Count(code => code.Equals(superTaunt.PrefixedCode))).IsEqualTo(1);
    }
}

/// <summary>
///     Helpers for building authenticated sessions, seeding redeemable codes, and posting store requests from <see cref="RedeemCodeTests"/>.
/// </summary>
internal static class RedeemCodeTestsHelper
{
    public static async Task<(string Cookie, int AccountID, int UserID)> SeedAuthenticatedSession(WebApplicationFactory<KONGORAssemblyMarker> factory, string emailAddress, string accountName, int goldCoins, int silverCoins, int plinkoTickets)
    {
        SRPAuthenticationService service = new (factory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(queried => queried.ID == account.User.ID);

        user.GoldCoins = goldCoins;
        user.SilverCoins = silverCoins;
        user.PlinkoTickets = plinkoTickets;

        await databaseContext.SaveChangesAsync();

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = factory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (cookie, account.ID, user.ID);
    }

    public static async Task SeedCode(WebApplicationFactory<KONGORAssemblyMarker> factory, string code, int goldCoinsReward, int silverCoinsReward, int plinkoTicketsReward, int? productID)
    {
        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        databaseContext.RedeemableCodes.Add(new RedeemableCode
        {
            Code                = code.ToUpperInvariant(),
            GoldCoinsReward     = goldCoinsReward,
            SilverCoinsReward   = silverCoinsReward,
            PlinkoTicketsReward = plinkoTicketsReward,
            ProductID           = productID
        });

        await databaseContext.SaveChangesAsync();
    }

    public static async Task<User> LoadUser(WebApplicationFactory<KONGORAssemblyMarker> factory, int userID)
    {
        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        return await databaseContext.Users.SingleAsync(user => user.ID == userID);
    }

    public static async Task<HttpResponseMessage> PostStore(WebApplicationFactory<KONGORAssemblyMarker> factory, string cookie, int accountID, string code)
    {
        HttpClient client = factory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["account_id"]      = accountID.ToString(),
            ["request_code"]    = "10",
            ["code"]            = code,
            ["hostTime"]        = "123456"
        });

        return await client.PostAsync("/store_requester.php", form);
    }

    public static async Task<IDictionary<object, object>> DeserialisePhpResponse(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return PhpSerialization.Deserialize(body) as IDictionary<object, object>
            ?? throw new InvalidOperationException($@"Response Body Was Not A PHP Array: ""{body}""");
    }
}
