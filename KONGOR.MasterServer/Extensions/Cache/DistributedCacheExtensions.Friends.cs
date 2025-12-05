namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructFriendRequestKey(int requesterID, int targetID) => $@"FRIEND-REQUEST:[""{requesterID}:{targetID}""]";

    /// <summary>
    ///     Stores a pending friend request in the distributed cache store.
    /// </summary>
    public static async Task SetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID, int notificationID)
    {
        await distributedCacheStore.StringSetAsync(ConstructFriendRequestKey(requesterID, targetID), notificationID, TimeSpan.FromSeconds(60));
    }

    /// <summary>
    ///     Retrieves a pending friend request from the distributed cache store.
    ///     Returns NULL if request doesn't exist or has expired.
    /// </summary>
    public static async Task<int?> GetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructFriendRequestKey(requesterID, targetID));

        return cachedValue.IsNullOrEmpty ? null : int.TryParse(cachedValue.ToString(), out int notificationID) ? notificationID : null;
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
