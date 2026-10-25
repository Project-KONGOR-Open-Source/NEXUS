namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Tests for lifting match locks when a match server submits the statistics of their match, which shows that the match is over.
///     Also tests that statistics which have been saved are accepted even when the locks cannot be lifted.
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
        MatchServer matchServer = BuildMatchServer();

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

    [Test]
    public async Task A_Failed_Lock_Release_Still_Accepts_The_Submitted_Statistics()
    {
        MatchServer matchServer = BuildMatchServer();

        await DistributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);

        await MakeLockReleaseFail(matchServer.ID);

        HttpResponseMessage response = await webApplicationFactory.CreateClient().PostAsync("stats_requester.php",
            new FormUrlEncodedContent(StatisticsSubmissionTestsHelper.BuildSubmission(matchServer.Cookie, matchServer.ID, matchID: 100)));

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await MatchStatisticsAreStored(100)).IsTrue();
        }
    }

    [Test]
    public async Task A_Failed_Lock_Release_Still_Accepts_The_Resubmitted_Statistics()
    {
        const string hostAccountName = "RESUBMIT-HOST";
        const string hostPassword = "HOST-PASSWORD";

        await new SRPAuthenticationService(webApplicationFactory).CreateAccountWithSRPCredentials("resubmit-host@kongor.test", hostAccountName, hostPassword);

        await MakeLockReleaseFail(1);

        HttpResponseMessage response = await webApplicationFactory.CreateClient().PostAsync("stats_requester.php",
            new FormUrlEncodedContent(StatisticsSubmissionTestsHelper.BuildResubmission(hostAccountName, hostPassword, serverID: 1, matchID: 100)));

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await MatchStatisticsAreStored(100)).IsTrue();
        }
    }

    /// <summary>
    ///     Makes lifting the locks on the match server fail, by storing a value of another type where the index of its locks belongs.
    /// </summary>
    private async Task MakeLockReleaseFail(int serverID)
        => await DistributedCache.StringSetAsync(DistributedCacheExtensions.ConstructMatchLockServerKey(serverID), "NOT-A-MATCH-LOCK-INDEX");

    private async Task<bool> MatchStatisticsAreStored(int matchID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await merrickContext.MatchStatistics.AnyAsync(statistics => statistics.MatchID == matchID);
    }

    private static MatchServer BuildMatchServer() => new ()
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
