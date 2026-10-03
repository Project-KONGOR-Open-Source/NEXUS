namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructMasteryBoostContextKey(int accountID, int matchID) => $@"MASTERY-BOOST-CONTEXT:[""{accountID}:{matchID}""]";

    /// <summary>
    ///     Records the mastery boost applied to the given match, so that subsequent match statistics reads can report the applied boost and so that a second boost cannot be applied to the same match.
    ///     The entry expires after the boost application window. Because it is written at application time, which is always within the window, it can never expire while its match is still eligible for boosting.
    /// </summary>
    public static async Task SetMasteryBoostContext(this IDatabase distributedCacheStore, int accountID, int matchID, MasteryBoostContext masteryBoostContext)
        => await distributedCacheStore.StringSetAsync(ConstructMasteryBoostContextKey(accountID, matchID), JsonSerializer.Serialize(masteryBoostContext), MasteryBoost.ApplicationWindow);

    /// <summary>
    ///     Gets the mastery boost applied to the given match, or <see langword="null"/> if no boost has been applied to it.
    /// </summary>
    public static async Task<MasteryBoostContext?> GetMasteryBoostContext(this IDatabase distributedCacheStore, int accountID, int matchID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructMasteryBoostContextKey(accountID, matchID));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<MasteryBoostContext>(cachedValue.ToString());
    }
}

/// <summary>
///     The mastery boost applied to a match, capturing the awarded experience and whether it was awarded by a super boost.
///     The boost type is recorded because the client reports the experience of the two boost types in separate match statistics response fields.
/// </summary>
public record MasteryBoostContext(int Experience, bool IsSuperBoost);
