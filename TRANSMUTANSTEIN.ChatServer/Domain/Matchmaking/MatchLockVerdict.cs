namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     The outcome of verifying a match lock before its player is sent back to the locked match.
///     Every verdict except <see cref="SendBack"/> lifts the lock.
/// </summary>
public enum MatchLockVerdict
{
    /// <summary>
    ///     The match server still hosts the locked match and can still seat the player, so the player is sent back to it.
    /// </summary>
    SendBack,

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
    /// </summary>
    LiftSlotUnconfirmed
}
