namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    /// <summary>
    ///     Constructs the key of the match lock of a user, of which there is at most one.
    /// </summary>
    internal static string ConstructMatchLockKey(int userID) => $@"MATCH-LOCK:[""{userID}""]";

    /// <summary>
    ///     Constructs the key of the per-server index which lists the users whose match lock is on the server.
    ///     The index allows the locks of a server to be found without scanning the entire key space.
    /// </summary>
    internal static string ConstructMatchLockServerKey(int serverID) => $@"MATCH-LOCK-SERVER:[""{serverID}""]";

    /// <summary>
    ///     Constructs the key of the marker which records that the match lock's login trigger has run for the session cookie of a login.
    /// </summary>
    internal static string ConstructMatchLockLoginCheckedKey(string sessionCookie) => $@"MATCH-LOCK-LOGIN-CHECKED:[""{sessionCookie}""]";

    /// <summary>
    ///     Stores the match lock of a user, replacing any previous one, and adds it to the index of its server.
    ///     Both expire when the lock reaches its safety limit, and a lock which is already past it is not stored.
    /// </summary>
    public static async Task SetMatchLock(this IDatabase distributedCacheStore, MatchLock matchLock)
    {
        TimeSpan timeToLive = matchLock.TimestampExpires - DateTimeOffset.UtcNow;

        if (timeToLive <= TimeSpan.Zero)
            return;

        string serializedMatchLock = JsonSerializer.Serialize(matchLock);

        string serverKey = ConstructMatchLockServerKey(matchLock.ServerID);

        ITransaction transaction = distributedCacheStore.CreateTransaction();

        _ = transaction.StringSetAsync(ConstructMatchLockKey(matchLock.UserID), serializedMatchLock, timeToLive);
        _ = transaction.SetAddAsync(serverKey, matchLock.UserID);

        // The Index Only Ever Extends To The Latest Expiry, So That A Lock Which Started Earlier Never Shortens It Under The Locks Of Other Users
        _ = transaction.KeyExpireAsync(serverKey, timeToLive, ExpireWhen.HasNoExpiry);
        _ = transaction.KeyExpireAsync(serverKey, timeToLive, ExpireWhen.GreaterThanCurrentExpiry);

        await transaction.ExecuteAsync();
    }

    public static async Task<MatchLock?> GetMatchLock(this IDatabase distributedCacheStore, int userID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructMatchLockKey(userID));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<MatchLock>(cachedValue.ToString());
    }

    /// <summary>
    ///     Marks that the match lock's login trigger has run for the session cookie of a login, so that the chat reconnects of the same login do not run it again.
    ///     Returns <see langword="true"/> only the first time for a session cookie, and the marker expires with the session cookie, or after <see cref="SessionCookieLifetime"/> when the session cookie's remaining lifetime is unknown.
    /// </summary>
    public static async Task<bool> MarkMatchLockLoginChecked(this IDatabase distributedCacheStore, string sessionCookie)
    {
        TimeSpan? sessionCookieTimeToLive = await distributedCacheStore.KeyTimeToLiveAsync(ConstructAccountSessionCookieKey(sessionCookie));

        TimeSpan timeToLive = sessionCookieTimeToLive is { } remainingLifetime && remainingLifetime > TimeSpan.Zero ? remainingLifetime : SessionCookieLifetime;

        return await distributedCacheStore.StringSetAsync(ConstructMatchLockLoginCheckedKey(sessionCookie), true, timeToLive, When.NotExists);
    }

    /// <summary>
    ///     Removes the match lock of a user only if it is for the given match, so that a lock for a newer match is never lifted by an older one ending.
    ///     Returns whether a lock was removed.
    /// </summary>
    public static async Task<bool> RemoveMatchLock(this IDatabase distributedCacheStore, int userID, int matchID)
    {
        string key = ConstructMatchLockKey(userID);

        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(key);

        if (cachedValue.IsNullOrEmpty)
            return false;

        MatchLock? matchLock = JsonSerializer.Deserialize<MatchLock>(cachedValue.ToString());

        if (matchLock is null || matchLock.MatchID != matchID)
            return false;

        // The Lock Is Only Removed If It Is Still Exactly What Was Read, So That A Lock Written In The Meantime Survives
        ITransaction transaction = distributedCacheStore.CreateTransaction();

        transaction.AddCondition(Condition.StringEqual(key, cachedValue));

        _ = transaction.KeyDeleteAsync(key);
        _ = transaction.SetRemoveAsync(ConstructMatchLockServerKey(matchLock.ServerID), userID);

        return await transaction.ExecuteAsync();
    }

    /// <summary>
    ///     Removes the match locks on a server which satisfy the predicate, or all of them when there is no predicate.
    ///     Returns the number of locks removed.
    /// </summary>
    public static async Task<int> RemoveMatchLocksForServer(this IDatabase distributedCacheStore, int serverID, Func<MatchLock, bool>? predicate = null)
    {
        string serverKey = ConstructMatchLockServerKey(serverID);

        RedisValue[] members = await distributedCacheStore.SetMembersAsync(serverKey);

        int removed = 0;

        foreach (RedisValue member in members)
        {
            if (!int.TryParse(member.ToString(), out int userID))
                continue;

            string lockKey = ConstructMatchLockKey(userID);

            RedisValue cachedValue = await distributedCacheStore.StringGetAsync(lockKey);

            MatchLock? matchLock = cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<MatchLock>(cachedValue.ToString());

            // A Member Whose Lock Is Gone Or Now On Another Server Is Stale, So It Is Dropped From The Index
            // The Drop Is Conditioned On What Was Read, So That A Lock Written In The Meantime Keeps Its Place In The Index
            if (matchLock is null || matchLock.ServerID != serverID)
            {
                ITransaction staleTransaction = distributedCacheStore.CreateTransaction();

                staleTransaction.AddCondition(cachedValue.IsNullOrEmpty ? Condition.KeyNotExists(lockKey) : Condition.StringEqual(lockKey, cachedValue));

                _ = staleTransaction.SetRemoveAsync(serverKey, member);

                await staleTransaction.ExecuteAsync();

                continue;
            }

            if (predicate is not null && !predicate(matchLock))
                continue;

            if (await distributedCacheStore.RemoveMatchLock(userID, matchLock.MatchID))
                removed++;
        }

        return removed;
    }
}
