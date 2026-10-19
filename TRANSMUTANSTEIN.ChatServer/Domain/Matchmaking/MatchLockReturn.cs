namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     A locked player's return to their match in progress, which a chat session holds from the moment the player is sent back until the player reaches the locked match server, the lock lifts, the session ends, or the return expires.
///     While it is pending, further attempts to play another game are refused without starting another return.
/// </summary>
public sealed class MatchLockReturn(int matchID, DateTimeOffset expires)
{
    /// <summary>
    ///     The ID of the locked match which the player is being sent back to.
    /// </summary>
    public int MatchID { get; } = matchID;

    /// <summary>
    ///     The moment after which the player is no longer reminded to return.
    /// </summary>
    public DateTimeOffset Expires { get; } = expires;

    /// <summary>
    ///     The moment at which the match server last confirmed that the player can still return.
    /// </summary>
    public DateTimeOffset LastVerified { get; set; }

    /// <summary>
    ///     The moment at which the player was last told why another game is refused.
    /// </summary>
    public DateTimeOffset LastNotified { get; set; }

    /// <summary>
    ///     Cancelled when the return ends, which stops its reminders.
    ///     It is never disposed, because it holds no timer, and because the return may be cancelled from any thread at any time.
    /// </summary>
    public CancellationTokenSource Cancellation { get; } = new ();

    /// <summary>
    ///     Ends the return, which stops its reminders.
    /// </summary>
    public void Cancel() => Cancellation.Cancel();
}
