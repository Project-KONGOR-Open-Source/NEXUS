namespace KONGOR.MasterServer.Constants;

/// <summary>
///     The notification types understood by the game client, mirroring its "ENotifyType" enumeration.
///     The numeric value is written as the type field of the pipe-separated notification data that the client parses.
/// </summary>
public enum ClientNotificationType
{
    BuddyAdded          = 2,  // A Friend Request Was Approved And The Accounts Are Now Friends
    BuddyRequestedAdder = 22, // Shown To The Account That Sent A Friend Request
    BuddyRequestedAdded = 23, // Shown To The Account That Received A Friend Request, And Is Approvable
    ReplayAvailable     = 26  // A Match Replay Is Available
}
