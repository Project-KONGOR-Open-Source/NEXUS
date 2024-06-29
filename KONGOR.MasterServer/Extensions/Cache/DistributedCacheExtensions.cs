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

    public static async Task<(string HostAccountName, MatchServer MatchServer)> GetMatchServerBySessionCookie(this IDatabase distributedCache, string sessionCookie)
    {
        (string hostAccountName, MatchServer matchServer) = (await distributedCache.HashGetAllAsync("MATCH-SERVERS"))
            .Select(entry => (entry.Name.ToString().Split(':').First(), JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())
                ?? throw new NullReferenceException("Deserialized Match Server Is NULL")))
            .SingleOrDefault(tuple => tuple.Item2.Cookie.Equals(sessionCookie));

        return (hostAccountName, matchServer);
    }

    public static async Task<(MatchServerManager MatchServerManager, List<MatchServer> MatchServers)> GetMatchServerManagerAndMatchServersByAccountName(this IDatabase distributedCache, string hostAccountName)
    {
        MatchServerManager matchServerManager = await distributedCache.GetMatchServerManagerByAccountName(hostAccountName);
        List<MatchServer> matchServers = await distributedCache.GetMatchServersByAccountName(hostAccountName);

        return (matchServerManager, matchServers);
    }

    public static async Task<MatchServerManager> GetMatchServerManagerByAccountName(this IDatabase distributedCache, string hostAccountName)
    {
        string? serializedMatchServerManager = await distributedCache.HashGetAsync(MatchServerManagersKey, hostAccountName);

        if (serializedMatchServerManager is null)
            throw new NullReferenceException($@"Serialized Match Server Manager For Host Account Name ""{hostAccountName}"" Is NULL");

        MatchServerManager? matchServerManager = JsonSerializer.Deserialize<MatchServerManager>(serializedMatchServerManager);

        if (matchServerManager is null)
            throw new NullReferenceException($@"Deserialized Match Server Manager For Host Account Name ""{hostAccountName}"" Is NULL");

        return matchServerManager;
    }

    public static async Task<List<MatchServer>> GetMatchServersByAccountName(this IDatabase distributedCache, string hostAccountName)
    {
        List<MatchServer> matchServers = [];

        IAsyncEnumerable<HashEntry> scanResult = distributedCache.HashScanAsync(MatchServersKey, pattern: $"{hostAccountName}:*", pageSize: int.MaxValue);

        await foreach (HashEntry entry in scanResult)
        {
            string serializedMatchServer = entry.Value.ToString();

            MatchServer? matchServer = JsonSerializer.Deserialize<MatchServer>(serializedMatchServer);

            if (matchServer is null)
                throw new NullReferenceException($@"Deserialized Match Server With Key ""{entry.Name}"" Is NULL");

            matchServers.Add(matchServer);
        }

        return matchServers;
    }

    // TODO: Add Extension Method To Remove Match Server Manager By Account Name If No Match Servers Are Associated With The Host Account Name
}
