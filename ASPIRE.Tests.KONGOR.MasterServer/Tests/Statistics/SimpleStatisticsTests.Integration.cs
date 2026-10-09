namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Integration tests for the simple statistics of the client-requester endpoint, which back the player card of the home page.
/// </summary>
public sealed class SimpleStatisticsTests_Integration(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string ClientRequesterRoute = "client_requester.php";

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Show_Simple_Stats_Reports_The_Top_Awards_With_The_Award_Codes_Of_The_Client()
    {
        SRPAuthenticationService service = new (webApplicationFactory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials("simple.stats@kongor.com", "SimpleStats", "DoesNotMatter123!");

        string cookie = Guid.NewGuid().ToString("N");

        await webApplicationFactory.Services.GetRequiredService<IDatabase>().SetAccountNameForSessionCookie(cookie, account.Name);

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            AccountStatistics statistics = await databaseContext.AccountStatistics.SingleAsync(candidate => candidate.AccountID == account.ID && candidate.Type == AccountStatisticsType.Matchmaking);

            statistics.AwardStatistics.MostKillsAwards = 4;
            statistics.AwardStatistics.LeastDeathsAwards = 3;
            statistics.AwardStatistics.SmackdownAwards = 2;
            statistics.AwardStatistics.AnnihilationAwards = 2;

            await databaseContext.SaveChangesAsync();
        }

        HttpResponseMessage response = await webApplicationFactory.CreateClient().PostAsync($"{ClientRequesterRoute}?f=show_simple_stats", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["cookie"]   = cookie,
            ["nickname"] = account.Name
        }));

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object> body = await PlinkoTestsHelper.DeserialisePhpResponse(response);

        List<string> awardNames = EnumeratePHPArrayValues(body["award_top4_name"]).Select(Convert.ToString).OfType<string>().ToList();

        // The Client Derives The Icon And The Tooltip Of Each Award From Its Code, And Equal Counts Are Ordered By The Award Priority Of The Original API (Annihilation Before Smackdown)
        await Assert.That(awardNames).IsEquivalentTo(["awd_mkill", "awd_ledth", "awd_mann", "awd_msd"], TUnit.Assertions.Enums.CollectionOrdering.Matching);
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
}
