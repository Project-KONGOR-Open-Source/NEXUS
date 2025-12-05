namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructFriendRequestKey(int requesterID, int targetID) => $@"FRIEND-REQUEST:[""{requesterID}:{targetID}""]";

    /// <summary>
    ///     Stores a pending friend request in the distributed cache store.
    ///     The value of this entry is the requester's and target's notification IDs as a tuple.
    /// </summary>
    public static async Task SetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID, int requesterNotificationID, int targetNotificationID)
    {
        string notificationIDsPair = $"{requesterNotificationID}:{targetNotificationID}";

        await distributedCacheStore.StringSetAsync(ConstructFriendRequestKey(requesterID, targetID), notificationIDsPair, TimeSpan.FromHours(24));
    }

    /// <summary>
    ///     Retrieves a pending friend request from the distributed cache store.
    ///     Returns a tuple containing the requester's and target's notification IDs, or NULL if the request doesn't exist or has expired.
    /// </summary>
    public static async Task<(int RequesterNotificationID, int TargetNotificationID)?> GetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructFriendRequestKey(requesterID, targetID));

        if (cachedValue.IsNullOrEmpty)
        {
            return null;
        }

        string[] parts = cachedValue.ToString().Split(':');

        if (parts.Length is not 2 || int.TryParse(parts.First(), out int requesterNotificationID) is false || int.TryParse(parts.Last(), out int targetNotificationID) is false)
        {
            return null;
        }

        return (requesterNotificationID, targetNotificationID);
    }

    /// <summary>
    ///     Removes a friend request from the distributed cache store, usually because it has been accepted or declined.
    /// </summary>
    public static async Task RemoveFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        await distributedCacheStore.KeyDeleteAsync(ConstructFriendRequestKey(requesterID, targetID));
    }

    /// <summary>
    ///     Checks whether a pending friend request already exists.
    /// </summary>
    public static async Task<bool> PendingFriendRequestExists(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        bool exists = await distributedCacheStore.KeyExistsAsync(ConstructFriendRequestKey(requesterID, targetID));

        return exists;
    }
}
