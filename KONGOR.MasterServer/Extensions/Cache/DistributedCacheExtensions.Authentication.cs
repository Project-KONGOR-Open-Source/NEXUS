namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructSRPAuthenticationSessionDataKey(string accountName) => $@"SRP-SESSION-DATA:[""{accountName}""]";

    public static async Task SetSRPAuthenticationSessionData(this IDatabase distributedCacheStore, string accountName, SRPAuthenticationSessionDataStageOne data)
    {
        string serializedData = JsonSerializer.Serialize(data);

        await distributedCacheStore.StringSetAsync(ConstructSRPAuthenticationSessionDataKey(accountName), serializedData, TimeSpan.FromSeconds(30));
    }

    public static async Task<SRPAuthenticationSessionDataStageOne?> GetSRPAuthenticationSessionData(this IDatabase distributedCacheStore, string accountName)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructSRPAuthenticationSessionDataKey(accountName));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<SRPAuthenticationSessionDataStageOne>(cachedValue.ToString());
    }

    public static async Task RemoveSRPAuthenticationSessionData(this IDatabase distributedCacheStore, string accountName)
        => await distributedCacheStore.KeyDeleteAsync(ConstructSRPAuthenticationSessionDataKey(accountName));

    private static string ConstructSRPAuthenticationSystemInformationKey(string accountName) => $@"SYSTEM-INFORMATION:[""{accountName}""]";

    public static async Task SetSRPAuthenticationSystemInformation(this IDatabase distributedCacheStore, string accountName, string systemInformation)
        => await distributedCacheStore.StringSetAsync(ConstructSRPAuthenticationSystemInformationKey(accountName), systemInformation, TimeSpan.FromSeconds(30));

    public static async Task<string?> GetSRPAuthenticationSystemInformation(this IDatabase distributedCacheStore, string accountName)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructSRPAuthenticationSystemInformationKey(accountName));

        return cachedValue.IsNullOrEmpty ? null : cachedValue.ToString();
    }

    public static async Task RemoveSRPAuthenticationSystemInformation(this IDatabase distributedCacheStore, string accountName)
        => await distributedCacheStore.KeyDeleteAsync(ConstructSRPAuthenticationSystemInformationKey(accountName));

    // The Cookie Is Stored In The Key Because Most Requests From HoN Are Made With Just A Cookie But No Account Information
    // Additionally, Cached Values Cannot Be Retrieved Without Their Respective Key, So Iterating Just The Values Is Not Possible Without Complex Reflection
    internal static string ConstructAccountSessionCookieKey(string cookie) => $@"ACCOUNT-SESSION-COOKIE:[""{cookie}""]";

    /// <summary>
    ///     How long a session cookie lives, which is also how long the records tied to a session cookie live.
    /// </summary>
    internal static readonly TimeSpan SessionCookieLifetime = TimeSpan.FromHours(24);

    /// <summary>
    ///     How many attempts a login makes to retire the earlier logins of its user before it fails.
    ///     An attempt is only lost to another login of the same user which committed in the meantime, and each such login can defeat an attempt only once, so a login among this many concurrent logins of one user always succeeds.
    /// </summary>
    internal const int SessionCookieSupersessionAttempts = 10;

    public static async Task SetAccountNameForSessionCookie(this IDatabase distributedCacheStore, string cookie, string accountName)
        => await distributedCacheStore.StringSetAsync(ConstructAccountSessionCookieKey(cookie), accountName, SessionCookieLifetime);

    /// <summary>
    ///     Logs an account in with the session cookie, and retires the earlier login of the account and the logins of the other given accounts, which are the other accounts of the account's user, so that a user holds one live login, as retail did.
    ///     In one transaction, the session cookie is stored for the account name, the account's latest-login record points at it, every session cookie which the records of the account and of the other accounts point at stops resolving, and the other accounts' records are deleted.
    ///     A login is never refused because another login is live, so that a player can always log in again, after a crash included.
    ///     Returns the retired session cookies, and throws an exception once <see cref="SessionCookieSupersessionAttempts"/> attempts have lost their races to other logins of the same user.
    /// </summary>
    public static async Task<List<string>> SupersedeSessionCookies(this IDatabase distributedCacheStore, int accountID, string accountName, string cookie, IReadOnlyCollection<int> otherAccountIDs)
    {
        RedisKey accountKey = ConstructAccountLatestSessionCookieKey(accountID);

        RedisKey[] otherAccountKeys = [.. otherAccountIDs.Where(otherAccountID => otherAccountID != accountID).Distinct().Select(otherAccountID => (RedisKey) ConstructAccountLatestSessionCookieKey(otherAccountID))];

        RedisKey[] keys = [accountKey, .. otherAccountKeys];

        for (int attempt = 1; attempt <= SessionCookieSupersessionAttempts; attempt++)
        {
            RedisValue[] records = await distributedCacheStore.StringGetAsync(keys);

            // The Logins Are Only Retired If Every Record Still Holds What Was Read, So That A Login Of The Same User Which Committed In The Meantime Is Retired By The Next Attempt Rather Than Left Live Beside This One
            ITransaction transaction = distributedCacheStore.CreateTransaction();

            for (int index = 0; index < keys.Length; index++)
                transaction.AddCondition(records[index].IsNull ? Condition.KeyNotExists(keys[index]) : Condition.StringEqual(keys[index], records[index]));

            List<string> retiredCookies = [.. records.Where(record => record.IsNullOrEmpty is false).Select(record => record.ToString())];

            foreach (string retiredCookie in retiredCookies)
                _ = transaction.KeyDeleteAsync(ConstructAccountSessionCookieKey(retiredCookie));

            foreach (RedisKey otherAccountKey in otherAccountKeys)
                _ = transaction.KeyDeleteAsync(otherAccountKey);

            _ = transaction.StringSetAsync(ConstructAccountSessionCookieKey(cookie), accountName, SessionCookieLifetime);
            _ = transaction.StringSetAsync(accountKey, cookie, SessionCookieLifetime);

            if (await transaction.ExecuteAsync())
                return retiredCookies;
        }

        throw new InvalidOperationException($@"Unable To Retire The Earlier Logins Of Account ID ""{accountID}"" Within {SessionCookieSupersessionAttempts} Attempts");
    }

    public static async Task<string?> GetAccountNameForSessionCookie(this IDatabase distributedCacheStore, string cookie)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructAccountSessionCookieKey(cookie));

        return cachedValue.IsNullOrEmpty ? null : cachedValue.ToString();
    }

    public static async Task RemoveAccountNameForSessionCookie(this IDatabase distributedCacheStore, string cookie)
        => await distributedCacheStore.KeyDeleteAsync(ConstructAccountSessionCookieKey(cookie));

    public static async Task<(bool IsValid, string? AccountName)> ValidateAccountSessionCookie(this IDatabase distributedCacheStore, string cookie)
    {
        string? accountName = await distributedCacheStore.GetAccountNameForSessionCookie(cookie);

        return (accountName is not null, accountName);
    }

    /// <summary>
    ///     Purges all session cookies from the cache.
    ///     This should be called at application startup to clear stale session data.
    /// </summary>
    public static async Task PurgeSessionCookies(this IConnectionMultiplexer distributedCacheProvider)
    {
        EndPoint endpoint = distributedCacheProvider.GetEndPoints().Single();
        IServer server = distributedCacheProvider.GetServer(endpoint);
        IDatabase distributedCacheStore = distributedCacheProvider.GetDatabase();

        await foreach (RedisKey key in server.KeysAsync(pattern: "ACCOUNT-SESSION-COOKIE:*"))
        {
            await distributedCacheStore.KeyDeleteAsync(key);
        }
    }

    /// <summary>
    ///     The distributed cache pub/sub channel on which account logout notifications are published by the master server and consumed by the chat server.
    ///     Used to force-terminate any active TCP chat session for the logged-out account, so that peers no longer see them as connected even if the client's chat socket has not closed.
    /// </summary>
    public const string AccountLogoutChannel = "ACCOUNT-LOGOUT";

    /// <summary>
    ///     Publishes an account logout notification on <see cref="AccountLogoutChannel"/>.
    ///     The payload is the logout serialised as JSON, which names the session cookie so that the chat server ends only the session which used it.
    /// </summary>
    public static async Task PublishAccountLogout(this IDatabase distributedCacheStore, AccountLogout logout)
        => await distributedCacheStore.Multiplexer.GetSubscriber().PublishAsync(RedisChannel.Literal(AccountLogoutChannel), JsonSerializer.Serialize(logout));
}
