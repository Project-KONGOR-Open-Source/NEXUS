namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Records that a match server other than the locked one asked to authenticate the client of a locked user's account, and that the master server refused it.
///     The master server publishes it for the chat server, which sends the player back to the locked match, or lifts the lock when the player can no longer return.
/// </summary>
public class MatchLockRefusal
{
    public required int UserID { get; set; }

    public required string AccountName { get; set; }
}
