namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class Notification
{
    /// <summary>
    ///     A pipe-separated set of notification data.
    ///     <br />
    ///     The format is:
    ///     "{SenderAccountName}|{Unknown}|{NotificationStatus}|{NotificationType}|{NotificationDisplayType}|{NotificationAction}|{NotificationTimestamp}|{NotificationID}".
    ///     <br />
    ///     The notification status can be either 0 = Removable, 1 = Not Seen, 2 = Seen. The other data points are exemplified
    ///     below.
    ///     <code>
    ///         Examples (the spaces are only added for readability, but they are not needed):
    ///             "KONGOR||23|notify_buddy_requested_added|notification_generic_action|action_friend_request|01/18 00:21 AM|5000001"
    ///             "KONGOR|| 2|notify_buddy_added          |notification_generic_info  |                     |01/18 00:22 AM|5000002"
    ///             "KONGOR|| 2|notify_buddy_requested_adder|notification_generic_info  |                     |01/18 00:23 AM|5000003"
    ///             "KONGOR|| 2|notify_replay_available     |notification_generic_info  |                     |01/18 00:24 AM|5000004"
    ///     </code>
    /// </summary>
    [PHPProperty("notification")]
    public required string PipeSeparatedNotificationData { get; set; }

    /// <summary>
    ///     The ID of the notification.
    ///     This value matches the last data point in the pipe-separated notification data set.
    /// </summary>
    [PHPProperty("notify_id")]
    public required string NotificationID { get; set; }
}
