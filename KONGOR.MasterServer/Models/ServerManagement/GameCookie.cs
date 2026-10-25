namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     The game cookie which the client connection request issues to an account for a match server.
///     A match server's duplicate session check compares the game cookie which it holds for an account with the account's stored one, so that a player who has since joined another game is removed from the match which they left.
/// </summary>
public class GameCookie
{
    public required string Cookie { get; set; }

    /// <summary>
    ///     The ID of the match server which the game cookie was issued for, or <see langword="null"/> when the session of the match server which asked was not a registered match server's.
    /// </summary>
    public required int? ServerID { get; set; }

    /// <summary>
    ///     The session cookie of the latest client connection which was served this game cookie, or which has since reported joining its match server.
    ///     The match server's duplicate session check is answered with it, so that the answer agrees with the session cookie which the client on the match server holds, and the client is never kicked as a duplicate login.
    /// </summary>
    public required string SessionCookie { get; set; }

    /// <summary>
    ///     The session cookie of the latest client connection which was served this game cookie, which a refresh never changes.
    ///     A session holding that session cookie is connected to a match server on its own, because a client which logs in again during its match is never served again, so that its report of joining a match server refreshes no other account of its user.
    ///     It is optional, so that a game cookie stored without it still reads, and counts as never served.
    /// </summary>
    public string? ServedSessionCookie { get; set; }
}
