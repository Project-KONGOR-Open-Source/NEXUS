namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructMasteryBoostContextKey(string cookie) => $@"MASTERY-BOOST-CONTEXT:[""{cookie}""]";

    /// <summary>
    ///     Caches the post-match mastery boost context (the hero and the experience a regular boost would award) when the match stats screen is shown, so a subsequent boost purchase can be applied without trusting client-supplied data.
    /// </summary>
    public static async Task SetMasteryBoostContext(this IDatabase distributedCacheStore, string cookie, MasteryBoostContext context)
    {
        string serializedContext = JsonSerializer.Serialize(context);

        await distributedCacheStore.StringSetAsync(ConstructMasteryBoostContextKey(cookie), serializedContext, TimeSpan.FromHours(1));
    }

    public static async Task<MasteryBoostContext?> GetMasteryBoostContext(this IDatabase distributedCacheStore, string cookie)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructMasteryBoostContextKey(cookie));

        return cachedValue.IsNullOrEmpty ? null : JsonSerializer.Deserialize<MasteryBoostContext>(cachedValue.ToString());
    }

    public static async Task RemoveMasteryBoostContext(this IDatabase distributedCacheStore, string cookie)
        => await distributedCacheStore.KeyDeleteAsync(ConstructMasteryBoostContextKey(cookie));
}

/// <summary>
///     The post-match mastery boost context: the hero played and the experience a regular mastery boost would award for that match.
/// </summary>
public sealed record MasteryBoostContext(string HeroIdentifier, int Experience);
