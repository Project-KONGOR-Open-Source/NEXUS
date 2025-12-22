namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    // TODO: Make Match Servers And Match Server Managers Individual Keys Instead Of Hash Entries And Have Expiration Policies That Renew On Status Update

    // TODO: Implement Cleanup Routine To Remove Stale Match Servers And Match Servers Managers (Managers With No Servers Should Also Be Considered Stale)

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

            foreach (MatchServer server in matchServerManager.MatchServers)
            {
                server.MatchServerManager = null;

                await distributedCacheStore.RemoveMatchServerByID(server.ID);
            }

            matchServerManager.MatchServers.Clear();
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

            matchServer.MatchServerManager?.MatchServers.Remove(matchServer);

            if (matchServer.MatchServerManager?.MatchServers.Any() is false)
                await distributedCacheStore.RemoveMatchServerManagerByID(matchServer.MatchServerManager.ID);

            matchServer.MatchServerManager = null;
        }
    }

    private static string ConstructMatchStartDataKey(int matchID) => $@"MATCH-START-DATA:[""{matchID}""]";

    /// <summary>
    ///     Stores match start data in the cache.
    ///     This data will be used to populate match statistics when the match ends.
    /// </summary>
    public static async Task SetMatchStartData(this IDatabase distributedCacheStore, MatchStartData matchStartData)
    {
        string serializedMatchStartData = JsonSerializer.Serialize(matchStartData);

        // Store For A Duration Of Time Longer Than Any Match Is Expected To Last
        await distributedCacheStore.StringSetAsync(ConstructMatchStartDataKey(matchStartData.MatchID), serializedMatchStartData, TimeSpan.FromHours(6));
    }

    /// <summary>
    ///     Retrieves match start data from the cache by match ID.
    ///     Returns NULL if no match start data is found for the given match ID.
    /// </summary>
    public static async Task<MatchStartData?> GetMatchStartData(this IDatabase distributedCacheStore, int matchID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructMatchStartDataKey(matchID));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<MatchStartData>(cachedValue.ToString());
    }

    /// <summary>
    ///     Retrieves the match start data associated with the specified match server ID from the distributed cache.
    /// </summary>
    public static async Task<MatchStartData?> GetMatchStartDataByMatchServerID(this IDatabase distributedCacheStore, int serverID)
    {
        EndPoint endPoint = distributedCacheStore.Multiplexer.GetEndPoints().Single();

        IServer server = distributedCacheStore.Multiplexer.GetServer(endPoint);

        List<MatchStartData> matches = [];

        foreach (RedisKey key in server.Keys(pattern: "MATCH-START-DATA:*"))
        {
            RedisValue cachedValue = await distributedCacheStore.StringGetAsync(key);

            if (cachedValue.IsNullOrEmpty is false)
            {
                MatchStartData? matchStartData = JsonSerializer.Deserialize<MatchStartData>(cachedValue.ToString());

                if (matchStartData is not null && matchStartData.ServerID == serverID)
                {
                    matches.Add(matchStartData);
                }
            }
        }

        return matches.SingleOrDefault();
    }

    /// <summary>
    ///     Removes match start data from the cache.
    ///     This should be called after full match statistics have been created.
    /// </summary>
    public static async Task RemoveMatchStartData(this IDatabase distributedCacheStore, int matchID)
        => await distributedCacheStore.KeyDeleteAsync(ConstructMatchStartDataKey(matchID));
}
