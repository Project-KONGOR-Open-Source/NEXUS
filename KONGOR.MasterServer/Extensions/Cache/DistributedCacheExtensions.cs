namespace KONGOR.MasterServer.Extensions.Cache;

public static class DistributedCacheExtensions
{
    private const string MatchServerManagersKey = "MATCH-SERVER-MANAGERS";

    /// <summary>
    ///     Sets the specified fields to their respective values in the hash stored at key.
    ///     This command overwrites the values of specified fields that exist in the hash.
    ///     If key doesn't exist, a new key holding a hash is created.
    /// </summary>
    public static async Task SetMatchServerManager(this IDatabase distributedCache, string hostAccountName, MatchServerManager matchServerManager)
    {
        string serializedMatchServerManager = JsonSerializer.Serialize(matchServerManager);
        await distributedCache.HashSetAsync(MatchServerManagersKey, [new HashEntry(hostAccountName, serializedMatchServerManager)]);
    }

    private const string MatchServersKey = "MATCH-SERVERS";

    /// <summary>
    ///     Sets the specified fields to their respective values in the hash stored at key.
    ///     This command overwrites the values of specified fields that exist in the hash.
    ///     If key doesn't exist, a new key holding a hash is created.
    /// </summary>
    public static async Task SetMatchServer(this IDatabase distributedCache, string hostAccountName, MatchServer matchServer)
    {
        string serializedMatchServer = JsonSerializer.Serialize(matchServer);
        await distributedCache.HashSetAsync(MatchServersKey, [new HashEntry($"{hostAccountName}:{matchServer.Instance}", serializedMatchServer)]);
    }

    public static async Task<List<MatchServer>> GetMatchServers(this IDatabase distributedCache)
    {
        List<MatchServer> matchServers = (await distributedCache.HashGetAllAsync(MatchServersKey))
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())).OfType<MatchServer>().ToList();

        return matchServers;
    }

    public static async Task<MatchServer?> GetMatchServerBySessionCookie(this IDatabase distributedCache, string sessionCookie)
    {
        HashEntry[] serializedMatchServers = await distributedCache.HashGetAllAsync(MatchServersKey);

        List<MatchServer> matchServers = serializedMatchServers.Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString()))
            .OfType<MatchServer>().ToList();

        MatchServer? matchServer = matchServers.SingleOrDefault(server => server.Cookie.Equals(sessionCookie));

        return matchServer;
    }

    public static async Task<(MatchServerManager? MatchServerManager, List<MatchServer> MatchServers)> GetMatchServerManagerAndMatchServersByAccountName(this IDatabase distributedCache, string hostAccountName)
    {
        MatchServerManager? matchServerManager = await distributedCache.GetMatchServerManagerByAccountName(hostAccountName);
        List<MatchServer> matchServers = await distributedCache.GetMatchServersByAccountName(hostAccountName);

        return (matchServerManager, matchServers);
    }

    public static async Task<MatchServerManager?> GetMatchServerManagerByAccountName(this IDatabase distributedCache, string hostAccountName)
    {
        string? serializedMatchServerManager = await distributedCache.HashGetAsync(MatchServerManagersKey, hostAccountName);

        if (serializedMatchServerManager is null) return null;

        MatchServerManager matchServerManager = JsonSerializer.Deserialize<MatchServerManager>(serializedMatchServerManager)
            ?? throw new NullReferenceException($@"Unable To Deserialize Match Server Manager With Key ""{hostAccountName}""");

        return matchServerManager;
    }

    public static async Task<List<MatchServer>> GetMatchServersByAccountName(this IDatabase distributedCache, string hostAccountName)
    {
        List<MatchServer> matchServers = [];

        IAsyncEnumerable<HashEntry> scanResult = distributedCache.HashScanAsync(MatchServersKey, pattern: $"{hostAccountName}:*", pageSize: int.MaxValue);

        await foreach (HashEntry entry in scanResult)
        {
            string serializedMatchServer = entry.Value.ToString();

            MatchServer matchServer = JsonSerializer.Deserialize<MatchServer>(serializedMatchServer)
                ?? throw new NullReferenceException($@"Unable To Deserialize Match Server With Key ""{entry.Name}""");

            matchServers.Add(matchServer);
        }

        return matchServers;
    }

    // TODO: Get Server By ID

    // TODO: Add Extension Methods To Remove Match Server Manager And Match Servers By Account Name

    // TODO: Add Extension Method To Remove Match Server Manager By Account Name If No Match Servers Are Associated With The Host Account Name
}
