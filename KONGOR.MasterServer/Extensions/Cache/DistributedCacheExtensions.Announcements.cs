namespace KONGOR.MasterServer.Extensions.Cache;

public static partial class DistributedCacheExtensions
{
    /// <summary>
    ///     Constructs the key of an announcement of the Project KONGOR Announcement Board.
    /// </summary>
    internal static string ConstructAnnouncementKey(Guid id) => $@"ANNOUNCEMENT:[""{id}""]";

    /// <summary>
    ///     The key of the index which lists the announcements.
    /// </summary>
    internal const string AnnouncementsIndexKey = "ANNOUNCEMENTS";

    /// <summary>
    ///     Stores an announcement, replacing any announcement with the same identity; the announcement expires on its own at its expiry.
    ///     An announcement without a future expiry is refused with an <see cref="ArgumentException"/>.
    /// </summary>
    public static async Task SetAnnouncement(this IDatabase distributedCacheStore, Announcement announcement)
    {
        TimeSpan timeToLive = ExpiringItemTimeToLive(announcement.Expires, nameof(announcement));

        await SetExpiringItem(distributedCacheStore, ConstructAnnouncementKey(announcement.ID), AnnouncementsIndexKey, announcement.ID, JsonSerializer.Serialize(announcement), timeToLive);
    }

    public static async Task<Announcement?> GetAnnouncement(this IDatabase distributedCacheStore, Guid id)
        => await GetExpiringItem<Announcement>(distributedCacheStore, ConstructAnnouncementKey(id));

    /// <summary>
    ///     Lists every announcement which has not expired, scheduled ones included, in no particular order.
    /// </summary>
    public static async Task<List<Announcement>> GetAnnouncements(this IDatabase distributedCacheStore)
        => await GetExpiringItems<Announcement>(distributedCacheStore, AnnouncementsIndexKey, ConstructAnnouncementKey);

    /// <summary>
    ///     Removes an announcement, and returns whether it existed.
    /// </summary>
    public static async Task<bool> RemoveAnnouncement(this IDatabase distributedCacheStore, Guid id)
        => await RemoveExpiringItem(distributedCacheStore, ConstructAnnouncementKey(id), AnnouncementsIndexKey, id);
}
