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

    public static async Task<MatchServer?> GetMatchServerBySessionCookie(this IDatabase distributedCacheStore, string sessionCookie)
    {
        HashEntry[] serializedMatchServers = await distributedCacheStore.HashGetAllAsync(MatchServersKey);

        List<MatchServer> matchServers = [.. serializedMatchServers
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())).OfType<MatchServer>()];

        MatchServer? matchServer = matchServers.SingleOrDefault(server => server.Cookie.Equals(sessionCookie));

        return matchServer;
    }

    public static async Task<MatchServerManager?> GetMatchServerManagerBySessionCookie(this IDatabase distributedCacheStore, string sessionCookie)
    {
        HashEntry[] serializedMatchServerManagers = await distributedCacheStore.HashGetAllAsync(MatchServerManagersKey);

        List<MatchServerManager> matchServerManagers = [.. serializedMatchServerManagers
            .Select(entry => JsonSerializer.Deserialize<MatchServerManager>(entry.Value.ToString())).OfType<MatchServerManager>()];

        MatchServerManager? matchServerManager = matchServerManagers.SingleOrDefault(manager => manager.Cookie.Equals(sessionCookie));

        return matchServerManager;
    }

    public static async Task<(MatchServerManager? MatchServerManager, List<MatchServer> MatchServers)> GetMatchServerManagerAndMatchServersByAccountName(this IDatabase distributedCacheStore, string hostAccountName)
    {
        MatchServerManager? matchServerManager = await distributedCacheStore.GetMatchServerManagerByAccountName(hostAccountName);
        List<MatchServer> matchServers = await distributedCacheStore.GetMatchServersByAccountName(hostAccountName);

        return (matchServerManager, matchServers);
    }

    public static async Task<MatchServerManager?> GetMatchServerManagerByAccountName(this IDatabase distributedCacheStore, string hostAccountName)
    {
        string? serializedMatchServerManager = await distributedCacheStore.HashGetAsync(MatchServerManagersKey, hostAccountName);

        if (serializedMatchServerManager is null) return null;

        MatchServerManager matchServerManager = JsonSerializer.Deserialize<MatchServerManager>(serializedMatchServerManager)
            ?? throw new NullReferenceException($@"Unable To Deserialize Match Server Manager With Key ""{hostAccountName}""");

        return matchServerManager;
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

    public static async Task<bool> RemoveMatchServer(this IDatabase distributedCacheStore, string hostAccountName, int instance)
    {
        bool removed = await distributedCacheStore.HashDeleteAsync(MatchServersKey, $"{hostAccountName}:{instance}");

        List<MatchServer> matchServers = await distributedCacheStore.GetMatchServersByAccountName(hostAccountName);

        if (matchServers.Any() is false)
            await distributedCacheStore.RemoveMatchServerManager(hostAccountName);

        return removed;
    }

    public static async Task<int> RemoveMatchServersByAccountName(this IDatabase distributedCacheStore, string hostAccountName)
    {
        List<MatchServer> matchServers = await distributedCacheStore.GetMatchServersByAccountName(hostAccountName);

        if (matchServers.Any())
        {
            RedisValue[] keys = [.. matchServers.Select(server => (RedisValue) $"{hostAccountName}:{server.Instance}")];

            long removedCount = await distributedCacheStore.HashDeleteAsync(MatchServersKey, keys);

            await distributedCacheStore.RemoveMatchServerManager(hostAccountName);

            return Convert.ToInt32(removedCount);
        }

        else
        {
            await distributedCacheStore.RemoveMatchServerManager(hostAccountName);

            return 0;
        }
    }

    public static async Task<bool> RemoveMatchServerManager(this IDatabase distributedCacheStore, string hostAccountName)
    {
        bool removed = await distributedCacheStore.HashDeleteAsync(MatchServerManagersKey, hostAccountName);

        return removed;
    }
}
