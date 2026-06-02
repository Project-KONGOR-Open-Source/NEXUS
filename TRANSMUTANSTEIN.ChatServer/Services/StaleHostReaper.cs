namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Periodically reconciles the distributed cache of match servers and match server managers against the live in-memory chat sessions, removing cache entries that have had no live session for longer than a grace period.
///     This reaps hosts that registered through HTTP authentication but never completed (or lost) their chat handshake, and orphaned entries left behind when the chat server restarts, neither of which the per-session disconnect cleanup can catch.
///     The reconciliation treats the in-memory session pools as the authoritative record of live hosts, which holds for the single-instance chat server because those pools are process-local static state.
/// </summary>
public sealed class StaleHostReaper(IDatabase distributedCacheStore, ILogger<StaleHostReaper> logger) : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromSeconds(60);

    /// <summary>
    ///     A grace period comfortably larger than the host reconnection window (approximately fifteen to forty-five seconds) so that briefly-disconnected or newly-authenticated hosts are not reaped before they establish or re-establish their chat session.
    /// </summary>
    private static readonly TimeSpan StaleGracePeriod = TimeSpan.FromMinutes(2);

    private Dictionary<int, DateTimeOffset> MatchServersFirstObservedMissing { get; set; } = [];

    private Dictionary<int, DateTimeOffset> MatchServerManagersFirstObservedMissing { get; set; } = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait For One Interval Before The First Sweep So That The Cache Has Settled After Startup
        try { await Task.Delay(SweepInterval, stoppingToken); } catch (OperationCanceledException) { return; }

        while (stoppingToken.IsCancellationRequested is false)
        {
            try
            {
                await Sweep();
            }

            catch (Exception exception)
            {
                logger.LogError(exception, "Stale Host Reaper Sweep Failed");
            }

            try { await Task.Delay(SweepInterval, stoppingToken); } catch (OperationCanceledException) { return; }
        }
    }

    private async Task Sweep()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Reconcile Match Servers
        List<MatchServer> matchServers = await distributedCacheStore.GetMatchServers();

        (List<int> staleMatchServerIDs, Dictionary<int, DateTimeOffset> nextMatchServersFirstObservedMissing) = Reconcile(
            matchServers.Select(matchServer => matchServer.ID), Context.MatchServerChatSessions.Keys, MatchServersFirstObservedMissing, now, StaleGracePeriod);

        MatchServersFirstObservedMissing = nextMatchServersFirstObservedMissing;

        foreach (int matchServerID in staleMatchServerIDs)
        {
            logger.LogInformation(@"Reaping Stale Match Server ID ""{MatchServerID}"" Which Has Had No Live Chat Session For Longer Than The Grace Period", matchServerID);

            await distributedCacheStore.RemoveMatchServerByID(matchServerID);
        }

        // Reconcile Match Server Managers
        List<MatchServerManager> matchServerManagers = await distributedCacheStore.GetMatchServerManagers();

        (List<int> staleMatchServerManagerIDs, Dictionary<int, DateTimeOffset> nextMatchServerManagersFirstObservedMissing) = Reconcile(
            matchServerManagers.Select(matchServerManager => matchServerManager.ID), Context.MatchServerManagerChatSessions.Keys, MatchServerManagersFirstObservedMissing, now, StaleGracePeriod);

        MatchServerManagersFirstObservedMissing = nextMatchServerManagersFirstObservedMissing;

        foreach (int matchServerManagerID in staleMatchServerManagerIDs)
        {
            MatchServerManager? matchServerManager = matchServerManagers.SingleOrDefault(candidate => candidate.ID == matchServerManagerID);

            logger.LogInformation(@"Reaping Stale Match Server Manager ID ""{MatchServerManagerID}"" Which Has Had No Live Chat Session For Longer Than The Grace Period", matchServerManagerID);

            await distributedCacheStore.RemoveMatchServerManagerByID(matchServerManagerID);

            // Release The Hosting Lease For A Reaped Manager So The Account Becomes Claimable Again Without Waiting For The Lease To Expire
            if (matchServerManager is not null)
                await distributedCacheStore.ReleaseHostLease(matchServerManager.HostAccountName);
        }
    }

    /// <summary>
    ///     Determines which cached host identifiers should be reaped: those without a live in-memory session that have remained missing for at least the grace period.
    ///     Returns the identifiers to reap and the next "first observed missing" map to carry into the following sweep, so that an entry must be continuously missing across the grace period before it is removed.
    /// </summary>
    /// <param name="cachedIdentifiers">The host identifiers currently present in the distributed cache.</param>
    /// <param name="liveIdentifiers">The host identifiers with a live in-memory chat session.</param>
    /// <param name="firstObservedMissing">The instant at which each currently-missing identifier was first observed to be missing, carried over from the previous sweep.</param>
    /// <param name="now">The current instant.</param>
    /// <param name="gracePeriod">The duration a cached identifier must remain missing before it is reaped.</param>
    public static (List<int> ToReap, Dictionary<int, DateTimeOffset> NextFirstObservedMissing) Reconcile
    (
        IEnumerable<int> cachedIdentifiers, IEnumerable<int> liveIdentifiers, IReadOnlyDictionary<int, DateTimeOffset> firstObservedMissing, DateTimeOffset now, TimeSpan gracePeriod
    )
    {
        HashSet<int> liveIdentifierSet = [.. liveIdentifiers];

        List<int> toReap = [];
        Dictionary<int, DateTimeOffset> nextFirstObservedMissing = [];

        foreach (int identifier in cachedIdentifiers)
        {
            // A Live Session Exists, So The Host Is Not Stale; Any Pending Mark Is Cleared By Not Carrying It Forward
            if (liveIdentifierSet.Contains(identifier))
                continue;

            DateTimeOffset observedMissingAt = firstObservedMissing.TryGetValue(identifier, out DateTimeOffset previouslyObserved) ? previouslyObserved : now;

            if (now - observedMissingAt >= gracePeriod)
                toReap.Add(identifier);

            else
                nextFirstObservedMissing[identifier] = observedMissingAt;
        }

        return (toReap, nextFirstObservedMissing);
    }
}
