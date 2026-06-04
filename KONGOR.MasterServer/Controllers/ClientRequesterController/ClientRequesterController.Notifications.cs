namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    /// <summary>
    ///     Handles the removal of a notification when it is explicitly actioned upon, such as by approving/declining or explicitly issuing an ignore action (as opposed to ignoring the notification by not actioning upon it).
    ///     The response is always a success, even when no backing entry is found, because the game client reuses notification IDs and may remove a notification whose backing entry has already been resolved.
    /// </summary>
    private async Task<IActionResult> DeleteNotification()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? notificationID = Request.Form["notify_id"];
        string? internalNotificationID = Request.Form["internal_id"];

        // Validate The Session Cookie And Resolve The Account, So The Removal Is Scoped To The Owner Of The Notification
        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Attempted To Remove A Notification With Invalid Cookie ""{cookie}""");

            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");
        }

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.Name.Equals(accountName));

        if (account is not null && int.TryParse(notificationID, out int parsedNotificationID))
        {
            bool removed = await RemovePersistedNotification(account.ID, parsedNotificationID);

            Logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Removed Notification With ID {NotificationID}; Persisted Entry Found: {Removed}",
                account.Name, account.ID, parsedNotificationID, removed);
        }

        Dictionary<string, string> response = new ()
        {
            { "notify_id", notificationID ?? "0" },
            { "internal_id", internalNotificationID ?? "0" },
            { "status", "OK" }
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Removes the persisted notification with the supplied ID for the account, dispatching to each persisted notification type in turn.
    ///     Returns <see langword="true"/> if a backing entry was found and removed; otherwise <see langword="false"/>.
    /// </summary>
    private async Task<bool> RemovePersistedNotification(int accountID, int notificationID)
    {
        if (await DistributedCache.RemoveFriendRequestByNotificationID(accountID, notificationID))
            return true;

        // TODO: Implement Removal Of Other Persisted Notification Types (e.g. Clan Invites)

        return false;
    }

    /// <summary>
    ///     Builds the persisted notifications to re-deliver to an account on login, so that any which were ignored or missed while the account was offline re-appear.
    /// </summary>
    private async Task<List<Notification>> BuildLoginNotifications(int accountID)
    {
        List<Notification> notifications = [];

        notifications.AddRange(await BuildFriendRequestNotifications(accountID));

        // TODO: Implement Building Of Other Persisted Notification Types (e.g. Clan Invites)

        if (notifications.Count > 0)
            Logger.LogInformation(@"Re-Delivered {NotificationCount} Notification(s) To Account With ID {AccountID} On Login", notifications.Count, accountID);

        return notifications;
    }

    /// <summary>
    ///     Builds the incoming friend request notifications to re-deliver to the account, each as an approvable notification.
    /// </summary>
    private async Task<List<Notification>> BuildFriendRequestNotifications(int accountID)
    {
        List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)> pendingRequests = await DistributedCache.GetPendingFriendRequestsForAccount(accountID);

        if (pendingRequests.Count is 0)
            return [];

        List<int> requesterAccountIDs = pendingRequests
            .Select(pendingRequest => pendingRequest.RequesterAccountID)
            .Distinct()
            .ToList();

        Dictionary<int, Account> requesterAccounts = await MerrickContext.Accounts
            .Include(account => account.Clan)
            .Where(account => requesterAccountIDs.Contains(account.ID))
            .ToDictionaryAsync(account => account.ID);

        List<Notification> notifications = [];

        foreach ((int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt) pendingRequest in pendingRequests)
        {
            Account? requesterAccount = requesterAccounts.GetValueOrDefault(pendingRequest.RequesterAccountID);

            if (requesterAccount is null)
                continue;

            notifications.Add(new Notification
            {
                NotificationID = pendingRequest.NotificationID.ToString(),
                PipeSeparatedNotificationData = FormatNotification(requesterAccount.NameWithClanTag, ClientNotificationType.BuddyRequestedAdded, pendingRequest.CreatedAt)
            });
        }

        return notifications;
    }

    // TODO: Implement Building Of Other Persisted Notification Types (e.g. Clan Invites)

    /// <summary>
    ///     Formats a notification as the pipe-separated string that the game client parses on login.
    ///     The client reads index 0 as the primary parameter (such as a sender name), index 2 as the numeric notification type, and index 6 as the timestamp. The remaining indices are placeholders that the client refills.
    ///     The notification ID is supplied separately to the client through the "notify_id" field.
    /// </summary>
    private static string FormatNotification(string parameter, ClientNotificationType notificationType, DateTimeOffset createdAt)
    {
        string timestamp = createdAt.ToString(@"MM/dd  hh:mm tt", CultureInfo.InvariantCulture);

        return $"{parameter}||{(int) notificationType}||||{timestamp}";
    }
}
