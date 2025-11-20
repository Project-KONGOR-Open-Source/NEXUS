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
        List<MatchServer> matchServers = (await distributedCacheStore.HashGetAllAsync(MatchServersKey))
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString())).OfType<MatchServer>().ToList();

        return matchServers;
    }

    public static async Task<MatchServer?> GetMatchServerBySessionCookie(this IDatabase distributedCacheStore, string sessionCookie)
    {
        HashEntry[] serializedMatchServers = await distributedCacheStore.HashGetAllAsync(MatchServersKey);

        List<MatchServer> matchServers = serializedMatchServers.Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString()))
            .OfType<MatchServer>().ToList();

        MatchServer? matchServer = matchServers.SingleOrDefault(server => server.Cookie.Equals(sessionCookie));

        return matchServer;
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

    // TODO: Get Server By ID

    // TODO: Add Extension Methods To Remove Match Server Manager And Match Servers By Account Name

    // TODO: Add Extension Method To Remove Match Server Manager By Account Name If No Match Servers Are Associated With The Host Account Name
}
