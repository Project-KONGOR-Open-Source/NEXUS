namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private const string MatchServerManagersKey = "MATCH-SERVER-MANAGERS";

    /// <summary>
    ///     Sets the specified fields to their respective values in the hash stored at key.
    ///     This command overwrites the values of specified fields that exist in the hash.
    ///     If key doesn't exist, a new key holding a hash is created.
    /// </summary>
    public static async Task SetMatchServerManager(this IDatabase distributedCacheStore, string hostAccountName, MatchServerManager matchServerManager)
    {
        string serializedMatchServerManager = JsonSerializer.Serialize(matchServerManager);

        await distributedCacheStore.HashSetAsync(MatchServerManagersKey, [new HashEntry(hostAccountName, serializedMatchServerManager)]);
    }

    public static async Task<List<MatchServerManager>> GetMatchServerManagers(this IDatabase distributedCacheStore)
    {
        HashEntry[] serializedMatchServerManagers = await distributedCacheStore.HashGetAllAsync(MatchServerManagersKey);

        List<MatchServerManager> matchServerManagers = [.. serializedMatchServerManagers
            .Select(entry => JsonSerializer.Deserialize<MatchServerManager>(entry.Value.ToString())).OfType<MatchServerManager>()];

        return matchServerManagers;
    }

    public static async Task<MatchServerManager?> GetMatchServerManagerByIPAddress(this IDatabase distributedCacheStore, string ipAddress)
    {
        HashEntry[] serializedMatchServerManagers = await distributedCacheStore.HashGetAllAsync(MatchServerManagersKey);

        List<MatchServerManager> matchServerManagers = [.. serializedMatchServerManagers
            .Select(entry => JsonSerializer.Deserialize<MatchServerManager>(entry.Value.ToString())).OfType<MatchServerManager>()];

        MatchServerManager? matchServerManager = matchServerManagers.SingleOrDefault(manager => manager.IPAddress.Equals(ipAddress));

        return matchServerManager;
    }

    public static async Task<MatchServerManager?> GetMatchServerManagerBySessionCookie(this IDatabase distributedCacheStore, string sessionCookie)
    {
        HashEntry[] serializedMatchServerManagers = await distributedCacheStore.HashGetAllAsync(MatchServerManagersKey);

        List<MatchServerManager> matchServerManagers = [.. serializedMatchServerManagers
            .Select(entry => JsonSerializer.Deserialize<MatchServerManager>(entry.Value.ToString())).OfType<MatchServerManager>()];

        MatchServerManager? matchServerManager = matchServerManagers.SingleOrDefault(manager => manager.Cookie.Equals(sessionCookie));

        return matchServerManager;
    }

    public static async Task<List<MatchServerManager>> GetMatchServerManagersByAccountName(this IDatabase distributedCacheStore, string hostAccountName)
    {
        List<MatchServerManager> matchServerManagers = [];

        IAsyncEnumerable<HashEntry> scanResult = distributedCacheStore.HashScanAsync(MatchServerManagersKey, pattern: hostAccountName, pageSize: int.MaxValue);

        await foreach (HashEntry entry in scanResult)
        {
            string serializedMatchServerManager = entry.Value.ToString();

            MatchServerManager matchServerManager = JsonSerializer.Deserialize<MatchServerManager>(serializedMatchServerManager)
                ?? throw new NullReferenceException($@"Unable To Deserialize Match Server Manager With Key ""{entry.Name}""");

            matchServerManagers.Add(matchServerManager);
        }

        return matchServerManagers;
    }

    public static async Task<MatchServerManager?> GetMatchServerManagerByID(this IDatabase distributedCacheStore, int serverManagerID)
    {
        HashEntry[] serializedMatchServerManagers = await distributedCacheStore.HashGetAllAsync(MatchServerManagersKey);

        List<MatchServerManager> matchServerManagers = [.. serializedMatchServerManagers
            .Select(entry => JsonSerializer.Deserialize<MatchServerManager>(entry.Value.ToString())).OfType<MatchServerManager>()];

        MatchServerManager? matchServerManager = matchServerManagers.SingleOrDefault(manager => manager.ID == serverManagerID);

        return matchServerManager;
    }

    public static async Task RemoveMatchServerManagerByID(this IDatabase distributedCacheStore, int serverManagerID)
    {
        MatchServerManager? matchServerManager = await distributedCacheStore.GetMatchServerManagerByID(serverManagerID);

        if (matchServerManager is not null)
        {
            string hostAccountName = matchServerManager.HostAccountName;

            await distributedCacheStore.HashDeleteAsync(MatchServerManagersKey, hostAccountName);

            foreach (int matchServerID in matchServerManager.MatchServerIDs)
            {
                MatchServer? matchServer = await distributedCacheStore.GetMatchServerByID(matchServerID);

                if (matchServer is not null)
                {
                    matchServer.MatchServerManagerID = null;

                    await distributedCacheStore.RemoveMatchServerByID(matchServer.ID);
                }
            }

            matchServerManager.MatchServerIDs.Clear();
        }
    }

    private const string MatchServersKey = "MATCH-SERVERS";

    /// <summary>
    ///     Sets the specified fields to their respective values in the hash stored at key.
    ///     This command overwrites the values of specified fields that exist in the hash.
    ///     If key doesn't exist, a new key holding a hash is created.
    /// </summary>
    public static async Task SetMatchServer(this IDatabase distributedCacheStore, string hostAccountName, MatchServer matchServer)
    {
        string serializedMatchServer = JsonSerializer.Serialize(matchServer);

        await distributedCacheStore.HashSetAsync(MatchServersKey, [new HashEntry($"{hostAccountName}:{matchServer.Instance}", serializedMatchServer)]);
    }

    public static async Task<List<MatchServer>> GetMatchServers(this IDatabase distributedCacheStore)
    {
        HashEntry[] serializedMatchServers = await distributedCacheStore.HashGetAllAsync(MatchServersKey);

        List<MatchServer> matchServers = [.. serializedMatchServers
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())).OfType<MatchServer>()];

        return matchServers;
    }

    public static async Task<MatchServer?> GetMatchServerByIPAddressAndPort(this IDatabase distributedCacheStore, string ipAddress, int port)
    {
        HashEntry[] serializedMatchServers = await distributedCacheStore.HashGetAllAsync(MatchServersKey);

        List<MatchServer> matchServers = [.. serializedMatchServers
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())).OfType<MatchServer>()];

        MatchServer? matchServer = matchServers.SingleOrDefault(server => server.IPAddress.Equals(ipAddress) && server.Port == port);

        return matchServer;
    }

    public static async Task<MatchServer?> GetMatchServerBySessionCookie(this IDatabase distributedCacheStore, string sessionCookie)
    {
        HashEntry[] serializedMatchServers = await distributedCacheStore.HashGetAllAsync(MatchServersKey);

        List<MatchServer> matchServers = [.. serializedMatchServers
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())).OfType<MatchServer>()];

        MatchServer? matchServer = matchServers.SingleOrDefault(server => server.Cookie.Equals(sessionCookie));

        return matchServer;
    }

    public static async Task<List<MatchServer>> GetMatchServersByAccountName(this IDatabase distributedCacheStore, string hostAccountName)
    {
        List<MatchServer> matchServers = [];

        IAsyncEnumerable<HashEntry> scanResult = distributedCacheStore.HashScanAsync(MatchServersKey, pattern: $"{hostAccountName}:*", pageSize: int.MaxValue);

        await foreach (HashEntry entry in scanResult)
        {
            string serializedMatchServer = entry.Value.ToString();

            MatchServer matchServer = JsonSerializer.Deserialize<MatchServer>(serializedMatchServer)
                ?? throw new NullReferenceException($@"Unable To Deserialize Match Server With Key ""{entry.Name}""");

            matchServers.Add(matchServer);
        }

        return matchServers;
    }

    public static async Task<MatchServer?> GetMatchServerByID(this IDatabase distributedCacheStore, int serverID)
    {
        HashEntry[] serializedMatchServers = await distributedCacheStore.HashGetAllAsync(MatchServersKey);

        List<MatchServer> matchServers = [.. serializedMatchServers
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())).OfType<MatchServer>()];

        MatchServer? matchServer = matchServers.SingleOrDefault(server => server.ID == serverID);

        return matchServer;
    }

    public static async Task RemoveMatchServerByID(this IDatabase distributedCacheStore, int serverID)
    {
        MatchServer? matchServer = await distributedCacheStore.GetMatchServerByID(serverID);

        if (matchServer is not null)
        {
            string hashField = $"{matchServer.HostAccountName}:{matchServer.Instance}";

            await distributedCacheStore.HashDeleteAsync(MatchServersKey, hashField);

            MatchServerManager? matchServerManager = await distributedCacheStore.GetMatchServerManagerByID(matchServer.MatchServerManagerID ?? default);

            if (matchServerManager is not null)
            {
                matchServerManager.MatchServerIDs.Remove(matchServer.ID);

                if (matchServerManager.MatchServerIDs.Any() is false)
                    await distributedCacheStore.RemoveMatchServerManagerByID(matchServerManager.ID);
            }

            matchServer.MatchServerManagerID = null;
        }
    }

    // The Hosting Lease Restricts An Open-Password Host Account (e.g. OPERATOR) To A Single Concurrent Host
    // The Lease Is Anchored On The Server Manager Session: The Manager Claims It, Match Servers Require And Renew It, And It Is Released When The Match Server Manager Disconnects
    // It Is Time-To-Live-Backed So That It Self-Heals If The Holding Host Crashes Without A Clean Disconnect

    // The Time-To-Live Is Chosen To Comfortably Exceed The Server Heartbeat Interval, So That An Actively-Hosting Machine Always Renews It In Time, While A Crashed Host Releases It Within A Few Minutes
    private static readonly TimeSpan HostLeaseTimeToLive = TimeSpan.FromMinutes(5);

    private static string ConstructHostLeaseKey(string hostAccountName) => $"MATCH-HOST-LEASE:{hostAccountName}";

    /// <summary>
    ///     Attempts to atomically claim the single-holder hosting lease for the specified host account.
    ///     The claim succeeds only if no lease is currently held.
    /// </summary>
    /// <param name="distributedCacheStore">The distributed cache store.</param>
    /// <param name="hostAccountName">The host account name to claim the lease for.</param>
    /// <returns><see langword="true"/> if the lease was claimed by this call, <see langword="false"/> if it is already held.</returns>
    public static async Task<bool> TryClaimHostLease(this IDatabase distributedCacheStore, string hostAccountName)
        => await distributedCacheStore.StringSetAsync(ConstructHostLeaseKey(hostAccountName), DateTimeOffset.UtcNow.ToString("O"), HostLeaseTimeToLive, When.NotExists);

    /// <summary>
    ///     Determines whether the single-holder hosting lease for the specified host account is currently held.
    /// </summary>
    public static async Task<bool> IsHostLeaseHeld(this IDatabase distributedCacheStore, string hostAccountName)
        => await distributedCacheStore.KeyExistsAsync(ConstructHostLeaseKey(hostAccountName));

    /// <summary>
    ///     Renews the time-to-live of an existing hosting lease for the specified host account.
    ///     Has no effect if the lease is not currently held.
    /// </summary>
    public static async Task RenewHostLease(this IDatabase distributedCacheStore, string hostAccountName)
        => await distributedCacheStore.KeyExpireAsync(ConstructHostLeaseKey(hostAccountName), HostLeaseTimeToLive);

    /// <summary>
    ///     Releases the single-holder hosting lease for the specified host account.
    /// </summary>
    public static async Task ReleaseHostLease(this IDatabase distributedCacheStore, string hostAccountName)
        => await distributedCacheStore.KeyDeleteAsync(ConstructHostLeaseKey(hostAccountName));

    private static string ConstructMatchInformationKey(int matchID) => $@"MATCH-INFORMATION:[""{matchID}""]";

    /// <summary>
    ///     Stores match information in the cache.
    ///     This data is used for creating tentative matches and is removed once the associated tentative match is confirmed and materialises into a real match.
    /// </summary>
    public static async Task SetMatchInformation(this IDatabase distributedCacheStore, MatchInformation matchInformation)
    {
        string serializedMatchInformation = JsonSerializer.Serialize(matchInformation);

        // One Hour Is More Than Sufficient For Any Tentative Match To Materialise Into A Real Match; This Time Span Should Essentially Always Be Longer Than Any Possible/Realistic Queue Time
        await distributedCacheStore.StringSetAsync(ConstructMatchInformationKey(matchInformation.MatchID), serializedMatchInformation, TimeSpan.FromHours(1));
    }

    /// <summary>
    ///     Retrieves match information from the cache by match ID.
    ///     Returns <see langword="null"/> if no match information is found for the given match ID.
    /// </summary>
    public static async Task<MatchInformation?> GetMatchInformation(this IDatabase distributedCacheStore, int matchID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructMatchInformationKey(matchID));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<MatchInformation>(cachedValue.ToString());
    }

    /// <summary>
    ///     Retrieves the match information associated with the specified match server ID from the distributed cache.
    ///     If multiple match entries exist for the same server ID (e.g. due to cache entries for previous matches on the same server which have not yet expired), the most recently started match is returned.
    /// </summary>
    public static async Task<MatchInformation?> GetMatchInformationByMatchServerID(this IDatabase distributedCacheStore, int serverID)
    {
        EndPoint endPoint = distributedCacheStore.Multiplexer.GetEndPoints().Single();

        IServer server = distributedCacheStore.Multiplexer.GetServer(endPoint);

        List<MatchInformation> matches = [];

        foreach (RedisKey key in server.Keys(pattern: "MATCH-INFORMATION:*"))
        {
            RedisValue cachedValue = await distributedCacheStore.StringGetAsync(key);

            if (cachedValue.IsNullOrEmpty is false)
            {
                MatchInformation? matchInformation = JsonSerializer.Deserialize<MatchInformation>(cachedValue.ToString());

                if (matchInformation is not null && matchInformation.ServerID == serverID)
                {
                    matches.Add(matchInformation);
                }
            }
        }

        return matches.OrderByDescending(match => match.TimestampStarted).FirstOrDefault();
    }

    public static async Task<MatchInformation?> GetMatchInformationByMatchServerSessionCookie(this IDatabase distributedCacheStore, string sessionCookie)
    {
        MatchServer? matchServer = await distributedCacheStore.GetMatchServerBySessionCookie(sessionCookie);

        if (matchServer is null) return null;

        return await distributedCacheStore.GetMatchInformationByMatchServerID(matchServer.ID);
    }

    /// <summary>
    ///     Removes match information from the cache.
    ///     Called after match statistics have been submitted and the match information snapshot has been persisted to the database.
    ///     From this point forward, the database snapshot is the single source of truth for match information.
    /// </summary>
    public static async Task RemoveMatchInformation(this IDatabase distributedCacheStore, int matchID)
        => await distributedCacheStore.KeyDeleteAsync(ConstructMatchInformationKey(matchID));
}
