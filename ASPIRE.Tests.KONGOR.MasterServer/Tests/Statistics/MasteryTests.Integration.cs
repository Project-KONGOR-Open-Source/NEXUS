namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Integration tests for the mastery client-requester endpoints, covering the post-match boost flow and the rule that a boost may only be applied to the account's most recent match.
/// </summary>
public sealed class MasteryTests_Integration(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string ClientRequesterRoute = "client_requester.php";

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

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
            // The Client Receives Error Code 5 ("Match Is Too Old") To Show An Error Modal, And The Boost Is Not Consumed
            await Assert.That(Convert.ToInt32(body["error_code"])).IsEqualTo(5);
            await Assert.That(MasteryConsumables.MasteryBoostsOwned(user)).IsEqualTo(1);
        }
    }

    private async Task<(Account Account, string Cookie)> SeedAuthenticatedAccount(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (account, cookie);
    }

    private async Task SeedRankedMatch(Account account, int matchID, string heroIdentifier, int heroLevel)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_MATCHMAKING);

        MatchStatistics matchStatistics = MatchDataHelper.BuildMatchStatistics(matchID);

        matchStatistics.MatchInformationSnapshot = System.Text.Json.JsonSerializer.Serialize(matchInformation);

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
