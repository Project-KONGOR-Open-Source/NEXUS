namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Tests for lifting match locks when a match server submits the statistics of their match, which shows that the match is over.
/// </summary>
public sealed class StatisticsSubmissionMatchLockTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private IDatabase DistributedCache => webApplicationFactory.Services.GetRequiredService<IDatabase>();

    [Before(Test)]
    public async Task InitialiseFactory()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();
    }

    [Test]
    public async Task Submitting_Match_Statistics_Lifts_The_Locks_Of_That_Match_And_Keeps_Others()
    {
        MatchServer matchServer = new ()
        {
            HostAccountID = 1,
            HostAccountName = "LOCKED-HOST",
            ID = 1,
            Name = "LOCKED-HOST:1",
            MatchServerManagerID = null,
            Instance = 1,
            IPAddress = "127.0.0.1",
            Port = 11235,
            Location = "USE",
            Description = "Match Lock Test Server"
        };

        await DistributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);

        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: matchServer.ID));
        await DistributedCache.SetMatchLock(BuildLock(userID: 8, matchID: 101, serverID: matchServer.ID));

        HttpResponseMessage response = await webApplicationFactory.CreateClient().PostAsync("stats_requester.php",
            new FormUrlEncodedContent(StatisticsSubmissionTestsHelper.BuildSubmission(matchServer.Cookie, matchServer.ID, matchID: 100)));

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await DistributedCache.GetMatchLock(7)).IsNull();
            await Assert.That((await DistributedCache.GetMatchLock(8))?.MatchID).IsEqualTo(101);
        }
    }

    private static MatchLock BuildLock(int userID, int matchID, int serverID) => new ()
    {
        UserID = userID,
        AccountID = userID,
        AccountName = $"LOCKED-{userID}",
        MatchID = matchID,
        CorrelationID = matchID * 1000 + userID,
        ArrangedMatchType = MatchType.AM_MATCHMAKING,
        ServerID = serverID,
        ServerAddress = "127.0.0.1",
        ServerPort = 11235,
        TimestampStarted = DateTimeOffset.UtcNow
    };
}
