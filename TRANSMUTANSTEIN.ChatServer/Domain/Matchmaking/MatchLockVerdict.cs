namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     The outcome of verifying a match lock before its player is sent back to the locked match.
///     Every verdict except <see cref="SendBack"/> and <see cref="WaitForSlot"/> lifts the lock, apart from <see cref="LiftSlotUnconfirmed"/> while a return waits for another machine and has sent nobody anywhere yet.
/// </summary>
public enum MatchLockVerdict
{
    /// <summary>
    ///     The match server still hosts the locked match and can still seat the player, so the player is sent back to it.
    /// </summary>
    SendBack,

    /// <summary>
    ///     The match server still hosts the locked match, but the slot account is still connected to it, so the player waits for that connection to leave before being sent back, and the lock is kept.
    ///     The match server ignores a second connection of an account which is still connected, so sending the player back any sooner would only send them into a connection which is ignored.
    /// </summary>
    WaitForSlot,

    /// <summary>
    ///     The match server is no longer registered, or has stopped reporting its status.
    /// </summary>
    LiftServerGone,

    /// <summary>
    ///     The match server no longer hosts the locked match.
    /// </summary>
    LiftMatchOver,

    /// <summary>
    ///     The match server reports that the player's slot can no longer be reclaimed.
    /// </summary>
    LiftSlotGone,

    /// <summary>
    ///     The match server did not answer whether the player's slot can be reclaimed, so the lock is lifted rather than trapping the player behind an outage.
    ///     A return which waits for another machine to leave the match keeps the lock instead, and waits on, because it has sent nobody anywhere, so nobody can be trapped by it.
    /// </summary>
    LiftSlotUnconfirmed
}
