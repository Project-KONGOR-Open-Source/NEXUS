namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Integration tests for the mastery client-requester endpoints, covering the mastery block of the match stats response, the post-match boost flow, the boost eligibility rules, and reward tier claims.
/// </summary>
public sealed class MasteryTests_Integration(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string ClientRequesterRoute = "client_requester.php";

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Boost_Match_Mastery_Applies_Experience_Records_It_On_The_Match_And_Consumes_A_Boost()
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

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == account.ID);

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        MatchParticipantStatistics participant = await LoadParticipant(account, matchID: 1);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(0);

            // A Ranked Match At Hero Level 10 Earns 200; With No Maximum-Level Heroes The Bonus Is Zero, So A Regular Boost Adds (200 + 0) * 2 = 400 On Top Of The 200
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(600);
            await Assert.That(participant.MasteryProgression?.BoostExperience).IsEqualTo(400);
            await Assert.That(participant.MasteryProgression?.SuperBoostExperience).IsEqualTo(0);
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Super_Boost_Advances_The_Hero_To_The_Next_Mastery_Level_Boundary()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.super@kongor.com", "BoostSuper");

        // 1800 Before The Match Plus 200 From The Match Leaves The Hero At 2000, Which Is Level 1 (Boundary 1400 To 3000)
        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, masteryExperienceBeforeMatch: 1800);
        await GrantMasteryBoosts(account, regularBoosts: 0, superBoosts: 1);

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

        MatchParticipantStatistics participant = await LoadParticipant(account, matchID: 1);

        using (Assert.Multiple())
        {
            // A Super Boost Advances The Hero To The Start Of The Next Mastery Level (The Upper Boundary Of The Current Level)
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(3000);
            await Assert.That(participant.MasteryProgression?.SuperBoostExperience).IsEqualTo(1000);
            await Assert.That(MasteryConsumables.SuperMasteryBoostsOwned(user)).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_A_Match_That_Is_Not_The_Most_Recent()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.old@kongor.com", "BoostOld");

        // Both Matches Share A Timestamp, So The Most Recent Match Is Resolved By The Later Participant Record
        DateTimeOffset timestampRecorded = DateTimeOffset.UtcNow;

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, timestampRecorded: timestampRecorded);
        await SeedRankedMatch(account, matchID: 2, heroIdentifier: "Hero_Armadon",  heroLevel: 10, timestampRecorded: timestampRecorded);
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
    public async Task Boost_Match_Mastery_Allows_The_Most_Recent_Mastery_Match_After_A_Later_Match_Without_Mastery()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.public@kongor.com", "BoostPublic");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, timestampRecorded: DateTimeOffset.UtcNow - TimeSpan.FromHours(1));

        // A Later Public Match Awards No Mastery Experience, So It Does Not Replace The Ranked Match As The Most Recent Mastery Match
        await SeedRankedMatch(account, matchID: 2, heroIdentifier: "Hero_Armadon", heroLevel: 10, matchType: MatchType.AM_PUBLIC, timestampRecorded: DateTimeOffset.UtcNow);
        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 0);

        HttpResponseMessage response = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        MatchParticipantStatistics participant = await LoadParticipant(account, matchID: 1);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(0);
            await Assert.That(participant.MasteryProgression?.BoostExperience).IsEqualTo(400);
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_A_Second_Application_To_The_Same_Match()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.twice@kongor.com", "BoostTwice");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);
        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 1);

        HttpResponseMessage firstResponse = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "0"
        });

        await Assert.That(firstResponse.IsSuccessStatusCode).IsTrue();

        // A Super Boost After A Regular Boost Is Also Rejected, Because Only One Boost May Be Applied To A Match
        HttpResponseMessage secondResponse = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(secondResponse);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == account.ID);

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        using (Assert.Multiple())
        {
            // The Client Receives The Original API's "Match Already Boosted" Error Code, The Second Boost Is Not Consumed, And The Experience From The First Boost Is Unchanged
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(4);
            await Assert.That(MasteryConsumables.SuperMasteryBoostsOwned(user)).IsEqualTo(1);
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(600);
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_A_Hero_At_The_Maximum_Mastery_Level()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.maximum@kongor.com", "BoostMaximum");

        // 36000 Before The Match Plus 200 From The Match Is Capped At The Level 15 Threshold
        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, masteryExperienceBeforeMatch: 36000);
        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 1);

        HttpResponseMessage boostResponse = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "1"
        });

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
            // The Original API Returned Its Generic Error Code For A Hero At The Maximum Mastery Level, And The Boost Is Not Consumed
            await Assert.That(Convert.ToInt32(boostBody["error_code"])).IsEqualTo(1);
            await Assert.That(MasteryConsumables.SuperMasteryBoostsOwned(user)).IsEqualTo(1);

            // The Match Statistics Report The Uncapped Experience After The Match, From Which The Client Derives The Exact Experience Before It, And Disable The Boost Controls
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(36000 + 200);
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
            await Assert.That(Convert.ToBoolean(mastery["mastery_super_canboost"])).IsFalse();
        }
    }

    [Test]
    public async Task Get_Match_Stats_For_A_Hero_Already_At_The_Maximum_Mastery_Level_Derives_A_Starting_Experience_At_The_Maximum_Level()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.maximum@kongor.com", "MaxLevelExp");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, masteryExperienceBeforeMatch: Mastery.MaximumMasteryExperience);

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        int derivedExperienceBeforeMatch = Convert.ToInt32(mastery["mastery_exp_original"]) - Convert.ToInt32(mastery["mastery_exp_match"]) - Convert.ToInt32(mastery["mastery_exp_heroes_addon"])
            - Convert.ToInt32(mastery["mastery_exp_boost"]) - Convert.ToInt32(mastery["mastery_exp_super_boost"]);

        // The Client Derives The Starting Value By Subtracting The Awarded Experience And Shows A Level-Up Popup When The Starting Value Is One Level Below The End, So A Hero Already At The Maximum Level Must Derive A Starting Value At The Maximum Level
        await Assert.That(derivedExperienceBeforeMatch).IsEqualTo(Mastery.MaximumMasteryExperience);
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_A_Match_Without_Mastery_Experience()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.nomastery@kongor.com", "BoostNoMastery");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, matchType: MatchType.AM_PUBLIC);
        await GrantMasteryBoosts(account, regularBoosts: 0, superBoosts: 1);

        HttpResponseMessage response = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == account.ID);

        using (Assert.Multiple())
        {
            // The Original API Had No Mastery Data For Matches That Award No Mastery Experience, So It Returned "Match Mastery Data Does Not Exist"
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(6);
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Boost_Match_Mastery_Rejects_An_Invalid_Super_Boost_Flag()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.flag@kongor.com", "BoostFlag");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);
        await GrantMasteryBoosts(account, regularBoosts: 1, superBoosts: 0);

        HttpResponseMessage response = await PostClientRequest("boost_match_mastery", new Dictionary<string, string>
        {
            ["cookie"]         = cookie,
            ["match_id"]       = "1",
            ["is_super_boost"] = "true"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(2);
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

        Mastery mastery = await databaseContext.Masteries.SingleAsync(record => record.AccountID == account.ID);

        MatchParticipantStatistics participant = await LoadParticipant(account, matchID: 1);

        using (Assert.Multiple())
        {
            // The Client Parses The Response Body As PHP-Serialised Data And Treats A Missing "error_code" Key As A Success, So The Error Must Be A PHP-Serialised Payload With The Original API's "Not Enough Mastery Boosts" Error Code
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(3);
            await Assert.That(mastery.GetHeroExperienceByHeroIdentifier("Hero_Accursed")).IsEqualTo(200);
            await Assert.That(participant.MasteryProgression?.BoostExperience).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Get_Match_Stats_Reports_The_Mastery_Experience_After_The_Match()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.original@kongor.com", "PostMatchExp");

        // 800 Before The Match Plus The 200 Awarded For A Ranked Match At Hero Level 10
        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, masteryExperienceBeforeMatch: 800);

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
            // The Client Animates The Progress Bar Up To "mastery_exp_original", Deriving The Starting Value By Subtracting The Match, Bonus, And Boost Experience
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(1000);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(200);
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsTrue();
        }
    }

    [Test]
    public async Task Get_Match_Stats_Reports_The_Maximum_Level_Hero_Count_And_Bonus_Recorded_For_The_Match()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.bonus@kongor.com", "RecordedBonus");

        // The Match Was Recorded With 12 Maximum-Level Heroes, While The Account Currently Has None, So Only The Recorded Values Can Produce The Expected Response
        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, heroesAtMaximumMasteryCount: 12);

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        using (Assert.Multiple())
        {
            // A Ranked Match At Hero Level 10 Earns 200, And 12 Maximum-Level Heroes Add 12 Points Of Bonus Experience
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_heroes_count"])).IsEqualTo(12);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_heroes_addon"])).IsEqualTo(12);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(200 + 12);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_to_boost"])).IsEqualTo((200 + 12) * 2);
        }
    }

    [Test]
    public async Task Get_Match_Stats_For_An_Older_Match_Reports_The_Progression_Of_That_Match()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.older@kongor.com", "OlderMatchExp");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, timestampRecorded: DateTimeOffset.UtcNow - TimeSpan.FromHours(1));
        await SeedRankedMatch(account, matchID: 2, heroIdentifier: "Hero_Accursed", heroLevel: 10, timestampRecorded: DateTimeOffset.UtcNow, masteryExperienceBeforeMatch: 200);

        HttpResponseMessage response = await PostClientRequest("get_match_stats", new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["match_id"] = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> mastery = (IDictionary<object, object>) body["match_mastery"];

        using (Assert.Multiple())
        {
            // The Hero Has 400 Experience Now, But The Older Match Reports The 200 It Had Right After That Match
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(200);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(200);
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
        }
    }

    [Test]
    public async Task Get_Match_Stats_For_A_Match_Without_Mastery_Experience_Reports_No_Mastery_Progression()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.nomastery@kongor.com", "NoMasteryExp");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, matchType: MatchType.AM_PUBLIC);

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
            // A Zero Match Experience Makes The Client Hide The Mastery Panel, Which Also Covers Matches Recorded Before The Mastery Progression Was Recorded
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(0);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(0);
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
        }
    }

    [Test]
    public async Task Get_Match_Stats_For_A_MidWars_Match_Reports_Mastery_But_No_Campaign_Progress()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("stats.midwars@kongor.com", "MidWarsStats");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, matchType: MatchType.AM_MATCHMAKING_MIDWARS, map: "midwars");

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
            // A MidWars Match At Hero Level 10 Awards 100, So The Client Receives A Non-Zero Match Value To Show The Mastery Panel
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_match"])).IsEqualTo(100);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(100);

            // MidWars Matches Do Not Participate In The Seasonal Campaign, So The Client Must Receive No Medal Or Placement Progress
            await Assert.That(Convert.ToInt32(campaignInformation["placement_matches"])).IsEqualTo(0);
            await Assert.That(Convert.ToString(campaignInformation["medal_after"])).IsEqualTo("0");
        }
    }

    [Test]
    public async Task Get_Match_Stats_Reports_An_Applied_Boost_And_Disables_Further_Boosting()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("boost.report@kongor.com", "BoostReport");

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10);
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
            // The Experience After The Match Includes The Boost: 0 Before, 200 From The Match, And 400 From The Boost
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(600);
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

        await SeedRankedMatch(account, matchID: 1, heroIdentifier: "Hero_Accursed", heroLevel: 10, masteryExperienceBeforeMatch: 1800);
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
            // A Super Boost Advances The Hero From 2000 To The 3000 Level Boundary, And The 1000 Difference Is Reported In The Super Boost Field
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_original"])).IsEqualTo(3000);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_boost"])).IsEqualTo(0);
            await Assert.That(Convert.ToInt32(mastery["mastery_exp_super_boost"])).IsEqualTo(1000);

            // A Non-Zero Applied Boost Makes The Client Display The Match As Already Boosted And Hide The Boost Purchase Controls
            await Assert.That(Convert.ToBoolean(mastery["mastery_canboost"])).IsFalse();
            await Assert.That(Convert.ToBoolean(mastery["mastery_super_canboost"])).IsFalse();
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

    [Test]
    public async Task Take_Mastery_Reward_Grants_A_Tier_The_Account_Has_Reached()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("reward.reached@kongor.com", "RewardReached");

        // A Single Hero At 1400 Experience Is Level 1, So The Account's Total Mastery Level Is Exactly 1
        await SetHeroExperience(account, "Hero_Accursed", 1400);

        HttpResponseMessage response = await PostClientRequest("take_mastery_reward", new Dictionary<string, string>
        {
            ["cookie"] = cookie,
            ["level"]  = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        MasteryRewards rewards = await databaseContext.MasteryRewards.SingleAsync(record => record.AccountID == account.ID);

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        int expectedBoosts = JSONConfiguration.MasteryRewardsConfiguration.MasteryRewards.Single(reward => reward.RequiredLevel == 1).ProductQuantity;

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(0);
            await Assert.That(rewards.HasObtained(1)).IsTrue();
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(expectedBoosts);
        }
    }

    [Test]
    public async Task Take_Mastery_Reward_Rejects_A_Tier_Above_The_Accounts_Total_Mastery_Level()
    {
        (Account account, string cookie) = await SeedAuthenticatedAccount("reward.locked@kongor.com", "RewardLocked");

        HttpResponseMessage response = await PostClientRequest("take_mastery_reward", new Dictionary<string, string>
        {
            ["cookie"] = cookie,
            ["level"]  = "1"
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        MasteryRewards? rewards = await databaseContext.MasteryRewards.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        using (Assert.Multiple())
        {
            // Error Code 4 Is The Original API's "Reward Does Not Exist" Code, Which It Returned For Tiers The Account Had Not Unlocked
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(4);
            await Assert.That(rewards?.HasObtained(1) ?? false).IsFalse();
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Take_Mastery_Reward_Rejects_A_Tier_That_Is_Not_Configured()
    {
        (Account _, string cookie) = await SeedAuthenticatedAccount("reward.missing@kongor.com", "RewardMissing");

        int unconfiguredLevel = Enumerable.Range(1, 10000).First(level => JSONConfiguration.MasteryRewardsConfiguration.MasteryRewards.All(reward => reward.RequiredLevel != level));

        HttpResponseMessage response = await PostClientRequest("take_mastery_reward", new Dictionary<string, string>
        {
            ["cookie"] = cookie,
            ["level"]  = unconfiguredLevel.ToString()
        });

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        // Error Code 5 Is The Original API's "Mastery Reward Invalid" Code
        await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(5);
    }

    [Test]
    public async Task Purchasing_A_Heros_Last_Avatar_Exchanges_That_Heros_Mastery_Coupon_For_The_All_Avatar_Coupon()
    {
        List<string> qiAvatars = MasteryCouponHelper.ApplicableAvatars("Hero_Chi");

        StoreItem lastAvatar = JSONConfiguration.StoreItemsConfiguration.GetEnabledItemsByType(StoreItemType.AlternativeAvatar)
            .Single(item => item.PrefixedCode == qiAvatars[^1]);

        (string cookie, int accountID, int userID) = await RedeemCodeTestsHelper.SeedAuthenticatedSession(webApplicationFactory, "coupon.exchange@kongor.com", "CouponExchange", goldCoins: lastAvatar.GoldCost, silverCoins: 0, plinkoTickets: 0);

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            User seededUser = await databaseContext.Users.SingleAsync(candidate => candidate.ID == userID);

            seededUser.OwnedStoreItems.AddRange(qiAvatars.SkipLast(1));
            seededUser.OwnedStoreItems.Add("cp.Qi Mastery Coupon * 1");

            await databaseContext.SaveChangesAsync();
        }

        HttpClient client = webApplicationFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/store_requester.php", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["cookie"]       = cookie,
            ["account_id"]   = accountID.ToString(),
            ["request_code"] = "4",
            ["product_id"]   = lastAvatar.ID.ToString(),
            ["currency"]     = "0",
            ["category_id"]  = "2", // The "Hero Avatars" Store Category (A File-Scoped Enumeration In The Store Controller)
            ["page"]         = "1",
            ["hostTime"]     = "0",
            ["discount"]     = "0"
        }));

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        User user = await RedeemCodeTestsHelper.LoadUser(webApplicationFactory, userID);

        using (Assert.Multiple())
        {
            await Assert.That(user.OwnedStoreItems).Contains(lastAvatar.PrefixedCode);
            await Assert.That(user.OwnedStoreItems.Any(item => item.StartsWith("cp.Qi Mastery Coupon * ", StringComparison.Ordinal))).IsFalse();
            await Assert.That(user.OwnedStoreItems).Contains("cp.All Avatar Mastery Coupon * 1");
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

    private async Task SeedRankedMatch(Account account, int matchID, string heroIdentifier, int heroLevel, MatchType matchType = MatchType.AM_MATCHMAKING, string map = "caldavar", DateTimeOffset? timestampRecorded = null, int masteryExperienceBeforeMatch = 0, int heroesAtMaximumMasteryCount = 0)
    {
        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(matchType, map: map);

        AccountStatisticsType statisticsType = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation);

        int masteryMatchExperience = Mastery.CalculateMatchExperience(statisticsType, heroLevel);
        int masteryBonusExperience = Mastery.CalculateBonusExperience(statisticsType, masteryMatchExperience, heroesAtMaximumMasteryCount);

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            MatchStatistics matchStatistics = MatchDataHelper.BuildMatchStatistics(matchID, map);

            matchStatistics.MatchInformationSnapshot = System.Text.Json.JsonSerializer.Serialize(matchInformation);

            if (timestampRecorded is not null)
                matchStatistics.TimestampRecorded = timestampRecorded.Value;

            MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(account.ID, account.Name, groupNumber: 1, win: 1, matchID: matchID, publicMatch: 0, rankedMatch: 1);

            participant.HeroIdentifier = heroIdentifier;
            participant.HeroLevel = heroLevel;

            // Mirror Statistics Submission, Which Records The Mastery Progression Only For Matches That Award Mastery Experience (Unless Stated Otherwise, The Account Has No Maximum-Level Heroes At The Time Of The Match, So The Bonus Is Zero)
            if (masteryMatchExperience > 0)
            {
                participant.MasteryProgression = new MasteryProgression
                {
                    ExperienceBeforeMatch = masteryExperienceBeforeMatch,
                    MatchExperience = masteryMatchExperience,
                    HeroesAtMaximumMasteryCount = heroesAtMaximumMasteryCount,
                    BonusExperience = masteryBonusExperience
                };
            }

            await databaseContext.MatchStatistics.AddAsync(matchStatistics);
            await databaseContext.MatchParticipantStatistics.AddAsync(participant);

            await databaseContext.SaveChangesAsync();
        }

        // Mirror Statistics Submission, Which Accrues The Match Experience Into The Hero's Persisted Total
        if (masteryMatchExperience > 0)
            await SetHeroExperience(account, heroIdentifier, masteryExperienceBeforeMatch + masteryMatchExperience + masteryBonusExperience);
    }

    private async Task<MatchParticipantStatistics> LoadParticipant(Account account, int matchID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.MatchParticipantStatistics.SingleAsync(statistics => statistics.AccountID == account.ID && statistics.MatchID == matchID);
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
