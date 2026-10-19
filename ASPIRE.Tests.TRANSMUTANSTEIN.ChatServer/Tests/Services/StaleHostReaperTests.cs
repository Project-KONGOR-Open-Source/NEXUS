namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Tests for the reconciliation logic that decides which cached hosts the stale host reaper removes.
///     Also tests that reaping a match server manager leaves its copy-on-write master in place, and that a copy-on-write master is reaped once its heartbeat is stale.
/// </summary>
public sealed class StaleHostReaperTests(TRANSMUTANSTEINIntegrationWebApplicationFactory webApplicationFactory)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UnixEpoch.AddDays(1);

    private static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(2);

    [Test]
    public async Task A_Host_With_A_Live_Session_Is_Never_Reaped()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1, 2], [1, 2], new Dictionary<int, DateTimeOffset>(), Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task A_Missing_Host_Is_Marked_On_First_Observation_And_Not_Reaped()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [], new Dictionary<int, DateTimeOffset>(), Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.ContainsKey(1)).IsTrue();
            await Assert.That(nextFirstObservedMissing[1]).IsEqualTo(Now);
        }
    }

    [Test]
    public async Task A_Missing_Host_Within_The_Grace_Period_Is_Not_Reaped_And_Retains_Its_Original_Mark()
    {
        DateTimeOffset firstObserved = Now - TimeSpan.FromSeconds(30);

        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [], new Dictionary<int, DateTimeOffset> { [1] = firstObserved }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing[1]).IsEqualTo(firstObserved);
        }
    }

    [Test]
    public async Task A_Missing_Host_Beyond_The_Grace_Period_Is_Reaped()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [], new Dictionary<int, DateTimeOffset> { [1] = Now - TimeSpan.FromMinutes(3) }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Contains(1)).IsTrue();
            await Assert.That(nextFirstObservedMissing.ContainsKey(1)).IsFalse();
        }
    }

    [Test]
    public async Task A_Host_That_Reconnected_Within_The_Grace_Period_Clears_Its_Mark()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([1], [1], new Dictionary<int, DateTimeOffset> { [1] = Now - TimeSpan.FromSeconds(90) }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.ContainsKey(1)).IsFalse();
        }
    }

    [Test]
    public async Task A_Host_No_Longer_In_The_Cache_Is_Dropped_From_The_Pending_Marks()
    {
        (List<int> toReap, Dictionary<int, DateTimeOffset> nextFirstObservedMissing) =
            StaleHostReaper.Reconcile([], [], new Dictionary<int, DateTimeOffset> { [1] = Now - TimeSpan.FromSeconds(30) }, Now, GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(toReap.Count).IsEqualTo(0);
            await Assert.That(nextFirstObservedMissing.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task A_Session_Within_The_Stale_Threshold_Is_Not_Stale()
    {
        await Assert.That(StaleHostReaper.SessionIsStale(Now - TimeSpan.FromSeconds(174), Now, TimeSpan.FromSeconds(175))).IsFalse();
    }

    [Test]
    public async Task A_Session_Exactly_At_The_Stale_Threshold_Is_Not_Stale()
    {
        await Assert.That(StaleHostReaper.SessionIsStale(Now - TimeSpan.FromSeconds(175), Now, TimeSpan.FromSeconds(175))).IsFalse();
    }

    [Test]
    public async Task A_Session_Beyond_The_Stale_Threshold_Is_Stale()
    {
        await Assert.That(StaleHostReaper.SessionIsStale(Now - TimeSpan.FromSeconds(176), Now, TimeSpan.FromSeconds(175))).IsTrue();
    }

    [Test]
    public async Task Reaping_A_Match_Server_Manager_Leaves_Its_Copy_On_Write_Master()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        const string hostAccountName = "REAPED-HOST";

        MatchServerManager matchServerManager = new ()
        {
            HostAccountID = 1,
            HostAccountName = hostAccountName,
            ID = hostAccountName.GetDeterministicInt32Hash(),
            MatchServerIDs = [],
            IPAddress = "127.0.0.1"
        };

        // The Copy-On-Write Master Is Still Sending Heartbeats When Its Manager Is Reaped
        CopyOnWriteMaster copyOnWriteMaster = CreateCopyOnWriteMaster(hostAccountName, matchServerManager.ID, Now + GracePeriod);

        await distributedCache.SetMatchServerManager(hostAccountName, matchServerManager);
        await distributedCache.SetCopyOnWriteMaster(copyOnWriteMaster);

        StaleHostReaper staleHostReaper = new (distributedCache, NullLogger<StaleHostReaper>.Instance);

        // The Manager Has No Live Chat Session, So The First Sweep Marks It As Missing And The Sweep After The Grace Period Reaps It
        await staleHostReaper.Sweep(Now);
        await staleHostReaper.Sweep(Now + GracePeriod);

        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetMatchServerManagerByID(matchServerManager.ID)).IsNull();
            await Assert.That(await distributedCache.GetCopyOnWriteMasterBySessionCookie(copyOnWriteMaster.Cookie)).IsNotNull();
        }
    }

    [Test]
    public async Task A_Copy_On_Write_Master_Is_Reaped_Only_Once_Its_Heartbeat_Is_Stale()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        // A Copy-On-Write Master Has No Chat Session, So It Is Judged Only By Its Last Heartbeat, Against The Limit Which Applies To A Match Server's Last Status Update
        CopyOnWriteMaster liveCopyOnWriteMaster = CreateCopyOnWriteMaster("LIVE-HOST", null, Now - StaleHostReaper.MatchServerSessionStaleStateThreshold);

        CopyOnWriteMaster staleCopyOnWriteMaster = CreateCopyOnWriteMaster("STALE-HOST", null, Now - StaleHostReaper.MatchServerSessionStaleStateThreshold - TimeSpan.FromSeconds(1));

        await distributedCache.SetCopyOnWriteMaster(liveCopyOnWriteMaster);
        await distributedCache.SetCopyOnWriteMaster(staleCopyOnWriteMaster);

        StaleHostReaper staleHostReaper = new (distributedCache, NullLogger<StaleHostReaper>.Instance);

        await staleHostReaper.Sweep(Now);

        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetCopyOnWriteMasterBySessionCookie(liveCopyOnWriteMaster.Cookie)).IsNotNull();
            await Assert.That(await distributedCache.GetCopyOnWriteMasterBySessionCookie(staleCopyOnWriteMaster.Cookie)).IsNull();
        }
    }

    private static CopyOnWriteMaster CreateCopyOnWriteMaster(string hostAccountName, int? matchServerManagerID, DateTimeOffset lastHeartbeat) => new ()
    {
        HostAccountID = 1,
        HostAccountName = hostAccountName,
        ID = $"{hostAccountName}:".GetDeterministicInt32Hash(),
        Name = "TEST-SERVER",
        MatchServerManagerID = matchServerManagerID,
        IPAddress = "127.0.0.1",
        Port = 11235,
        Location = "EU",
        Description = "Test Server",
        TimestampLastHeartbeat = lastHeartbeat
    };
}
