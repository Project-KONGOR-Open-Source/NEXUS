namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Integration tests for the seasonal rank reported by the statistics of the client-requester endpoint, which the client turns into the rank name and the rank icon of the player profile.
/// </summary>
public sealed class SeasonStatisticsTests_Integration(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string ClientRequesterRoute = "client_requester.php";

    private const double RankedSkillRating = 1800;

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    [Arguments("campaign", true)]
    [Arguments("campaign_casual", false)]
    public async Task Show_Stats_Reports_The_Season_Medal_As_The_Current_And_Highest_Level(string table, bool placementsCompleted)
    {
        (Account account, string cookie) = await SeedRankedAccount("season.stats@kongor.com", "SeasonStats");

        HttpResponseMessage response = await webApplicationFactory.CreateClient().PostAsync(ClientRequesterRoute, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["f"]        = "show_stats",
            ["nickname"] = account.Name,
            ["cookie"]   = cookie,
            ["table"]    = table
        }));

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        // The Client Translates The Level Into A Rank Name ("player_compaign_level_name_S7_{Level}"), So It Must Be The Season Medal Rather Than The Account Level, And No Medal Is Awarded During Placements
        string expectedLevel = placementsCompleted ? ((int) RankExtensions.GetRank(RankedSkillRating)).ToString() : ((int) Rank.NO_MEDAL).ToString();

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToString(body["current_level"])).IsEqualTo(expectedLevel);
            await Assert.That(Convert.ToString(body["highest_level_current"])).IsEqualTo(expectedLevel);
        }
    }

    [Test]
    public async Task Show_Stats_Reports_A_Highest_Medal_Above_The_Current_Medal_After_Rating_Losses()
    {
        (Account account, string cookie) = await SeedRankedAccount("season.highest@kongor.com", "SeasonHighest");

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            AccountStatistics rankedStatistics = await databaseContext.AccountStatistics.SingleAsync(statistics => statistics.AccountID == account.ID && statistics.Type == AccountStatisticsType.Matchmaking);

            rankedStatistics.HighestMedal = Rank.IMMORTAL;

            await databaseContext.SaveChangesAsync();
        }

        HttpResponseMessage response = await webApplicationFactory.CreateClient().PostAsync(ClientRequesterRoute, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["f"]        = "show_stats",
            ["nickname"] = account.Name,
            ["cookie"]   = cookie,
            ["table"]    = "campaign"
        }));

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToString(body["current_level"])).IsEqualTo(((int) RankExtensions.GetRank(RankedSkillRating)).ToString());
            await Assert.That(Convert.ToString(body["highest_level_current"])).IsEqualTo(((int) Rank.IMMORTAL).ToString());
        }
    }

    [Test]
    public async Task Show_Simple_Stats_Reports_The_Season_Medal_As_The_Current_Level()
    {
        (Account account, string cookie) = await SeedRankedAccount("season.simple@kongor.com", "SeasonSimple");

        HttpResponseMessage response = await webApplicationFactory.CreateClient().PostAsync($"{ClientRequesterRoute}?f=show_simple_stats", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["nickname"] = account.Name
        }));

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        IDictionary<object, object> normalSeason = (IDictionary<object, object>) body["season_normal"];
        IDictionary<object, object> casualSeason = (IDictionary<object, object>) body["season_casual"];

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(normalSeason["current_level"])).IsEqualTo((int) RankExtensions.GetRank(RankedSkillRating));
            await Assert.That(Convert.ToInt32(casualSeason["current_level"])).IsEqualTo((int) Rank.NO_MEDAL);
        }
    }

    /// <summary>
    ///     Seeds an account that has completed its ranked placement matches but is still in its casual placement matches, with an authenticated session.
    /// </summary>
    private async Task<(Account Account, string Cookie)> SeedRankedAccount(string emailAddress, string accountName)
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        string cookie = Guid.NewGuid().ToString("N");

        await webApplicationFactory.Services.GetRequiredService<IDatabase>().SetAccountNameForSessionCookie(cookie, account.Name);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        AccountStatistics rankedStatistics = await databaseContext.AccountStatistics.SingleAsync(statistics => statistics.AccountID == account.ID && statistics.Type == AccountStatisticsType.Matchmaking);
        AccountStatistics casualStatistics = await databaseContext.AccountStatistics.SingleAsync(statistics => statistics.AccountID == account.ID && statistics.Type == AccountStatisticsType.MatchmakingCasual);

        rankedStatistics.SkillRating = RankedSkillRating;
        rankedStatistics.PlacementMatchesData = "110101";

        casualStatistics.SkillRating = RankedSkillRating;
        casualStatistics.PlacementMatchesData = "11";

        // Statistics Submission Never Leaves The Highest Medal Below The Current Medal
        rankedStatistics.HighestMedal = rankedStatistics.CurrentMedal();
        casualStatistics.HighestMedal = casualStatistics.CurrentMedal();

        // An Account Level That Is Not A Valid Medal Distinguishes The Season Medal From The Account Level
        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

        user.TotalLevel = 666;

        await databaseContext.SaveChangesAsync();

        return (account, cookie);
    }
}
