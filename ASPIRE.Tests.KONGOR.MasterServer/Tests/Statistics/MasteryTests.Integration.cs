namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Integration tests for the mastery client-requester endpoints, covering the mastery block of the match stats response, the post-match boost flow, and the rules that a boost may only be applied to the account's most recent match and only once per match.
/// </summary>
public sealed class MasteryTests_Integration(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string ClientRequesterRoute = "client_requester.php";

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Boost_Match_Mastery_Applies_Experience_And_Consumes_A_Boost()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.apply@kongor.com", "BoostApply");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);
        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 0);

        HttpResponseMessage response = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == account.ID);

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        using (Assert.Multiple())
        {
            // A Ranked Match At Hero Level 10 Earns 200 Base Experience; With No Maxed Heroes The Bonus Is Zero, So A Regular Boost Adds (200 + 0) * 2 = 400
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(400);
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Super_Boost_Advances_The_Hero_To_The_Next_Mastery_Level_Boundary()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.super@kongor.com", "BoostSuper");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);
        await GrantMasteryBoosts(account, regularBoosts: 0, superBoosts: 1);
        await SetHeroExperience(account, "Hero_Accursed", 2000); // Level 1 (Boundary 1400 - 3000)

        HttpResponseMessage response = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "1"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == account.ID);

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        using (Assert.Multiple())
        {
            // A Super Boost Advances The Hero To The Start Of The Next Mastery Level (The Upper Boundary Of The Current Level)
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(3000);
            await Assert.That(MasteryConsumables.SuperMasteryBoostsOwned(user)).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_A_Match_That_Is_Not_The_Most_Recent()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.old@kongor.com", "BoostOld");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);   // Older Match
        await SeedRankedMatch(account, matchID: 2, heroIdentifier: "Hero_Armadon",  heroLevel: 10);    // Most Recent Match
        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 0);

        HttpResponseMessage response = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        using (Assert.Multiple())
        {
            // The Client Receives The Original API's "Match Outdated" Error Code To Show An Error Modal, And The Boost Is Not Consumed
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(5);
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(1);
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_A_Second_Application_To_The_Same_Match()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.twice@kongor.com", "BoostTwice");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);
        await GrantMasteryBoosts(account, regularBoosts: 2, superBoosts: 0);

        Dictionary<string, string> boostRequest = new ()
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        };

        HttpResponseMessage firstResponse = await PostClientRequest("boost_match_mastery", boostRequest);

        await Assert.That(firstResponse.IsSuccessStatusCode).IsTrue();

        HttpResponseMessage secondResponse = await PostClientRequest("boost_match_mastery", boostRequest);

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(secondResponse);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == account.ID);

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        using (Assert.Multiple())
        {
            // The Client Receives The Original API's "Match Already Boosted" Error Code To Show An Error Modal, The Second Boost Is Not Consumed, And The Experience From The First Boost Is Unchanged
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(4);
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(1);
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(400);
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_A_Match_Older_Than_The_Application_Window()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.window@kongor.com", "BoostWindow");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10,
            timestampRecorded: DateTimeOffset.UtcNow - MasteryBoost.ApplicationWindow - TimeSpan.FromDays(1));

        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 0);

        HttpResponseMessage boostResponse = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        });

        await Assert.That(boostResponse.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> boostBody = await PlinkoTestsHelper.DeserialisePhpResponse(boostResponse);

        HttpResponseMessage statsResponse = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        IDictionary<object, object> statsBody = await PlinkoTestsHelper.DeserialisePhpResponse(statsResponse);

        IDictionary<object, object> mastery = (IDictionary<object, object>) statsBody["match_mastery"];

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        using (Assert.Multiple())
        {
            // The Client Receives The Original API's "Match Outdated" Error Code To Show An Error Modal, And The Boost Is Not Consumed
            await Assert.That(Convert.ToInt32(boostBody["error_code"])).IsEqualTo(5);
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(1);

            // The Boost Controls Are Also Disabled When The Match Stats Screen Is Loaded, So The Client Does Not Offer A Boost Which The Server Would Reject
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
            await Assert.That(Convert.ToBoolean(mastery["mastery_super_canboost"])).IsFalse();
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Without_An_Owned_Boost_Returns_The_Not_Enough_Boosts_Error_Code()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.none@kongor.com", "BoostNone");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);

        HttpResponseMessage response = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery? mastery = await databaseContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        using (Assert.Multiple())
        {
            // The Client Parses The Response Body As PHP-Serialised Data And Treats A Missing "error_code" Key As A Success, So The Error Must Be A PHP-Serialised Payload With The Original API's "Not Enough Mastery Boosts" Error Code
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(3);
            await Assert.That(mastery?.TotalMasteryExperience() ?? 0).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Get_Match_Stats_Reports_The_Pre_Match_Mastery_Experience()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.original@kongor.com", "PreMatchExp");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);

        // The Persisted Value Is The Post-Match Total: 800 Pre-Match Experience Plus The 200 Experience Accrued For A Ranked Match At Hero Level 10
        await SetHeroExperience(account, "Hero_Accursed", 1000);

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        using (Assert.Multiple())
        {
            // The Client Treats "mastery_exp_original" As The Pre-Match Starting Value And Adds The Match Experience On Top Of It
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(800);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(200);
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsTrue();
        }
    }

    [Test]
    public async Task Get_Match_Stats_For_A_MidWars_Match_Reports_Mastery_But_No_Campaign_Progress()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.midwars@kongor.com", "MidWarsStats");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, matchType: MatchType.AM_MATCHMAKING_MIDWARS, map: "midwars");

        // The Persisted Value Is The Post-Match Total: A MidWars Match At Hero Level 10 Accrues 100 Experience
        await SetHeroExperience(account, "Hero_Accursed", 100);

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        IDictionary<object, object> playerRow = (IDictionary<object, object>) EnumeratePHPArrayValues(body["match_player_stats"]).Select(matchPlayers => EnumeratePHPArrayValues(matchPlayers).Single()).Single();

        IDictionary<object, object> campaignInformation = (IDictionary<object, object>) playerRow["campaign_info"];

        using (Assert.Multiple())
        {
            // MidWars Matches Accrue Mastery Experience, So The Client Must Receive A Non-Zero Match Value To Show The Mastery Panel
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(100);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(0);

            // MidWars Matches Do Not Participate In The Seasonal Campaign, So The Client Must Receive No Medal Or Placement Progress
            await Assert.That(Convert.ToInt32(campaignInformation["placement_matches"])).IsEqualTo(0);
            await Assert.That(Convert.ToString(campaignInformation["medal_after"])).IsEqualTo("0");
        }
    }

    [Test]
    public async Task Get_Match_Stats_For_A_Match_The_Requester_Did_Not_Play_In_Returns_An_Empty_Mastery_Block()
    {
        (Account participant, string _) = await SeedAuthenticatedAccount("stats.player@kongor.com", "MatchPlayer");
        (Account _, string spectatorCookie) = await SeedAuthenticatedAccount("stats.viewer@kongor.com", "MatchViewer");

        await SeedRankedMatch(participant, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = spectatorCookie,
            ["match_id"] = "1"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(0);
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
        }
    }

    [Test]
    public async Task Get_Match_Stats_Reports_An_Applied_Boost_And_Disables_Further_Boosting()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.report@kongor.com", "BoostReport");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);

        // The Persisted Value Is The Post-Match Total: Zero Pre-Match Experience Plus The 200 Experience Accrued For A Ranked Match At Hero Level 10
        await SetHeroExperience(account, "Hero_Accursed", 200);
        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 0);

        HttpResponseMessage boostResponse = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        });

        await Assert.That(boostResponse.IsSuccessStatusCode).IsTrue();

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        using (Assert.Multiple())
        {
            // A Regular Boost Adds Double The Combined Match And Bonus Experience: (200 + 0) * 2 = 400
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(0);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(200);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_boost"])).IsEqualTo(400);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_super_boost"])).IsEqualTo(0);

            // A Non-Zero Applied Boost Makes The Client Display The Match As Already Boosted And Hide The Boost Purchase Controls
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
            await Assert.That(Convert.ToBoolean(mastery["mastery_super_canboost"])).IsFalse();
        }
    }

    [Test]
    public async Task Get_Match_Stats_Reports_An_Applied_Super_Boost_In_The_Super_Boost_Field()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.superreport@kongor.com", "SuperBoostRep");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);

        await SetHeroExperience(account, "Hero_Accursed", 2000); // Level 1 (Boundary 1400 - 3000)
        await GrantMasteryBoosts(account, regularBoosts: 0, superBoosts: 1);

        HttpResponseMessage boostResponse = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "1"
        });

        await Assert.That(boostResponse.IsSuccessStatusCode).IsTrue();

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        using (Assert.Multiple())
        {
            // A Super Boost Advances The Hero From 2000 Experience To The 3000 Experience Level Boundary, And The 1000 Experience Difference Is Reported In The Super Boost Field
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_boost"])).IsEqualTo(0);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_super_boost"])).IsEqualTo(1000);

            // A Non-Zero Applied Boost Makes The Client Display The Match As Already Boosted And Hide The Boost Purchase Controls
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
            await Assert.That(Convert.ToBoolean(mastery["mastery_super_canboost"])).IsFalse();
        }
    }

    [Test]
    public async Task Purchasing_A_Mastery_Boost_From_The_Match_Stats_Screen_Adds_The_Consumable_Without_Applying_Experience()
    {
        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "boost.buy@kongor.com", "BoostBuy", goldCoins: 1000, silverCoins: 0, plinkoTickets: 0);

        HttpClient client = webApplicationFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/store_requester.php", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["cookie"]       = cookie,
            ["account_id"]   = accountID.ToString(),
            ["request_code"] = "4",
            ["product_id"]   = MasteryBoost.Regular.ProductCode.ToString(),
            ["currency"]     = "0",
            ["category_id"]  = "MASTERY"
        }));

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery? mastery = await databaseContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == accountID);

        using (Assert.Multiple())
        {
            // The Purchase Adds The Consumable And Deducts The Gold; The Experience Is Only Applied By The Client's Follow-Up "boost_match_mastery" Request
            await Assert.That(Convert.ToInt32(body["product_id"])).IsEqualTo(MasteryBoost.Regular.ProductCode);
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(1);
            await Assert.That(user.GoldCoins).IsEqualTo(1000 - MasteryBoost.Regular.GoldCost);
            await Assert.That(mastery?.TotalMasteryExperience() ?? 0).IsEqualTo(0);
        }
    }

    /// <summary>
    ///     A deserialised PHP array is a list when its keys are consecutive integers and a dictionary otherwise, so both shapes are enumerated uniformly here.
    /// </summary>
    private static IReadOnlyList<object> EnumeratePHPArrayValues(object phpArray) => phpArray switch
    {
        IDictionary<object, object> dictionary => [.. dictionary.Values],
        IEnumerable<object> values              => [.. values],
        _                                       => throw new InvalidOperationException($@"Unexpected PHP Array Shape ""{phpArray.GetType()}""")
    };

    private async Task<(Account Account, string Cookie)> SeedAuthenticatedAccount(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (account, cookie);
    }

    private async Task SeedRankedMatch(Account account, int matchID, string heroIdentifier, int heroLevel, MatchType matchType = MatchType.AM_MATCHMAKING, string map = "caldavar", DateTimeOffset? timestampRecorded = null)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(matchType, map: map);

        MatchStatistics matchStatistics = MatchDataHelper.BuildMatchStatistics(matchID, map);

        matchStatistics.MatchInformationSnapshot = System.Text.Json.JsonSerializer.Serialize(matchInformation);

        if (timestampRecorded is not null)
            matchStatistics.TimestampRecorded = timestampRecorded.Value;

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, matchID: matchID, publicMatch: 0, rankedMatch: 1);

        participant.HeroIdentifier = heroIdentifier;
        participant.HeroLevel = heroLevel;

        await databaseContext.MatchStatistics.AddAsync(matchStatistics);
        await databaseContext.MatchParticipantStatistics.AddAsync(participant);

        await databaseContext.SaveChangesAsync();
    }

    private async Task GrantMasteryBoosts(Account account, int regularBoosts, int superBoosts)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        if (regularBoosts > 0)
            MasteryConsumables.AddMasteryBoost(user, regularBoosts);

        if (superBoosts > 0)
            MasteryConsumables.AddSuperMasteryBoost(user, superBoosts);

        await databaseContext.SaveChangesAsync();
    }

    private async Task SetHeroExperience(Account account, string heroIdentifier, int experience)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery? mastery = await databaseContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        if (mastery is null)
        {
            Account trackedAccount = await databaseContext.Accounts.SingleAsync(candidate => candidate.ID == account.ID);

            mastery = new Mastery { Account = trackedAccount };

            await databaseContext.Masteries.AddAsync(mastery);
        }

        mastery.SetHeroExperienceByHeroIdentifier(heroIdentifier, experience);

        await databaseContext.SaveChangesAsync();
    }

    private async Task<HttpResponseMessage> PostClientRequest(string function, Dictionary<string, string> formFields)
    {
        HttpClient client = webApplicationFactory.CreateClient();

        return await client.PostAsync($"{ClientRequesterRoute}?f={function}", new FormUrlEncodedContent(formFields));
    }
}
