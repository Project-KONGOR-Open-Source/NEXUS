namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    private static string ConstructFriendRequestKey(int requesterID, int targetID) => $@"FRIEND-REQUEST:[""{requesterID}:{targetID}""]";

    /// <summary>
    ///     Constructs the key of the per-account index which lists every pending friend request that the account is involved in, as either the requester or the target.
    ///     The index allows an account's pending requests to be enumerated without scanning the entire key space.
    /// </summary>
    private static string ConstructFriendRequestIndexKey(int accountID) => $@"FRIEND-REQUEST-INDEX:[""{accountID}""]";

    /// <summary>
    ///     Constructs the index member which identifies a single friend request by its requester and target.
    /// </summary>
    private static string ConstructFriendRequestIndexMember(int requesterID, int targetID) => $"{requesterID}:{targetID}";

    /// <summary>
    ///     Stores a pending friend request in the distributed cache store.
    ///     The value of this entry is the requester's and target's notification IDs, along with the creation timestamp.
    ///     The request is also added to both accounts' indices so that it can be re-delivered when either account logs in.
    /// </summary>
    public static async Task SetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID, int requesterNotificationID, int targetNotificationID)
    {
        string value = $"{requesterNotificationID}:{targetNotificationID}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        await distributedCacheStore.StringSetAsync(ConstructFriendRequestKey(requesterID, targetID), value, TimeSpan.FromDays(7));

        string indexMember = ConstructFriendRequestIndexMember(requesterID, targetID);

        await distributedCacheStore.SetAddAsync(ConstructFriendRequestIndexKey(requesterID), indexMember);
        await distributedCacheStore.SetAddAsync(ConstructFriendRequestIndexKey(targetID), indexMember);
    }

    /// <summary>
    ///     Retrieves a pending friend request from the distributed cache store.
    ///     Returns a tuple containing the requester's and target's notification IDs, or <see langword="null"/> if the request doesn't exist or has expired.
    /// </summary>
    public static async Task<(int RequesterNotificationID, int TargetNotificationID)?> GetFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructFriendRequestKey(requesterID, targetID));

        (int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt)? parsedValue = ParseFriendRequestValue(cachedValue);

        if (parsedValue is null)
        {
            return null;
        }

        return (parsedValue.Value.RequesterNotificationID, parsedValue.Value.TargetNotificationID);
    }

    /// <summary>
    ///     Removes a friend request from the distributed cache store, usually because it has been accepted or declined.
    ///     The request is removed from both accounts' indices as well.
    /// </summary>
    public static async Task RemoveFriendRequest(this IDatabase distributedCacheStore, int requesterID, int targetID)
    {
        await distributedCacheStore.KeyDeleteAsync(ConstructFriendRequestKey(requesterID, targetID));

        string indexMember = ConstructFriendRequestIndexMember(requesterID, targetID);

        await distributedCacheStore.SetRemoveAsync(ConstructFriendRequestIndexKey(requesterID), indexMember);
        await distributedCacheStore.SetRemoveAsync(ConstructFriendRequestIndexKey(targetID), indexMember);
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
    ///     Retrieves the pending friend requests for which the account is the target, so that they can be re-delivered as notifications when the account logs in.
    ///     Each entry contains the requester's account ID, the target's notification ID, and the time at which the request was made.
    /// </summary>
    public static async Task<List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)>> GetPendingFriendRequestsForAccount(this IDatabase distributedCacheStore, int accountID)
    {
        List<(int RequesterID, int TargetID, int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt)> requests = await GetFriendRequestsInvolvingAccount(distributedCacheStore, accountID);

        return requests
            .Where(request => request.TargetID == accountID)
            .Select(request => (request.RequesterID, request.TargetNotificationID, request.CreatedAt))
            .ToList();
    }

    /// <summary>
    ///     Removes the pending friend request whose notification ID, as owned by the account, matches the supplied notification ID.
    ///     This is invoked when the client explicitly removes a notification (for example, by approving or ignoring a friend request).
    ///     Returns <see langword="true"/> if a matching request was found and removed; otherwise <see langword="false"/>.
    /// </summary>
    public static async Task<bool> RemoveFriendRequestByNotificationID(this IDatabase distributedCacheStore, int accountID, int notificationID)
    {
        List<(int RequesterID, int TargetID, int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt)> requests = await GetFriendRequestsInvolvingAccount(distributedCacheStore, accountID);

        foreach ((int RequesterID, int TargetID, int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt) request in requests)
        {
            // The Account Owns The Requester Notification ID When It Is The Requester, And The Target Notification ID When It Is The Target
            bool ownsAsRequester = request.RequesterID == accountID && request.RequesterNotificationID == notificationID;
            bool ownsAsTarget = request.TargetID == accountID && request.TargetNotificationID == notificationID;

            if (ownsAsRequester || ownsAsTarget)
            {
                await RemoveFriendRequest(distributedCacheStore, request.RequesterID, request.TargetID);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Enumerates every pending friend request that the account is involved in, as either the requester or the target.
    ///     Index members which point at expired or malformed requests are removed as they are encountered, so that the index heals itself over time.
    /// </summary>
    private static async Task<List<(int RequesterID, int TargetID, int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt)>> GetFriendRequestsInvolvingAccount(IDatabase distributedCacheStore, int accountID)
    {
        string indexKey = ConstructFriendRequestIndexKey(accountID);

        RedisValue[] indexMembers = await distributedCacheStore.SetMembersAsync(indexKey);

        List<(int RequesterID, int TargetID, int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt)> requests = [];

        foreach (RedisValue indexMember in indexMembers)
        {
            string[] accountIDs = indexMember.ToString().Split(':');

            if (accountIDs.Length is not 2 || int.TryParse(accountIDs[0], out int requesterID) is false || int.TryParse(accountIDs[1], out int targetID) is false)
            {
                await distributedCacheStore.SetRemoveAsync(indexKey, indexMember);

                continue;
            }

            RedisValue cachedValue = await distributedCacheStore.StringGetAsync(ConstructFriendRequestKey(requesterID, targetID));

            (int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt)? parsedValue = ParseFriendRequestValue(cachedValue);

            if (parsedValue is null)
            {
                // The Request Has Expired Or Is Missing, So Heal The Index By Removing The Stale Member
                await distributedCacheStore.SetRemoveAsync(indexKey, indexMember);

                continue;
            }

            requests.Add((requesterID, targetID, parsedValue.Value.RequesterNotificationID, parsedValue.Value.TargetNotificationID, parsedValue.Value.CreatedAt));
        }

        return requests;
    }

    /// <summary>
    ///     Parses the cached value of a friend request into its notification IDs and creation timestamp.
    ///     Returns <see langword="null"/> when the value is absent or malformed.
    ///     A missing timestamp is tolerated and falls back to the current time, so that values written before timestamps were stored remain usable.
    /// </summary>
    private static (int RequesterNotificationID, int TargetNotificationID, DateTimeOffset CreatedAt)? ParseFriendRequestValue(RedisValue cachedValue)
    {
        if (cachedValue.IsNullOrEmpty)
        {
            return null;
        }

        string[] parts = cachedValue.ToString().Split(':');

        if (parts.Length < 2 || int.TryParse(parts[0], out int requesterNotificationID) is false || int.TryParse(parts[1], out int targetNotificationID) is false)
        {
            return null;
        }

        DateTimeOffset createdAt = parts.Length >= 3 && long.TryParse(parts[2], out long unixSeconds)
            ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            : DateTimeOffset.UtcNow;

        return (requesterNotificationID, targetNotificationID, createdAt);
    }
}
