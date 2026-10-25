namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Records that the master server refused a locked user's account its server list, or the authentication of its client which a match server other than the locked one asked for, or which the locked one asked for when the account is not the locked account.
///     The master server publishes it for the chat server, which sends the player back to the locked match, or lifts the lock when the player can no longer return.
/// </summary>
public class MatchLockRefusal
{
    public required int UserID { get; set; }

    // The Bare Name Finds The Chat Session, Which Is Registered Under It, So It Carries No Clan Tag
    public required string AccountName { get; set; }
}
