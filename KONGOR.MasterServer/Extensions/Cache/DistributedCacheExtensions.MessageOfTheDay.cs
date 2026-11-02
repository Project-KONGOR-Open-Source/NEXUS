namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    /// <summary>
    ///     Constructs the key of a message of the day event.
    /// </summary>
    internal static string ConstructMessageOfTheDayEventKey(Guid id) => $@"MESSAGE-OF-THE-DAY-EVENT:[""{id}""]";

    /// <summary>
    ///     The key of the index which lists the message of the day's events.
    /// </summary>
    internal const string MessageOfTheDayEventsIndexKey = "MESSAGE-OF-THE-DAY-EVENTS";

    /// <summary>
    ///     Stores a message of the day event, replacing any event with the same identity; the event expires on its own at its expiry.
    ///     An event without a future expiry is refused with an <see cref="ArgumentException"/>.
    /// </summary>
    public static async Task SetMessageOfTheDayEvent(this IDatabase distributedCacheStore, MessageOfTheDayItem item)
    {
        TimeSpan timeToLive = ExpiringItemTimeToLive(item.Expires, nameof(item));

        await SetExpiringItem(distributedCacheStore, ConstructMessageOfTheDayEventKey(item.ID), MessageOfTheDayEventsIndexKey, item.ID, JsonSerializer.Serialize(item), timeToLive);
    }

    public static async Task<MessageOfTheDayItem?> GetMessageOfTheDayEvent(this IDatabase distributedCacheStore, Guid id)
        => await GetExpiringItem<MessageOfTheDayItem>(distributedCacheStore, ConstructMessageOfTheDayEventKey(id));

    /// <summary>
    ///     Lists every message of the day event which has not expired, scheduled ones included, in no particular order.
    /// </summary>
    public static async Task<List<MessageOfTheDayItem>> GetMessageOfTheDayEvents(this IDatabase distributedCacheStore)
        => await GetExpiringItems<MessageOfTheDayItem>(distributedCacheStore, MessageOfTheDayEventsIndexKey, ConstructMessageOfTheDayEventKey);

    /// <summary>
    ///     Removes a message of the day event, and returns whether it existed.
    /// </summary>
    public static async Task<bool> RemoveMessageOfTheDayEvent(this IDatabase distributedCacheStore, Guid id)
        => await RemoveExpiringItem(distributedCacheStore, ConstructMessageOfTheDayEventKey(id), MessageOfTheDayEventsIndexKey, id);
}
