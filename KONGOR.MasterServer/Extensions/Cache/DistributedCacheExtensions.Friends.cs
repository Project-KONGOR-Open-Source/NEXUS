namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructFriendRequestKey(int requesterID, int targetID) => $@"FRIEND-REQUEST:[""{requesterID}:{targetID}""]";

    /// <summary>
    ///     Constructs the key of the per-account inbox which lists every pending friend request that targets the account.
    ///     The inbox allows an account's incoming requests to be enumerated without scanning the entire key space.
    /// </summary>
    private static string ConstructFriendRequestInboxKey(int targetID) => $@"FRIEND-REQUEST-INBOX:[""{targetID}""]";

    /// <summary>
    ///     Constructs the inbox entry which identifies a single friend request by its requester and target.
    /// </summary>
    private static string ConstructFriendRequestInboxEntry(int requesterID, int targetID) => $"{requesterID}:{targetID}";

    /// <summary>
    ///     Stores a pending friend request in the distributed cache store.
    ///     The value of this entry is the target's notification ID, along with the creation timestamp.
    ///     Only the target's notification is persisted; the requester's own notification is session-scoped and is never re-delivered.
    ///     The friend request is also added to the target's inbox so that it can be re-delivered when the target logs in.
    /// </summary>
    public static async Task SetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID, int targetNotificationID)
    {
        string value = $"{targetNotificationID}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        await distributedCacheStore.StringSetAsync(ConstructFriendRequestKey(requesterID, targetID), value, TimeSpan.FromDays(7));

        await distributedCacheStore.SetAddAsync(ConstructFriendRequestInboxKey(targetID), ConstructFriendRequestInboxEntry(requesterID, targetID));
    }

    /// <summary>
    ///     Retrieves the target's notification ID for a pending friend request from the distributed cache store.
    ///     Returns <see langword="null"/> if the friend request doesn't exist or has expired.
    /// </summary>
    public static async Task<int?> GetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructFriendRequestKey(requesterID, targetID));

        (int TargetNotificationID, DateTimeOffset CreatedAt)? parsedValue = ParseFriendRequestValue(cachedValue);

        if (parsedValue is null)
        {
            return null;
        }

        return parsedValue.Value.TargetNotificationID;
    }

    /// <summary>
    ///     Removes a friend request from the distributed cache store, usually because it has been accepted or declined.
    ///     The friend request is removed from the target's inbox as well.
    /// </summary>
    public static async Task RemoveFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        await distributedCacheStore.KeyDeleteAsync(ConstructFriendRequestKey(requesterID, targetID));

        // The Distributed Cache Deletes The Inbox Set Automatically Once Its Last Entry Is Removed, So There Is No Empty Set Left To Clean Up
        await distributedCacheStore.SetRemoveAsync(ConstructFriendRequestInboxKey(targetID), ConstructFriendRequestInboxEntry(requesterID, targetID));
    }

    /// <summary>
    ///     Checks whether a pending friend request already exists.
    /// </summary>
    public static async Task<bool> PendingFriendRequestExists(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        bool exists = await distributedCacheStore.KeyExistsAsync(ConstructFriendRequestKey(requesterID, targetID));

        return exists;
    }

    /// <summary>
    ///     Retrieves the pending friend requests which target the account, so that they can be re-delivered as notifications when the account logs in.
    ///     Each entry contains the requester's account ID, the target's notification ID, and the time at which the friend request was made.
    ///     Inbox entries which point at expired or malformed friend requests are removed as they are encountered, so that the inbox heals itself over time.
    /// </summary>
    public static async Task<List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)>> GetPendingFriendRequestsForAccount(this IDatabase distributedCacheStore, int accountID)
    {
        string inboxKey = ConstructFriendRequestInboxKey(accountID);

        RedisValue[] inboxEntries = await distributedCacheStore.SetMembersAsync(inboxKey);

        List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)> pendingRequests = [];

        foreach (RedisValue inboxEntry in inboxEntries)
        {
            string[] accountIDs = inboxEntry.ToString().Split(':');

            if (accountIDs.Length is not 2 || int.TryParse(accountIDs[0], out int requesterID) is false || int.TryParse(accountIDs[1], out int targetID) is false)
            {
                await distributedCacheStore.SetRemoveAsync(inboxKey, inboxEntry);

                continue;
            }

            RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructFriendRequestKey(requesterID, targetID));

            (int TargetNotificationID, DateTimeOffset CreatedAt)? parsedValue = ParseFriendRequestValue(cachedValue);

            if (parsedValue is null)
            {
                // The Friend Request Has Expired Or Is Missing, So Heal The Inbox By Removing The Stale Entry
                // If This Is The Last Entry In The Inbox, Following Its Deletion, The Distributed Cache Will Automatically Delete The Inbox Set, So There Is No Empty Set Left To Clean Up
                await distributedCacheStore.SetRemoveAsync(inboxKey, inboxEntry);

                continue;
            }

            pendingRequests.Add((requesterID, parsedValue.Value.TargetNotificationID, parsedValue.Value.CreatedAt));
        }

        return pendingRequests;
    }

    /// <summary>
    ///     Removes the pending friend request whose target notification ID matches the supplied notification ID.
    ///     This is invoked when the client explicitly removes a notification (for example, by approving or ignoring a friend request).
    ///     Only the target's notification is persisted, so the account is always the target of the matching friend request.
    ///     Returns <see langword="true"/> if a matching friend request was found and removed; otherwise <see langword="false"/>.
    /// </summary>
    public static async Task<bool> RemoveFriendRequestByNotificationID(this IDatabase distributedCacheStore, int accountID, int notificationID)
    {
        List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)> pendingRequests = await distributedCacheStore.GetPendingFriendRequestsForAccount(accountID);

        foreach ((int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt) pendingRequest in pendingRequests)
        {
            if (pendingRequest.NotificationID == notificationID)
            {
                await distributedCacheStore.RemoveFriendRequest(pendingRequest.RequesterAccountID, accountID);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Removes every pending friend request which targets the account, clearing the account's entire friend request inbox.
    ///     This is invoked when the client removes all notifications at once.
    /// </summary>
    public static async Task RemoveAllFriendRequestsForAccount(this IDatabase distributedCacheStore, int accountID)
    {
        List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)> pendingRequests = await distributedCacheStore.GetPendingFriendRequestsForAccount(accountID);

        foreach ((int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt) pendingRequest in pendingRequests)
        {
            await distributedCacheStore.RemoveFriendRequest(pendingRequest.RequesterAccountID, accountID);
        }
    }

    /// <summary>
    ///     Parses the cached value of a friend request into the target's notification ID and the creation timestamp.
    ///     Returns <see langword="null"/> when the value is absent or malformed.
    ///     A missing timestamp is tolerated and falls back to the current time.
    /// </summary>
    private static (int TargetNotificationID, DateTimeOffset CreatedAt)? ParseFriendRequestValue(RedisValue cachedValue)
    {
        if (cachedValue.IsNullOrEmpty)
        {
            return null;
        }

        string[] parts = cachedValue.ToString().Split(':');

        if (parts.Length < 1 || int.TryParse(parts[0], out int targetNotificationID) is false)
        {
            return null;
        }

        DateTimeOffset createdAt = parts.Length >= 2 && long.TryParse(parts[1], out long unixSeconds)
            ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            : DateTimeOffset.UtcNow;

        return (targetNotificationID, createdAt);
    }
}
