namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    /// <summary>
    ///     Stores an item under its own key, which expires on its own, and adds its identity to an index, so that the items can be listed without scanning the key space.
    ///     The index's expiry only ever grows to the latest item's, so that an item stored earlier never shortens it.
    /// </summary>
    private static async Task SetExpiringItem(IDatabase distributedCacheStore, string itemKey, string indexKey, Guid id, string serializedItem, TimeSpan timeToLive)
    {
        ITransaction transaction = distributedCacheStore.CreateTransaction();

        _ = transaction.StringSetAsync(itemKey, serializedItem, timeToLive);
        _ = transaction.SetAddAsync(indexKey, id.ToString());
        _ = transaction.KeyExpireAsync(indexKey, timeToLive, ExpireWhen.HasNoExpiry);
        _ = transaction.KeyExpireAsync(indexKey, timeToLive, ExpireWhen.GreaterThanCurrentExpiry);

        await transaction.ExecuteAsync();
    }

    private static async Task<T?> GetExpiringItem<T>(IDatabase distributedCacheStore, string itemKey) where T : class
        => DeserialiseExpiringItem<T>(await distributedCacheStore.StringGetAsync(itemKey));

    /// <summary>
    ///     Lists the items of an index whose keys have not expired, in no particular order.
    ///     A member whose item has expired or cannot be read is dropped from the index, conditioned on the item still being missing, so that an item stored in the meantime keeps its place.
    /// </summary>
    private static async Task<List<T>> GetExpiringItems<T>(IDatabase distributedCacheStore, string indexKey, Func<Guid, string> constructItemKey) where T : class
    {
        List<T> items = [];

        foreach (RedisValue member in await distributedCacheStore.SetMembersAsync(indexKey))
        {
            if (Guid.TryParse(member.ToString(), out Guid id) is false)
            {
                await distributedCacheStore.SetRemoveAsync(indexKey, member);

                continue;
            }

            string itemKey = constructItemKey(id);

            RedisValue cachedValue = await distributedCacheStore.StringGetAsync(itemKey);

            T? item = DeserialiseExpiringItem<T>(cachedValue);

            if (item is null)
            {
                ITransaction staleTransaction = distributedCacheStore.CreateTransaction();

                staleTransaction.AddCondition(cachedValue.IsNull ? Condition.KeyNotExists(itemKey) : Condition.StringEqual(itemKey, cachedValue));

                _ = staleTransaction.SetRemoveAsync(indexKey, member);

                await staleTransaction.ExecuteAsync();

                continue;
            }

            items.Add(item);
        }

        return items;
    }

    /// <summary>
    ///     Removes an item and its place in the index, and returns whether the item existed.
    /// </summary>
    private static async Task<bool> RemoveExpiringItem(IDatabase distributedCacheStore, string itemKey, string indexKey, Guid id)
    {
        ITransaction transaction = distributedCacheStore.CreateTransaction();

        Task<bool> deleted = transaction.KeyDeleteAsync(itemKey);
        _ = transaction.SetRemoveAsync(indexKey, id.ToString());

        await transaction.ExecuteAsync();

        return await deleted;
    }

    /// <summary>
    ///     The time an item has left to live, refusing an item without a future expiry, which could never be stored.
    /// </summary>
    private static TimeSpan ExpiringItemTimeToLive(DateTimeOffset? expires, string parameterName)
    {
        TimeSpan? timeToLive = expires - DateTimeOffset.UtcNow;

        if (timeToLive is not { } remainingLifetime || remainingLifetime <= TimeSpan.Zero)
            throw new ArgumentException("The Item Needs An Expiry In The Future", parameterName);

        return remainingLifetime;
    }

    private static T? DeserialiseExpiringItem<T>(RedisValue cachedValue) where T : class
    {
        if (cachedValue.IsNullOrEmpty)
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(cachedValue.ToString());
        }

        catch (JsonException)
        {
            return null;
        }
    }
}
