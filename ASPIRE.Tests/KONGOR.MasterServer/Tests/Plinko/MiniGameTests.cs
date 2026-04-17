namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Plinko;

/// <summary>
///     Integration tests for the Plinko mini-game endpoints: <c>/master/casino/</c>, <c>/master/casino/drop/</c>, and <c>/master/casino/viewchest/</c>.
/// </summary>
public sealed class MiniGameTests
{
    private const string PlinkoIndexRoute     = "/master/casino/";
    private const string PlinkoDropRoute      = "/master/casino/drop/";
    private const string PlinkoViewChestRoute = "/master/casino/viewchest/";

    [Test]
    public async Task Index_WithInvalidCookie_ReturnsStatusZero()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        HttpClient client = factory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"] = "not-a-real-cookie"
        });

        HttpResponseMessage response = await client.PostAsync(PlinkoIndexRoute, form);

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(0);
    }

    [Test]
    public async Task Index_WithValidCookie_ReturnsEveryDeclaredResultParam()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, _) = await SeedAuthenticatedSession(factory, "plinko.index@kongor.com", "PlinkoIndex", goldCoins: 500, plinkoTickets: 100);

        HttpResponseMessage response = await PostForm(factory, PlinkoIndexRoute, new Dictionary<string, string> { ["cookie"] = cookie });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

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
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await SeedAuthenticatedSession(factory, "plinko.poor.gold@kongor.com", "PoorGold", goldCoins: 10, plinkoTickets: 0);

        HttpResponseMessage response = await PostForm(factory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "gold"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(10);
            await Assert.That(user.PlinkoTickets).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Drop_WithInsufficientTickets_ReturnsFailureWithoutMutation()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await SeedAuthenticatedSession(factory, "plinko.poor.ticket@kongor.com", "PoorTicket", goldCoins: 0, plinkoTickets: 5);

        HttpResponseMessage response = await PostForm(factory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "tickets"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await LoadUser(factory, userID);

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
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await SeedAuthenticatedSession(factory, $"plinko.bad.{Guid.NewGuid():N}@kongor.com", $"Bad{Guid.NewGuid().ToString("N")[..8]}", goldCoins: 1000, plinkoTickets: 1000);

        HttpResponseMessage response = await PostForm(factory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = currency
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.GoldCoins).IsEqualTo(1000);
            await Assert.That(user.PlinkoTickets).IsEqualTo(1000);
        }
    }

    [Test]
    public async Task Drop_WithSufficientGold_DeductsCostAndReturnsEveryDeclaredResultParam()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await SeedAuthenticatedSession(factory, "plinko.drop.gold@kongor.com", "DropGold", goldCoins: 1000, plinkoTickets: 0);

        HttpResponseMessage response = await PostForm(factory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "gold"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

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

        User user = await LoadUser(factory, userID);

        await Assert.That(user.GoldCoins).IsEqualTo(1000 - JSONConfiguration.PlinkoConfiguration.GoldCost);
    }

    [Test]
    public async Task Drop_WithTickets_DeductsTicketCost()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, int userID) = await SeedAuthenticatedSession(factory, "plinko.drop.ticket@kongor.com", "DropTicket", goldCoins: 0, plinkoTickets: 500);

        HttpResponseMessage response = await PostForm(factory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "tickets"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(1);

        User user = await LoadUser(factory, userID);

        // The Player Paid TicketCost Up-Front, But May Have Won Additional Tickets Back.
        // A Chest Tier Win That Grants A Product Means A Net Delta Of -TicketCost.
        // A Ticket Tier Or Exhausted Chest Tier Means A Non-Negative Net Delta, So The Balance Cannot Fall Below The Starting Balance Less The Cost.
        await Assert.That(user.PlinkoTickets).IsGreaterThanOrEqualTo(500 - JSONConfiguration.PlinkoConfiguration.TicketCost);
    }

    [Test]
    public async Task Drop_ExhaustedChestTier_PaysExhaustionReward()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        // Seed The User As Already Owning Every Product In Every Chest Tier So That Any Rolled Chest Is Guaranteed To Be Exhausted.
        // Ticket Tiers Are Unaffected By This Setup But Still Satisfy The "No Product Granted" Invariant.
        (string cookie, int userID) = await SeedAuthenticatedSession(factory, "plinko.exhaust@kongor.com", "Exhaust", goldCoins: 1000, plinkoTickets: 100);

        // Preload Every Chest Tier Product Into The User's Owned Items
        HashSet<string> seeded = CollectEveryChestTierPrefixedCode();

        await MutateUser(factory, userID, user =>
        {
            foreach (string code in seeded)
            {
                if (user.OwnedStoreItems.Contains(code).Equals(false))
                    user.OwnedStoreItems.Add(code);
            }
        });

        int ownedCountBeforeDrop = (await LoadUser(factory, userID)).OwnedStoreItems.Count;

        HttpResponseMessage response = await PostForm(factory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "gold"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        // Every Possible Outcome Now Grants Tickets (Either Chest Exhaustion Or A Ticket Tier Roll).
        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["product_id"])).IsEqualTo(-1);
            await Assert.That(Convert.ToInt32(body["products_exhausted"])).IsEqualTo(1);
            await Assert.That(Convert.ToInt32(body["ticket_amount"])).IsGreaterThan(0);
        }

        User user = await LoadUser(factory, userID);

        // No New Product Should Have Been Granted When Every Chest Tier Is Exhausted And Ticket Tiers Never Grant Products.
        await Assert.That(user.OwnedStoreItems.Count).IsEqualTo(ownedCountBeforeDrop);
    }

    [Test]
    public async Task Drop_SuccessfulChestDrop_AppendsWinningProductToOwnedItems()
    {
        // The Stubbed Random Always Rolls Tier 1 (NextDouble = 0.0 Satisfies "roll < Tier1Probability") And Always Picks The First Eligible Item (Next(N) = 0), Making The Drop Fully Deterministic Without Relying On Retry Attempts
        FixedRandom random = new (nextDouble: 0.0, nextInteger: 0);

        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance(random: random);

        // The Deterministic Winner Is The First Enabled And Purchasable Product In Tier 1, Since The Controller Iterates The Tier's Product List In Declared Order And Our Stubbed Next(N) Returns Index 0
        StoreItem target = FirstEligibleProductInTier(tierID: 1)
            ?? throw new InvalidOperationException(@"Plinko Tier 1 Has No Enabled And Purchasable Products; The Test Setup Is Incompatible With The Current Configuration");

        (string cookie, int userID) = await SeedAuthenticatedSession(factory, "plinko.drop.owned@kongor.com", "DropOwned", goldCoins: JSONConfiguration.PlinkoConfiguration.GoldCost, plinkoTickets: 0);

        // Pre-Own Every Enabled And Purchasable Chest-Tier Product Except The Deterministic Target, So That Exactly One Eligible Product Remains When Tier 1 Is Rolled, Guaranteeing That Next(1) Returns 0 (Target Wins)
        HashSet<string> everyChestTierCode = CollectEveryChestTierPrefixedCode();

        await MutateUser(factory, userID, user =>
        {
            foreach (string code in everyChestTierCode)
            {
                if (code.Equals(target.PrefixedCode))
                    continue;

                if (user.OwnedStoreItems.Contains(code).Equals(false))
                    user.OwnedStoreItems.Add(code);
            }
        });

        int ownedCountBeforeDrop = (await LoadUser(factory, userID)).OwnedStoreItems.Count;

        HttpResponseMessage response = await PostForm(factory, PlinkoDropRoute, new Dictionary<string, string>
        {
            ["cookie"]      = cookie,
            ["currency"]    = "gold"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        User user = await LoadUser(factory, userID);

        using (Assert.Multiple())
        {
            // The Stubbed Random Must Produce A Tier 1 Chest Roll Granting The Deterministic Target
            await Assert.That(Convert.ToInt32(body["random_tier"])).IsEqualTo(1);
            await Assert.That(Convert.ToInt32(body["products_exhausted"])).IsEqualTo(0);
            await Assert.That(Convert.ToInt32(body["product_id"])).IsEqualTo(target.ID);

            // The Won Item Must Be Appended To OwnedStoreItems Under The Exact PrefixedCode The Store Controller Uses For Its Ownership Check
            await Assert.That(user.OwnedStoreItems).Contains(target.PrefixedCode);
            await Assert.That(user.OwnedStoreItems.Count).IsEqualTo(ownedCountBeforeDrop + 1);

            // The Response Must Contain The Dropped Product's Details Under The Declared Keys
            await Assert.That(body["product_name"].ToString()).IsEqualTo(target.Code);
            await Assert.That(body["product_type"].ToString()).IsEqualTo(StoreItem.GetClientCategoryName(target.StoreItemType));
        }
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    public async Task ViewChest_WithValidTier_ReturnsEveryDeclaredResultParam(int tierID)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, _) = await SeedAuthenticatedSession(factory, $"viewchest{tierID}@kongor.com", $"ViewChest{tierID}", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PostForm(factory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = tierID.ToString(),
            ["target_index"]    = "1"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

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
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, _) = await SeedAuthenticatedSession(factory, $"badtier{tierID}@kongor.com", $"BadTier{tierID}", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PostForm(factory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = tierID.ToString(),
            ["target_index"]    = "1"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["status_code"])).IsEqualTo(0);
    }

    [Test]
    public async Task ViewChest_WithTargetIndexZero_ClampsToOneWithoutThrowing()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, _) = await SeedAuthenticatedSession(factory, "clamp@kongor.com", "Clamp", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PostForm(factory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = "1",
            ["target_index"]    = "0"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["first_item_index"])).IsEqualTo(1);
    }

    [Test]
    public async Task ViewChest_WithTargetIndexBeyondTierSize_ReturnsEmptyPage()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();

        (string cookie, _) = await SeedAuthenticatedSession(factory, "beyond@kongor.com", "Beyond", goldCoins: 0, plinkoTickets: 0);

        HttpResponseMessage response = await PostForm(factory, PlinkoViewChestRoute, new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["tier_id"]         = "1",
            ["target_index"]    = "100000"
        });

        IDictionary<object, object> body = await DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["items_amount"])).IsEqualTo(0);
    }

    private static async Task<(string Cookie, int UserID)> SeedAuthenticatedSession(WebApplicationFactory<KONGORAssemblyMarker> factory, string emailAddress, string accountName, int goldCoins, int plinkoTickets)
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

    private static async Task<User> LoadUser(WebApplicationFactory<KONGORAssemblyMarker> factory, int userID)
    {
        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        return await databaseContext.Users.SingleAsync(user => user.ID == userID);
    }

    private static async Task MutateUser(WebApplicationFactory<KONGORAssemblyMarker> factory, int userID, Action<User> mutation)
    {
        MerrickContext databaseContext = factory.Services.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(queried => queried.ID == userID);

        mutation(user);

        await databaseContext.SaveChangesAsync();
    }

    private static StoreItem? FirstEligibleProductInTier(int tierID)
    {
        PlinkoTierProductsConfiguration tierProducts = JSONConfiguration.PlinkoTierProductsConfiguration;
        StoreItemsConfiguration storeItems = JSONConfiguration.StoreItemsConfiguration;

        foreach (int productID in tierProducts.GetProductIDs(tierID))
        {
            StoreItem? storeItem = storeItems.GetByID(productID);

            if (storeItem is not null && storeItem.IsEnabled && storeItem.Purchasable)
                return storeItem;
        }

        return null;
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

    private static async Task<HttpResponseMessage> PostForm(WebApplicationFactory<KONGORAssemblyMarker> factory, string route, Dictionary<string, string> fields)
    {
        HttpClient client = factory.CreateClient();

        FormUrlEncodedContent form = new (fields);

        return await client.PostAsync(route, form);
    }

    private static async Task<IDictionary<object, object>> DeserialisePhpResponse(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return PhpSerialization.Deserialize(body) as IDictionary<object, object>
            ?? throw new InvalidOperationException($@"Response Body Was Not A PHP Array: ""{body}""");
    }
}

/// <summary>
///     A <see cref="Random"/> subclass whose <see cref="NextDouble"/> and <see cref="Next(int)"/> return fixed, caller-supplied values.
///     Used by tests that need to drive randomness-dependent code paths deterministically via dependency injection.
/// </summary>
file sealed class FixedRandom(double nextDouble, int nextInteger) : Random
{
    public override double NextDouble() => nextDouble;

    public override int Next(int maxValue) => nextInteger;
}
