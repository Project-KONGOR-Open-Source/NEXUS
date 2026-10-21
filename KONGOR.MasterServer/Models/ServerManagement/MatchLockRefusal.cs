namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Records that the master server refused a locked user's account its server list, or the authentication of its client which a match server other than the locked one asked for.
///     The master server publishes it for the chat server, which sends the player back to the locked match, or lifts the lock when the player can no longer return.
/// </summary>
public class MatchLockRefusal
{
    public required int UserID { get; set; }

    public required string AccountName { get; set; }
}
