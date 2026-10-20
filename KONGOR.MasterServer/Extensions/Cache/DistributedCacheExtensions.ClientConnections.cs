namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    /// <summary>
    ///     Constructs the key of the session cookie of an account's newest login.
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

    /// <summary>
    ///     Records the session cookie of an account's newest login, replacing that of any earlier login, so that a client of an earlier login can be told apart.
    /// </summary>
    public static async Task SetLatestSessionCookie(this IDatabase distributedCacheStore, int accountID, string cookie)
        => await distributedCacheStore.StringSetAsync(ConstructAccountLatestSessionCookieKey(accountID), cookie, SessionCookieLifetime);

    public static async Task<string?> GetLatestSessionCookie(this IDatabase distributedCacheStore, int accountID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructAccountLatestSessionCookieKey(accountID));

        return cachedValue.IsNullOrEmpty ? null : cachedValue.ToString();
    }

    /// <summary>
    ///     Issues the game cookie of an account's client connection to a match server, identified by its server ID, or by <see langword="null"/> when its session is not a registered match server's, and returns it.
    ///     The stored game cookie is kept, and its expiry refreshed, when it was issued for the same match server, whatever that match server's session, or when the match server is not known, so that reconnecting to a match in progress keeps it.
    ///     Otherwise, a new game cookie replaces it, so that a match server which the account has left can tell that the account has joined another game.
    ///     When the match server is known, the account is also recorded as served by it.
    ///     The game cookie returned is always the stored one, and when none could be stored or read, even after one retry, an exception is thrown instead.
    /// </summary>
    public static async Task<string> IssueGameCookie(this IDatabase distributedCacheStore, int accountID, int? serverID)
    {
        string key = ConstructAccountGameCookieKey(accountID);

        // A Game Cookie Which Is Not Stored Is Never Served, Because The Duplicate Session Check Removes A Player Whose Game Cookie Differs From The Stored One
        // An Attempt Whose Game Cookie Vanished Before It Could Be Stored Or Read Is Retried Once, And Failing The Client Connection Is Preferred Over Serving A Game Cookie Which Was Never Stored
        string gameCookie = await distributedCacheStore.TryIssueGameCookie(key, serverID)
            ?? await distributedCacheStore.TryIssueGameCookie(key, serverID)
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
    private static async Task<string?> TryIssueGameCookie(this IDatabase distributedCacheStore, string key, int? serverID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(key);

        GameCookie? storedGameCookie = DeserialiseGameCookie(cachedValue);

        // The Stored Value Is Only Refreshed Or Replaced If It Is Still Exactly What Was Read, So That The Game Cookie Returned Is Never One Which Another Client Connection Has Replaced Or Which Has Expired In The Meantime
        ITransaction transaction = distributedCacheStore.CreateTransaction();

        transaction.AddCondition(cachedValue.IsNull ? Condition.KeyNotExists(key) : Condition.StringEqual(key, cachedValue));

        string gameCookie;

        if (storedGameCookie is not null && (serverID is null || storedGameCookie.ServerID == serverID))
        {
            _ = transaction.KeyExpireAsync(key, SessionCookieLifetime);

            gameCookie = storedGameCookie.Cookie;
        }

        else
        {
            GameCookie issuedGameCookie = new () { Cookie = Guid.CreateVersion7().ToString(), ServerID = serverID };

            _ = transaction.StringSetAsync(key, JsonSerializer.Serialize(issuedGameCookie), SessionCookieLifetime);

            gameCookie = issuedGameCookie.Cookie;
        }

        if (await transaction.ExecuteAsync())
            return gameCookie;

        // Another Client Connection Changed The Stored Game Cookie First, And Returning That One Means That Both Hold The Stored Game Cookie
        return DeserialiseGameCookie(await distributedCacheStore.StringGetAsync(key))?.Cookie;
    }

    public static async Task<string?> GetGameCookie(this IDatabase distributedCacheStore, int accountID)
        => DeserialiseGameCookie(await distributedCacheStore.StringGetAsync(ConstructAccountGameCookieKey(accountID)))?.Cookie;

    /// <summary>
    ///     Retrieves the IDs of the accounts whose client connections the match server has served.
    /// </summary>
    public static async Task<HashSet<int>> GetAccountsConnectedToMatchServer(this IDatabase distributedCacheStore, int serverID)
    {
        RedisValue[] members = await distributedCacheStore.SetMembersAsync(ConstructMatchServerAccountsKey(serverID));

        HashSet<int> accountIDs = [];

        foreach (RedisValue member in members)
        {
            if (int.TryParse(member.ToString(), out int accountID))
                accountIDs.Add(accountID);
        }

        return accountIDs;
    }

    /// <summary>
    ///     Deserialises a stored game cookie, which counts as absent when it cannot be deserialised, as after a change to the shape of the game cookie, so that it is replaced rather than failing the client connection.
    /// </summary>
    private static GameCookie? DeserialiseGameCookie(RedisValue cachedValue)
    {
        if (cachedValue.IsNullOrEmpty)
            return null;

        try
        {
            GameCookie? gameCookie = JsonSerializer.Deserialize<GameCookie>(cachedValue.ToString());

            return string.IsNullOrEmpty(gameCookie?.Cookie) ? null : gameCookie;
        }

        catch (JsonException)
        {
            return null;
        }
    }
}
