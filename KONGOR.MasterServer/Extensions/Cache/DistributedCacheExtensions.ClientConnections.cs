namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    /// <summary>
    ///     Constructs the key of the session cookie of an account's newest login, which the next login of the account retires, and for an account of a regular user the next login of any of the user's accounts retires as well.
    /// </summary>
    internal static string ConstructAccountLatestSessionCookieKey(int accountID) => $@"ACCOUNT-LATEST-SESSION-COOKIE:[""{accountID}""]";

    /// <summary>
    ///     Constructs the key of the game cookie of an account, of which there is at most one.
    /// </summary>
    internal static string ConstructAccountGameCookieKey(int accountID) => $@"ACCOUNT-GAME-COOKIE:[""{accountID}""]";

    /// <summary>
    ///     Constructs the key of the per-server index which lists the accounts whose client connections the match server has served.
    /// </summary>
    internal static string ConstructMatchServerAccountsKey(int serverID) => $@"MATCH-SERVER-ACCOUNTS:[""{serverID}""]";

    public static async Task<string?> GetLatestSessionCookie(this IDatabase distributedCacheStore, int accountID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructAccountLatestSessionCookieKey(accountID));

        return cachedValue.IsNullOrEmpty ? null : cachedValue.ToString();
    }

    /// <summary>
    ///     Deletes the latest-login record of an account when it points at the session cookie, and leaves it alone when it points at another, so that a logout of a retired login never clears the record of a newer one.
    ///     Clearing it keeps every latest-login record pointing at a live session cookie, so that a later login never counts a dead one as retired.
    /// </summary>
    public static async Task RemoveLatestSessionCookie(this IDatabase distributedCacheStore, int accountID, string cookie)
    {
        RedisKey key = ConstructAccountLatestSessionCookieKey(accountID);

        ITransaction transaction = distributedCacheStore.CreateTransaction();

        transaction.AddCondition(Condition.StringEqual(key, cookie));

        _ = transaction.KeyDeleteAsync(key);

        await transaction.ExecuteAsync();
    }

    /// <summary>
    ///     Issues the game cookie of an account's client connection to a match server, identified by its server ID, or by <see langword="null"/> when its session is not a registered match server's, and returns it.
    ///     The stored game cookie is kept, and its expiry refreshed, when it was issued for the same match server, whatever that match server's session, or when the match server is not known, so that reconnecting to a match in progress keeps it.
    ///     Otherwise, a new game cookie replaces it, so that a match server which the account has left can tell that the account has joined another game.
    ///     Either way, the stored game cookie carries the session cookie which the client connection posted, which is the one that the client on the match server now holds.
    ///     When the match server is known, the account is also recorded as served by it.
    ///     The game cookie returned is always the stored one, and when none could be stored or read, even after one retry, an exception is thrown instead.
    /// </summary>
    public static async Task<string> IssueGameCookie(this IDatabase distributedCacheStore, int accountID, int? serverID, string sessionCookie)
    {
        string key = ConstructAccountGameCookieKey(accountID);

        // A Game Cookie Which Is Not Stored Is Never Served, Because The Duplicate Session Check Removes A Player Whose Game Cookie Differs From The Stored One
        // An Attempt Whose Game Cookie Vanished Before It Could Be Stored Or Read Is Retried Once, And Failing The Client Connection Is Preferred Over Serving A Game Cookie Which Was Never Stored
        string gameCookie = await distributedCacheStore.TryIssueGameCookie(key, serverID, sessionCookie)
            ?? await distributedCacheStore.TryIssueGameCookie(key, serverID, sessionCookie)
            ?? throw new InvalidOperationException($@"Unable To Store Or Read A Game Cookie For Account ID ""{accountID}""");

        if (serverID is not null)
        {
            string serverKey = ConstructMatchServerAccountsKey(serverID.Value);

            ITransaction serverTransaction = distributedCacheStore.CreateTransaction();

            _ = serverTransaction.SetAddAsync(serverKey, accountID);
            _ = serverTransaction.KeyExpireAsync(serverKey, SessionCookieLifetime);

            await serverTransaction.ExecuteAsync();
        }

        return gameCookie;
    }

    /// <summary>
    ///     Makes one attempt to issue the game cookie stored at the key, and returns the game cookie which is stored when the attempt ends.
    ///     Returns <see langword="null"/> when the attempt lost a race and no readable game cookie is stored any longer.
    /// </summary>
    private static async Task<string?> TryIssueGameCookie(this IDatabase distributedCacheStore, string key, int? serverID, string sessionCookie)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(key);

        GameCookie? storedGameCookie = DeserialiseGameCookie(cachedValue);

        // The Stored Value Is Only Refreshed Or Replaced If It Is Still Exactly What Was Read, So That The Game Cookie Returned Is Never One Which Another Client Connection Has Replaced Or Which Has Expired In The Meantime
        ITransaction transaction = distributedCacheStore.CreateTransaction();

        transaction.AddCondition(cachedValue.IsNull ? Condition.KeyNotExists(key) : Condition.StringEqual(key, cachedValue));

        string gameCookie;

        if (storedGameCookie is not null && (serverID is null || storedGameCookie.ServerID == serverID))
        {
            if (string.Equals(storedGameCookie.SessionCookie, sessionCookie, StringComparison.Ordinal))
            {
                _ = transaction.KeyExpireAsync(key, SessionCookieLifetime);
            }

            else
            {
                GameCookie reissuedGameCookie = new () { Cookie = storedGameCookie.Cookie, ServerID = storedGameCookie.ServerID, SessionCookie = sessionCookie };

                _ = transaction.StringSetAsync(key, JsonSerializer.Serialize(reissuedGameCookie), SessionCookieLifetime);
            }

            gameCookie = storedGameCookie.Cookie;
        }

        else
        {
            GameCookie issuedGameCookie = new () { Cookie = Guid.CreateVersion7().ToString(), ServerID = serverID, SessionCookie = sessionCookie };

            _ = transaction.StringSetAsync(key, JsonSerializer.Serialize(issuedGameCookie), SessionCookieLifetime);

            gameCookie = issuedGameCookie.Cookie;
        }

        if (await transaction.ExecuteAsync())
            return gameCookie;

        // Another Client Connection Changed The Stored Game Cookie First, And Returning That One Means That Both Hold The Stored Game Cookie
        return DeserialiseGameCookie(await distributedCacheStore.StringGetAsync(key))?.Cookie;
    }

    public static async Task<string?> GetGameCookie(this IDatabase distributedCacheStore, int accountID)
        => (await distributedCacheStore.GetStoredGameCookie(accountID))?.Cookie;

    /// <summary>
    ///     Retrieves the stored game cookie of an account, with the match server and the session cookie which it is stored with, or <see langword="null"/> when there is none which can be read.
    /// </summary>
    public static async Task<GameCookie?> GetStoredGameCookie(this IDatabase distributedCacheStore, int accountID)
        => DeserialiseGameCookie(await distributedCacheStore.StringGetAsync(ConstructAccountGameCookieKey(accountID)));

    /// <summary>
    ///     Records the session cookie as the one which the client on the match server holds for the account, because a client which logs in again during its match pushes its new session cookie to its match server ("CHostClient::UpdateCookie").
    ///     Only a stored game cookie which was issued for that match server is changed, and only its session cookie, so that a refresh never creates a record, never changes a game cookie, and never extends its expiry.
    ///     Returns <see langword="true"/> when the account's stored game cookie is for that match server, which then carries the session cookie, unless a client connection changed the record first, and that newer record is kept.
    ///     Returns <see langword="false"/>, changing nothing, otherwise.
    /// </summary>
    public static async Task<bool> RefreshClientSessionCookie(this IDatabase distributedCacheStore, int accountID, int serverID, string sessionCookie)
    {
        string key = ConstructAccountGameCookieKey(accountID);

        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(key);

        GameCookie? storedGameCookie = DeserialiseGameCookie(cachedValue);

        if (storedGameCookie is null || storedGameCookie.ServerID != serverID)
            return false;

        if (string.Equals(storedGameCookie.SessionCookie, sessionCookie, StringComparison.Ordinal))
            return true;

        GameCookie refreshedGameCookie = new () { Cookie = storedGameCookie.Cookie, ServerID = storedGameCookie.ServerID, SessionCookie = sessionCookie };

        // The Record Is Only Refreshed If It Is Still Exactly What Was Read, Because Only A Newer Client Connection Or Refresh Changes It In The Meantime, And That Newer Record Is Kept
        ITransaction transaction = distributedCacheStore.CreateTransaction();

        transaction.AddCondition(Condition.StringEqual(key, cachedValue));

        _ = transaction.StringSetAsync(key, JsonSerializer.Serialize(refreshedGameCookie), Expiration.KeepTtl);

        await transaction.ExecuteAsync();

        return true;
    }

    /// <summary>
    ///     Retrieves which of the given accounts the match server has served, in the order in which they are given.
    ///     Only the given accounts are checked, in one request, so that the cost does not grow with every account which the match server has ever served.
    /// </summary>
    public static async Task<List<int>> GetAccountsConnectedToMatchServer(this IDatabase distributedCacheStore, int serverID, IReadOnlyList<int> accountIDs)
    {
        // The Distributed Cache Rejects A Membership Check Of No Members
        if (accountIDs.Count is 0)
            return [];

        bool[] connected = await distributedCacheStore.SetContainsAsync(ConstructMatchServerAccountsKey(serverID), [.. accountIDs.Select(accountID => (RedisValue) accountID)]);

        return [.. accountIDs.Where((accountID, index) => connected[index])];
    }

    /// <summary>
    ///     Deserialises a stored game cookie, which counts as absent when it cannot be deserialised, as after a change to the shape of the game cookie, so that it is replaced rather than failing the client connection.
    ///     It also counts as absent without a session cookie, because the duplicate session check would answer it empty, and the match server removes a player whose session cookie is answered empty.
    /// </summary>
    private static GameCookie? DeserialiseGameCookie(RedisValue cachedValue)
    {
        if (cachedValue.IsNullOrEmpty)
            return null;

        try
        {
            GameCookie? gameCookie = JsonSerializer.Deserialize<GameCookie>(cachedValue.ToString());

            return string.IsNullOrEmpty(gameCookie?.Cookie) || string.IsNullOrEmpty(gameCookie.SessionCookie) ? null : gameCookie;
        }

        catch (JsonException)
        {
            return null;
        }
    }
}
