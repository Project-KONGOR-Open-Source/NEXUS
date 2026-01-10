namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructSRPAuthenticationSessionDataKey(string accountName)
    {
        return $@"SRP-SESSION-DATA:[""{accountName}""]";
    }

    public static async Task SetSRPAuthenticationSessionData(this IDatabase distributedCacheStore, string accountName,
        SRPAuthenticationSessionDataStageOne data)
    {
        string serializedData = JsonSerializer.Serialize(data);

        await distributedCacheStore.StringSetAsync(ConstructSRPAuthenticationSessionDataKey(accountName),
            serializedData, TimeSpan.FromSeconds(30));
    }

    public static async Task<SRPAuthenticationSessionDataStageOne?> GetSRPAuthenticationSessionData(
        this IDatabase distributedCacheStore, string accountName)
    {
        RedisValue cachedValue =
            await distributedCacheStore.StringGetAsync(ConstructSRPAuthenticationSessionDataKey(accountName));

        return cachedValue.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<SRPAuthenticationSessionDataStageOne>(cachedValue.ToString());
    }

    public static async Task RemoveSRPAuthenticationSessionData(this IDatabase distributedCacheStore,
        string accountName)
    {
        await distributedCacheStore.KeyDeleteAsync(ConstructSRPAuthenticationSessionDataKey(accountName));
    }

    private static string ConstructSRPAuthenticationSystemInformationKey(string accountName)
    {
        return $@"SYSTEM-INFORMATION:[""{accountName}""]";
    }

    public static async Task SetSRPAuthenticationSystemInformation(this IDatabase distributedCacheStore,
        string accountName, string systemInformation)
    {
        await distributedCacheStore.StringSetAsync(ConstructSRPAuthenticationSystemInformationKey(accountName),
            systemInformation, TimeSpan.FromSeconds(30));
    }

    public static async Task<string?> GetSRPAuthenticationSystemInformation(this IDatabase distributedCacheStore,
        string accountName)
    {
        RedisValue cachedValue =
            await distributedCacheStore.StringGetAsync(ConstructSRPAuthenticationSystemInformationKey(accountName));

        return cachedValue.IsNullOrEmpty ? null : cachedValue.ToString();
    }

    public static async Task RemoveSRPAuthenticationSystemInformation(this IDatabase distributedCacheStore,
        string accountName)
    {
        await distributedCacheStore.KeyDeleteAsync(ConstructSRPAuthenticationSystemInformationKey(accountName));
    }

    // The Cookie Is Stored In The Key Because Most Requests From HoN Are Made With Just A Cookie But No Account Information
    // Additionally, Cached Values Cannot Be Retrieved Without Their Respective Key, So Iterating Just The Values Is Not Possible Without Complex Reflection
    private static string ConstructAccountSessionCookieKey(string cookie)
    {
        return $@"ACCOUNT-SESSION-COOKIE:[""{cookie}""]";
    }

    public static async Task SetAccountNameForSessionCookie(this IDatabase distributedCacheStore, string cookie,
        string accountName)
    {
        await distributedCacheStore.StringSetAsync(ConstructAccountSessionCookieKey(cookie), accountName,
            TimeSpan.FromHours(24));
    }

    public static async Task<string?> GetAccountNameForSessionCookie(this IDatabase distributedCacheStore,
        string cookie)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructAccountSessionCookieKey(cookie));

        return cachedValue.IsNullOrEmpty ? null : cachedValue.ToString();
    }

    public static async Task RemoveAccountNameForSessionCookie(this IDatabase distributedCacheStore, string cookie)
    {
        await distributedCacheStore.KeyDeleteAsync(ConstructAccountSessionCookieKey(cookie));
    }

    public static async Task<(bool IsValid, string? AccountName)> ValidateAccountSessionCookie(
        this IDatabase distributedCacheStore, string cookie)
    {
        // Check 1: Raw Cookie (As provided)
        string? accountName = await distributedCacheStore.GetAccountNameForSessionCookie(cookie);

        if (accountName is not null)
        {
            return (true, accountName);
        }

        // Check 2: Fuzzy Logic (If dash-less, try dashed GUID format)
        if (cookie.Length == 32 && Guid.TryParse(cookie, out Guid guid))
        {
            string dashedCookie = guid.ToString(); // Default "D" format has dashes
            accountName = await distributedCacheStore.GetAccountNameForSessionCookie(dashedCookie);
            if (accountName is not null)
            {
                return (true, accountName);
            }
        }

        // Check 3: Reverse Fuzzy (If dashed, try dash-less - rarely needed but safe)
        if (cookie.Contains("-") && Guid.TryParse(cookie, out Guid guid2))
        {
            string noDashCookie = guid2.ToString("N");
            accountName = await distributedCacheStore.GetAccountNameForSessionCookie(noDashCookie);
            if (accountName is not null)
            {
                return (true, accountName);
            }
        }

        return (false, null);
    }

    public static async Task<(bool IsValid, string? AccountName)> ValidateAccountSessionCookie(
        this IDatabase distributedCacheStore, string cookie, MerrickContext dbContext, ILogger? logger = null)
    {
        // 1. Check Redis (Fast Path)
        (bool isValid, string? accountName) = await distributedCacheStore.ValidateAccountSessionCookie(cookie);
        if (isValid)
        {
            return (true, accountName);
        }

        // 2. Check Database (Fallback Path)
        // Fuzzy Logic:
        // - If length 32 (no dashes), try match with dashed version.
        // - If length 36 (dashes), try match with dash-less version.

        string? altCookie = null;
        if (cookie.Length == 32 && Guid.TryParse(cookie, out Guid guid))
        {
            altCookie = guid.ToString(); // Dashed
        }
        else if (cookie.Contains("-") && Guid.TryParse(cookie, out Guid guid2))
        {
            altCookie = guid2.ToString("N"); // No Dashes
        }

        Account? account = await dbContext.Accounts.FirstOrDefaultAsync(a =>
            a.Cookie == cookie || (altCookie != null && a.Cookie == altCookie));

        if (account != null)
        {
            // Session is valid in DB. Restore to Redis.
            await distributedCacheStore.SetAccountNameForSessionCookie(cookie, account.Name);
            if (logger != null)
            {
                logger.LogInformation(
                    $"[SessionFallback] DB Hit! Restored Session From Database For Cookie '{cookie}' (User: {account.Name})");
            }

            return (true, account.Name);
        }

        return (false, null);
    }

    /// <summary>
    ///     Purges all session cookies from the cache.
    ///     This should be called at application startup to clear stale session data.
    /// </summary>
    public static async Task PurgeSessionCookies(this IConnectionMultiplexer distributedCacheProvider)
    {
        EndPoint endpoint = distributedCacheProvider.GetEndPoints().First();
        IServer server = distributedCacheProvider.GetServer(endpoint);
        IDatabase distributedCacheStore = distributedCacheProvider.GetDatabase();

        await foreach (RedisKey key in server.KeysAsync(pattern: "ACCOUNT-SESSION-COOKIE:*"))
        {
            await distributedCacheStore.KeyDeleteAsync(key);
        }
    }
}