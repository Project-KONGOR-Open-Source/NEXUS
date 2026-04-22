namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Plinko;

/// <summary>
///     Integration tests for the Plinko mini-game endpoints: <c>/master/casino/</c>, <c>/master/casino/drop/</c>, and <c>/master/casino/viewchest/</c>.
/// </summary>
public sealed class MiniGameTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string PlinkoIndexRoute     = "/master/casino/";
    private const string PlinkoDropRoute      = "/master/casino/drop/";
    private const string PlinkoViewChestRoute = "/master/casino/viewchest/";

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

    [Test]
    public async Task Index_WithInvalidCookie_ReturnsStatusZero()
    {
        HttpClient client = webApplicationFactory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"] = "not-a-real-cookie"
        });

        HttpResponseMessage response = await client.PostAsync(PlinkoIndexRoute, form);

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(0);
    }

    [Test]
    public async Task Index_WithValidCookie_ReturnsEveryDeclaredResultParam()
    {
        (string cookie, _) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "plinko.index@kongor.com", "PlinkoIndex", goldCoins: 500, plinkoTickets: 100);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoIndexRoute, new Dictionary<string, string> { ["cookie"] = cookie });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(body).ContainsKey("status_code");
            await Assert.That(body).ContainsKey("tiers");
            await Assert.That(body).ContainsKey("user_tickets");
            await Assert.That(body).ContainsKey("gold_cost");
            await Assert.That(body).ContainsKey("ticket_cost");
            await Assert.That(body).ContainsKey("amount_of_products");
            await Assert.That(body).ContainsKey("last_update_time");

            await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);
            await Assert.That(Convert.ToInt32(body["gold_cost"])).IsEqualTo(JSONConfiguration.PlinkoConfiguration.GoldCost);
            await Assert.That(Convert.ToInt32(body["ticket_cost"])).IsEqualTo(JSONConfiguration.PlinkoConfiguration.TicketCost);
            await Assert.That(Convert.ToInt32(body["user_tickets"])).IsEqualTo(100);
        }
    }

    [Test]
    public async Task Drop_WithInsufficientGold_ReturnsFailureWithoutMutation()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "plinko.poor.gold@kongor.com", "PoorGold", goldCoins: 10, plinkoTickets: 0);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "gold"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(10);
            await Assert.That(user.PlinkoTickets).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Drop_WithInsufficientTickets_ReturnsFailureWithoutMutation()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "plinko.poor.ticket@kongor.com", "PoorTicket", goldCoins: 0, plinkoTickets: 5);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "tickets"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(0);
            await Assert.That(user.PlinkoTickets).IsEqualTo(5);
        }
    }

    [Test]
    [Arguments("silver")]
    [Arguments("")]
    [Arguments("gold_or_tickets")]
    public async Task Drop_WithUnsupportedCurrency_ReturnsFailureWithoutMutation(string currency)
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, $"plinko.bad.{Guid.NewGuid():N}@kongor.com", $"Bad{Guid.NewGuid().ToString("N")[..8]}", goldCoins: 1000, plinkoTickets: 1000);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = currency
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(1000);
            await Assert.That(user.PlinkoTickets).IsEqualTo(1000);
        }
    }

    [Test]
    public async Task Drop_WithSufficientGold_DeductsCostAndReturnsEveryDeclaredResultParam()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "plinko.drop.gold@kongor.com", "DropGold", goldCoins: 1000, plinkoTickets: 0);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "gold"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(body).ContainsKey("status_code");
            await Assert.That(body).ContainsKey("random_tier");
            await Assert.That(body).ContainsKey("product_id");
            await Assert.That(body).ContainsKey("product_name");
            await Assert.That(body).ContainsKey("product_path");
            await Assert.That(body).ContainsKey("product_type");
            await Assert.That(body).ContainsKey("ticket_amount");
            await Assert.That(body).ContainsKey("user_tickets");
            await Assert.That(body).ContainsKey("user_gold");
            await Assert.That(body).ContainsKey("products_exhausted");

            await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);
            await Assert.That(Convert.ToInt32(body["user_gold"])).IsEqualTo(1000 - JSONConfiguration.PlinkoConfiguration.GoldCost);
        }

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.GoldCoins).IsEqualTo(1000 - JSONConfiguration.PlinkoConfiguration.GoldCost);
    }

    [Test]
    public async Task Drop_WithTickets_DeductsTicketCost()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "plinko.drop.ticket@kongor.com", "DropTicket", goldCoins: 0, plinkoTickets: 500);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "tickets"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.PlinkoTickets).IsGreaterThanOrEqualTo(500 - JSONConfiguration.PlinkoConfiguration.TicketCost);
    }

    [Test]
    public async Task Drop_ExhaustedChestTier_PaysExhaustionReward()
    {
        (string cookie, int userID) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "plinko.exhaust@kongor.com", "Exhaust", goldCoins: 1000, plinkoTickets: 100);

        HashSet<string> seeded = CollectEveryChestTierPrefixedCode();

        await PlinkoTestsHelper.MutateUser(webApplicationFactory, userID, user =>
        {
            foreach (string code in seeded)
            {
                if (user.OwnedStoreItems.Contains(code).Equals(false))
                    user.OwnedStoreItems.Add(code);
            }
        });

        int ownedCountBeforeDrop = (await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID)).OwnedStoreItems.Count;

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "gold"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["product_id"])).IsEqualTo(-1);
            await Assert.That(Convert.ToInt32(body["products_exhausted"])).IsEqualTo(1);
            await Assert.That(Convert.ToInt32(body["ticket_amount"])).IsGreaterThan(0);
        }

        User user = await PlinkoTestsHelper.LoadUser(webApplicationFactory, userID);

        await Assert.That(user.OwnedStoreItems.Count).IsEqualTo(ownedCountBeforeDrop);
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    public async Task ViewChest_WithValidTier_ReturnsEveryDeclaredResultParam(int tierID)
    {
        (string cookie, _) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, $"viewchest{tierID}@kongor.com", $"ViewChest{tierID}", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = tierID.ToString(),
            ["target_index"]    = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(body).ContainsKey("tier_id");
            await Assert.That(body).ContainsKey("target_index");
            await Assert.That(body).ContainsKey("first_item_index");
            await Assert.That(body).ContainsKey("items_amount");
            await Assert.That(body).ContainsKey("product_names");
            await Assert.That(body).ContainsKey("product_types");
            await Assert.That(body).ContainsKey("product_paths");
            await Assert.That(body).ContainsKey("product_ids");

            await Assert.That(Convert.ToInt32(body["tier_id"])).IsEqualTo(tierID);
            await Assert.That(Convert.ToInt32(body["first_item_index"])).IsEqualTo(1);
            await Assert.That(Convert.ToInt32(body["items_amount"])).IsGreaterThan(0);
        }
    }

    [Test]
    [Arguments(0)]
    [Arguments(5)]
    [Arguments(6)]
    [Arguments(99)]
    public async Task ViewChest_WithUnsupportedTier_ReturnsStatusZero(int tierID)
    {
        (string cookie, _) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, $"badtier{tierID}@kongor.com", $"BadTier{tierID}", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = tierID.ToString(),
            ["target_index"]    = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(0);
    }

    [Test]
    public async Task ViewChest_WithTargetIndexZero_ClampsToOneWithoutThrowing()
    {
        (string cookie, _) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "clamp@kongor.com", "Clamp", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = "1",
            ["target_index"]    = "0"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["first_item_index"])).IsEqualTo(1);
    }

    [Test]
    public async Task ViewChest_WithTargetIndexBeyondTierSize_ReturnsEmptyPage()
    {
        (string cookie, _) = await PlinkoTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "beyond@kongor.com", "Beyond", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PlinkoTestsHelper.PostForm(webApplicationFactory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = "1",
            ["target_index"]    = "100000"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["items_amount"])).IsEqualTo(0);
    }

    private static HashSet<string> CollectEveryChestTierPrefixedCode()
    {
        PlinkoTierProductsConfiguration tierProducts = JSONConfiguration.PlinkoTierProductsConfiguration;
        StoreItemsConfiguration storeItems = JSONConfiguration.StoreItemsConfiguration;

        HashSet<string> codes = new ();

        foreach (int tierID in new[] { 1, 2, 3, 4 })
        {
            foreach (int productID in tierProducts.GetProductIDs(tierID))
            {
                StoreItem? storeItem = storeItems.GetByID(productID);

                if (storeItem is null)
                    continue;

                if (storeItem.IsEnabled.Equals(false) || storeItem.Purchasable.Equals(false))
                    continue;

                codes.Add(storeItem.PrefixedCode);
            }
        }

        return codes;
    }
}
