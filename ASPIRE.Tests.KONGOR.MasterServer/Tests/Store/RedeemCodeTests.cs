namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Store;

/// <summary>
///     Integration tests for the <c>request_code = 10</c> (promo code redemption) branch of the store controller at <c>/store_requester.php</c>.
///     The client's UI in <c>store.lua</c> treats redemption as successful only when <c>responseCode = 0</c> and <c>popupCode = 6</c>, and parses the <c>redeemed</c> field as a three-part comma-separated string of the form <c>"gold,silver,productID~productPath"</c>.
/// </summary>
public sealed class RedeemCodeTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const int PopupSuccess              = 6;
    private const int PopupError                = 1;
    private const int ResponseCodeNoResponse    = 0;

    private const int ErrorCodeAlreadyUsed      = 45;
    private const int ErrorCodeInvalid          = 47;

    private const int SuperTauntProductID       = 1792;

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

    [Test]
    public async Task Redeem_WithCurrencyOnlyCode_GrantsCurrencyAndReturnsSuccess()
    {
        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.currency@kongor.com", "RedeemCurrency", goldCoins: 100, silverCoins: 500, plinkoTickets: 10);

        await RedeemCodeTestsHelper.SeedCode(webApplicationFactory, "WELCOME2026", goldCoinsReward: 1000, silverCoinsReward: 5000, plinkoTicketsReward: 50, productID: null);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, cookie, accountID, "WELCOME2026");

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

        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

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
        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.product@kongor.com", "RedeemProduct", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(webApplicationFactory, "SUPERTAUNT", goldCoinsReward: 0, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: SuperTauntProductID);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, cookie, accountID, "SUPERTAUNT");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["responseCode"])).IsEqualTo(ResponseCodeNoResponse);
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupSuccess);
            await Assert.That(body["redeemed"]?.ToString()).IsEqualTo($",,{SuperTauntProductID}~/ui/fe2/store/icons/taunt_super.tga");
        }

        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.OwnedStoreItems.Any(code => code.EndsWith("Super-Taunt"))).IsTrue();
    }

    [Test]
    public async Task Redeem_WithFullBundle_GrantsEverythingAndEmitsAllThreeSegments()
    {
        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.bundle@kongor.com", "RedeemBundle", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(webApplicationFactory, "MEGABUNDLE", goldCoinsReward: 250, silverCoinsReward: 1000, plinkoTicketsReward: 25, productID: SuperTauntProductID);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, cookie, accountID, "MEGABUNDLE");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(body["redeemed"]?.ToString()).IsEqualTo($"250,1000,{SuperTauntProductID}~/ui/fe2/store/icons/taunt_super.tga");

        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

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
        (string cookie, int accountID, _) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.case@kongor.com", "RedeemCase", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(webApplicationFactory, "MIXEDCASE", goldCoinsReward: 100, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: null);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, cookie, accountID, "   mixedcase   ");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupSuccess);
    }

    [Test]
    public async Task Redeem_WithUnknownCode_ReturnsInvalidErrorAndDoesNotMutate()
    {
        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.unknown@kongor.com", "RedeemUnknown", goldCoins: 100, silverCoins: 500, plinkoTickets: 10);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, cookie, accountID, "DEFINITELY-NOT-A-REAL-CODE");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupError);
            await Assert.That(Convert.ToInt32(body["errorCode"])).IsEqualTo(ErrorCodeInvalid);
        }

        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

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
        (string cookie, int accountID, _) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.blank@kongor.com", "RedeemBlank", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, cookie, accountID, string.Empty);

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
        (string firstCookie, int firstAccountID, int _) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.first@kongor.com", "RedeemFirst", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);
        (string secondCookie, int secondAccountID, int secondUserID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.second@kongor.com", "RedeemSecond", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        await RedeemCodeTestsHelper.SeedCode(webApplicationFactory, "SINGLESHOT", goldCoinsReward: 500, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: null);

        HttpResponseMessage firstResponse = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, firstCookie, firstAccountID, "SINGLESHOT");
        IDictionary<object, object> firstBody = await RedeemCodeTestsHelper.DeserialisePhpResponse(firstResponse);
        await Assert.That(Convert.ToInt32(firstBody["popupCode"])).IsEqualTo(PopupSuccess);

        HttpResponseMessage secondResponse = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, secondCookie, secondAccountID, "SINGLESHOT");
        IDictionary<object, object> secondBody = await RedeemCodeTestsHelper.DeserialisePhpResponse(secondResponse);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(secondBody["popupCode"])).IsEqualTo(PopupError);
            await Assert.That(Convert.ToInt32(secondBody["errorCode"])).IsEqualTo(ErrorCodeAlreadyUsed);
        }

        User secondUser = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, secondUserID);

        await Assert.That(secondUser.GoldCoins).IsEqualTo(0);
    }

    [Test]
    public async Task Redeem_WithProductCodeWhenAlreadyOwned_StillConsumesCodeButDoesNotDuplicateOwnership()
    {
        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "redeem.own@kongor.com", "RedeemOwn", goldCoins: 0, silverCoins: 0, plinkoTickets: 0);

        StoreItem superTaunt = JSONConfiguration.StoreItemsConfiguration.GetByID(SuperTauntProductID)!;

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            User preSeededUser = await databaseContext.Users.SingleAsync(candidate => candidate.ID == userID);

            preSeededUser.OwnedStoreItems.Add(superTaunt.PrefixedCode);

            await databaseContext.SaveChangesAsync();
        }

        await RedeemCodeTestsHelper.SeedCode(webApplicationFactory, "ALREADYOWNED", goldCoinsReward: 0, silverCoinsReward: 0, plinkoTicketsReward: 0, productID: SuperTauntProductID);

        HttpResponseMessage response = await RedeemCodeTestsHelper.PostStore(webApplicationFactory, cookie, accountID, "ALREADYOWNED");

        IDictionary<object, object> body = await RedeemCodeTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["popupCode"])).IsEqualTo(PopupSuccess);

        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.OwnedStoreItems.Count(code => code.Equals(superTaunt.PrefixedCode))).IsEqualTo(1);
    }
}
